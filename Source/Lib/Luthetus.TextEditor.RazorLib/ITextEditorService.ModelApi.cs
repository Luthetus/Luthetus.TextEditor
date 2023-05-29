using Fluxor;
using Luthetus.TextEditor.RazorLib.Analysis.CSharp.SyntaxActors;
using Luthetus.TextEditor.RazorLib.Analysis.Css.Decoration;
using Luthetus.TextEditor.RazorLib.Analysis.Css.SyntaxActors;
using Luthetus.TextEditor.RazorLib.Analysis.FSharp.SyntaxActors;
using Luthetus.TextEditor.RazorLib.Analysis.GenericLexer.Decoration;
using Luthetus.TextEditor.RazorLib.Analysis.Html.Decoration;
using Luthetus.TextEditor.RazorLib.Analysis.Html.SyntaxActors;
using Luthetus.TextEditor.RazorLib.Analysis.JavaScript.SyntaxActors;
using Luthetus.TextEditor.RazorLib.Analysis.Json.Decoration;
using Luthetus.TextEditor.RazorLib.Analysis.Json.SyntaxActors;
using Luthetus.TextEditor.RazorLib.Analysis.Razor.SyntaxActors;
using Luthetus.TextEditor.RazorLib.Analysis.TypeScript.SyntaxActors;
using Luthetus.TextEditor.RazorLib.Model;
using Luthetus.TextEditor.RazorLib.Store.Model;
using Luthetus.TextEditor.RazorLib.Decoration;
using Luthetus.TextEditor.RazorLib.Lexing;
using Luthetus.TextEditor.RazorLib.Row;
using Luthetus.TextEditor.RazorLib.ViewModel;
using System.Collections.Immutable;

namespace Luthetus.TextEditor.RazorLib;

public partial interface ITextEditorService
{
    public interface IModelApi
    {
        public void DeleteTextByMotion(TextEditorModelsCollection.DeleteTextByMotionAction deleteTextByMotionAction);
        public void DeleteTextByRange(TextEditorModelsCollection.DeleteTextByRangeAction deleteTextByRangeAction);
        public void Dispose(TextEditorModelKey textEditorModelKey);
        public TextEditorModel? FindOrDefault(TextEditorModelKey textEditorModelKey);
        public string? GetAllText(TextEditorModelKey textEditorModelKey);
        public ImmutableArray<TextEditorViewModel> GetViewModelsOrEmpty(TextEditorModelKey textEditorModelKey);
        public void HandleKeyboardEvent(TextEditorModelsCollection.KeyboardEventAction keyboardEventAction);
        public void InsertText(TextEditorModelsCollection.InsertTextAction insertTextAction);
        public void RedoEdit(TextEditorModelKey textEditorModelKey);
        /// <summary>It is recommended to use the <see cref="RegisterTemplated" /> method as it will internally reference the <see cref="ITextEditorLexer" /> and <see cref="IDecorationMapper" /> that correspond to the desired text editor.</summary>
        public void RegisterCustom(TextEditorModel model);
        /// <summary>As an example, for a C# Text Editor one would pass in a <see cref="WellKnownModelKind" /> of <see cref="WellKnownModelKind.CSharp" />.<br /><br />For a Plain Text Editor one would pass in a <see cref="WellKnownModelKind" /> of <see cref="WellKnownModelKind.Plain" />.</summary>
        public void RegisterTemplated(TextEditorModelKey textEditorModelKey, WellKnownModelKind wellKnownModelKind, ResourceUri resourceUri, DateTime resourceLastWriteTime, string fileExtension, string initialContent);
        public void Reload(TextEditorModelKey textEditorModelKey, string content, DateTime resourceLastWriteTime);
        public void SetResourceData(TextEditorModelKey textEditorModelKey, ResourceUri resourceUri, DateTime resourceLastWriteTime);
        public void SetUsingRowEndingKind(TextEditorModelKey textEditorModelKey, RowEndingKind rowEndingKind);
        public void UndoEdit(TextEditorModelKey textEditorModelKey);
        public TextEditorModel? FindOrDefaultByResourceUri(ResourceUri resourceUri);
    }

    public class ModelApi : IModelApi
    {
        private readonly ITextEditorService _textEditorService;
        private readonly IDispatcher _dispatcher;
        private readonly LuthetusTextEditorOptions _luthetusTextEditorOptions;

        public ModelApi(
            IDispatcher dispatcher,
            LuthetusTextEditorOptions luthetusTextEditorOptions,
            ITextEditorService textEditorService)
        {
            _dispatcher = dispatcher;
            _luthetusTextEditorOptions = luthetusTextEditorOptions;
            _textEditorService = textEditorService;
        }

        public TextEditorModel? FindOrDefaultByResourceUri(
            ResourceUri resourceUri)
        {
            return _textEditorService.ModelsCollectionWrap.Value.TextEditorList
                .FirstOrDefault(x =>
                    x.ResourceUri == resourceUri);
        }

        public void UndoEdit(
            TextEditorModelKey textEditorModelKey)
        {
            var undoEditAction = new TextEditorModelsCollection.UndoEditAction(
                textEditorModelKey);

            _dispatcher.Dispatch(undoEditAction);
        }

        public void SetUsingRowEndingKind(
            TextEditorModelKey textEditorModelKey,
            RowEndingKind rowEndingKind)
        {
            _dispatcher.Dispatch(
                new TextEditorModelsCollection.SetUsingRowEndingKindAction(
                    textEditorModelKey,
                    rowEndingKind));
        }

