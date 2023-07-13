using Luthetus.Common.RazorLib.BackgroundTaskCase.BaseTypes;
using Luthetus.TextEditor.RazorLib.Analysis.Css.SyntaxActors;
using Luthetus.TextEditor.RazorLib.HostedServiceCase.CompilerServiceCase;
using Luthetus.TextEditor.RazorLib.Lexing;
using Luthetus.TextEditor.RazorLib.Model;
using System.Collections.Immutable;

namespace Luthetus.TextEditor.RazorLib.CompilerServiceCase.CssCase;

public class TextEditorCssCompilerService : ICompilerService
{
    private readonly Dictionary<TextEditorModelKey, CssResource> _cssResourceMap = new();
    private readonly object _cssResourceMapLock = new();
    private readonly ICompilerServiceBackgroundTaskQueue _compilerServiceBackgroundTaskQueue;

    public TextEditorCssCompilerService(ICompilerServiceBackgroundTaskQueue compilerServiceBackgroundTaskQueue)
    {
        _compilerServiceBackgroundTaskQueue = compilerServiceBackgroundTaskQueue;
    }

    public void RegisterModel(TextEditorModel model)
    {
        lock (_cssResourceMapLock)
        {
            if (_cssResourceMap.ContainsKey(model.ModelKey))
                return;

            _cssResourceMap.Add(
                model.ModelKey,
                new(model.ModelKey, model.ResourceUri, this));

            QueueParseRequest(model);
        }
    }

    public ImmutableArray<TextEditorTextSpan> GetSyntacticTextSpansFor(TextEditorModel model)
    {
        lock (_cssResourceMapLock)
        {
            if (!_cssResourceMap.ContainsKey(model.ModelKey))
                return ImmutableArray<TextEditorTextSpan>.Empty;

            return _cssResourceMap[model.ModelKey].SyntacticTextSpans ??
                ImmutableArray<TextEditorTextSpan>.Empty;
        }
    }

    public ImmutableArray<ITextEditorSymbol> GetSymbolsFor(TextEditorModel textEditorModel)
    {
        return ImmutableArray<ITextEditorSymbol>.Empty;
    }

    public ImmutableArray<TextEditorTextSpan> GetDiagnosticTextSpansFor(TextEditorModel textEditorModel)
    {
        return ImmutableArray<TextEditorTextSpan>.Empty;
    }

    public void ModelWasModified(TextEditorModel textEditorModel, ImmutableArray<TextEditorTextSpan> editTextSpans)
    {
        // Do nothing
    }

    public void DisposeModel(TextEditorModel textEditorModel)
    {
        lock (_cssResourceMapLock)
        {
            _cssResourceMap.Remove(textEditorModel.ModelKey);
        }
    }

    private void QueueParseRequest(TextEditorModel model)
    {
        var parseBackgroundWorkItem = new BackgroundTask(
            async cancellationToken =>
            {
                var text = model.GetAllText();

                var lexer = new TextEditorCssLexer(model.ResourceUri);
                var lexResult = await lexer.Lex(text, model.RenderStateKey);

                lock (_cssResourceMapLock)
                {
                    if (!_cssResourceMap.ContainsKey(model.ModelKey))
                        return;

                    _cssResourceMap[model.ModelKey]
                        .SyntacticTextSpans = lexResult;
                }

                await model.ApplySyntaxHighlightingAsync();

                return;
            },
            "TODO: name",
            "TODO: description",
            false,
            _ => Task.CompletedTask,
            null,
            CancellationToken.None);

        _compilerServiceBackgroundTaskQueue.QueueBackgroundWorkItem(parseBackgroundWorkItem);
    }
}