        public void SetResourceData(
            TextEditorModelKey textEditorModelKey,
            ResourceUri resourceUri,
            DateTime resourceLastWriteTime)
        {
            _dispatcher.Dispatch(
                new TextEditorModelsCollection.SetResourceDataAction(
                    textEditorModelKey,
                    resourceUri,
                    resourceLastWriteTime));
        }

        public void Reload(
            TextEditorModelKey textEditorModelKey,
            string content,
            DateTime resourceLastWriteTime)
        {
            _dispatcher.Dispatch(
                new TextEditorModelsCollection.ReloadAction(
                    textEditorModelKey,
                    content,
                    resourceLastWriteTime));
        }

        public void RegisterTemplated(
            TextEditorModelKey textEditorModelKey,
            WellKnownModelKind wellKnownModelKind,
            ResourceUri resourceUri,
            DateTime resourceLastWriteTime,
            string fileExtension,
            string initialContent)
        {
            ITextEditorLexer? lexer = null;
            IDecorationMapper? decorationMapper = null;

            switch (wellKnownModelKind)
            {
                case WellKnownModelKind.CSharp:
                    lexer = new TextEditorCSharpLexer(resourceUri);
                    decorationMapper = new GenericDecorationMapper();
                    break;
                case WellKnownModelKind.Html:
                    lexer = new TextEditorHtmlLexer(resourceUri);
                    decorationMapper = new TextEditorHtmlDecorationMapper();
                    break;
                case WellKnownModelKind.Css:
                    lexer = new TextEditorCssLexer(resourceUri);
                    decorationMapper = new TextEditorCssDecorationMapper();
                    break;
                case WellKnownModelKind.Json:
                    lexer = new TextEditorJsonLexer(resourceUri);
                    decorationMapper = new TextEditorJsonDecorationMapper();
                    break;
                case WellKnownModelKind.FSharp:
                    lexer = new TextEditorFSharpLexer(resourceUri);
                    decorationMapper = new GenericDecorationMapper();
                    break;
                case WellKnownModelKind.Razor:
                    lexer = new TextEditorRazorLexer(resourceUri);
                    decorationMapper = new TextEditorHtmlDecorationMapper();
                    break;
                case WellKnownModelKind.JavaScript:
                    lexer = new TextEditorJavaScriptLexer(resourceUri);
                    decorationMapper = new GenericDecorationMapper();
                    break;
                case WellKnownModelKind.TypeScript:
                    lexer = new TextEditorTypeScriptLexer(resourceUri);
                    decorationMapper = new GenericDecorationMapper();
                    break;
            }

            var textEditorModel = new TextEditorModel(
                resourceUri,
                resourceLastWriteTime,
                fileExtension,
                initialContent,
                lexer,
                decorationMapper,
                null,
                null,
                textEditorModelKey);

            // IBackgroundTaskQueue does not work well here because
            // this Task does not need to be tracked.
            _ = Task.Run(async () =>
            {
                try
                {
                    await textEditorModel.ApplySyntaxHighlightingAsync();

                    _dispatcher.Dispatch(
                        new TextEditorModelsCollection.ForceRerenderAction(
                            textEditorModel.ModelKey));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }, CancellationToken.None);

            _dispatcher.Dispatch(
                new TextEditorModelsCollection.RegisterAction(
                    textEditorModel));
        }

        public void RegisterCustom(
            TextEditorModel model)
        {
            _dispatcher.Dispatch(
                new TextEditorModelsCollection.RegisterAction(
                    model));
        }

        public void RedoEdit(
            TextEditorModelKey textEditorModelKey)
        {
            var redoEditAction = new TextEditorModelsCollection.RedoEditAction(
                textEditorModelKey);

            _dispatcher.Dispatch(redoEditAction);
        }

        public void InsertText(
            TextEditorModelsCollection.InsertTextAction insertTextAction)
        {
            _dispatcher.Dispatch(insertTextAction);
        }

        public void HandleKeyboardEvent(
            TextEditorModelsCollection.KeyboardEventAction keyboardEventAction)
        {
            _dispatcher.Dispatch(keyboardEventAction);
        }

        public ImmutableArray<TextEditorViewModel> GetViewModelsOrEmpty(
            TextEditorModelKey textEditorModelKey)
        {
            return _textEditorService.ViewModelsCollectionWrap.Value.ViewModelsList
                .Where(x => x.ModelKey == textEditorModelKey)
                .ToImmutableArray();
        }

        public string? GetAllText(
            TextEditorModelKey textEditorModelKey)
        {
            return _textEditorService.ModelsCollectionWrap.Value.TextEditorList
                .FirstOrDefault(x => x.ModelKey == textEditorModelKey)
                ?.GetAllText();
        }

        public TextEditorModel? FindOrDefault(
            TextEditorModelKey textEditorModelKey)
        {
            return _textEditorService.ModelsCollectionWrap.Value.TextEditorList
                .FirstOrDefault(x => x.ModelKey == textEditorModelKey);
        }

        public void Dispose(
            TextEditorModelKey textEditorModelKey)
        {
            _dispatcher.Dispatch(
                new TextEditorModelsCollection.DisposeAction(
                    textEditorModelKey));
        }

        public void DeleteTextByRange(
            TextEditorModelsCollection.DeleteTextByRangeAction deleteTextByRangeAction)
        {
            _dispatcher.Dispatch(deleteTextByRangeAction);
        }

        public void DeleteTextByMotion(
            TextEditorModelsCollection.DeleteTextByMotionAction deleteTextByMotionAction)
        {
            _dispatcher.Dispatch(deleteTextByMotionAction);
        }
    }
}
