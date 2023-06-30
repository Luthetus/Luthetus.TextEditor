using System.Collections.Immutable;
using Luthetus.Common.RazorLib.Misc;
using Luthetus.TextEditor.RazorLib.Analysis.GenericLexer;
using Luthetus.TextEditor.RazorLib.Analysis.GenericLexer.SyntaxActors;
using Luthetus.TextEditor.RazorLib.Analysis.TypeScript.Facts;
using Luthetus.TextEditor.RazorLib.Lexing;

namespace Luthetus.TextEditor.RazorLib.Analysis.TypeScript.SyntaxActors;

public class TextEditorTypeScriptLexer
{
    public static readonly GenericPreprocessorDefinition TypeScriptPreprocessorDefinition = new(
        "#",
        ImmutableArray<DeliminationExtendedSyntaxDefinition>.Empty);

    public static readonly GenericLanguageDefinition TypeScriptLanguageDefinition = new GenericLanguageDefinition(
        "\"",
        "\"",
        "(",
        ")",
        ".",
        "//",
        new[]
        {
            WhitespaceFacts.CARRIAGE_RETURN.ToString(),
            WhitespaceFacts.LINE_FEED.ToString()
        }.ToImmutableArray(),
        "/*",
        "*/",
        TypeScriptKeywords.ALL,
        TypeScriptPreprocessorDefinition);

    private readonly GenericSyntaxTree _typeScriptSyntaxTree;

    public TextEditorTypeScriptLexer(ResourceUri resourceUri)
    {
        _typeScriptSyntaxTree = new GenericSyntaxTree(TypeScriptLanguageDefinition);
        ResourceUri = resourceUri;
    }

    public RenderStateKey ModelRenderStateKey { get; private set; } = RenderStateKey.Empty;
    
    public ResourceUri ResourceUri { get; }

    public Task<ImmutableArray<TextEditorTextSpan>> Lex(
        string sourceText,
        RenderStateKey modelRenderStateKey)
    {
        var typeScriptSyntaxUnit = _typeScriptSyntaxTree.ParseText(
            ResourceUri,
            sourceText);

        var typeScriptSyntaxWalker = new GenericSyntaxWalker();

        typeScriptSyntaxWalker.Visit(typeScriptSyntaxUnit.GenericDocumentSyntax);

        var textEditorTextSpans = new List<TextEditorTextSpan>();

        textEditorTextSpans
            .AddRange(typeScriptSyntaxWalker.GenericStringSyntaxes
                .Select(x => x.TextEditorTextSpan));

        textEditorTextSpans
            .AddRange(typeScriptSyntaxWalker.GenericCommentSingleLineSyntaxes
                .Select(x => x.TextEditorTextSpan));

        textEditorTextSpans
            .AddRange(typeScriptSyntaxWalker.GenericCommentMultiLineSyntaxes
                .Select(x => x.TextEditorTextSpan));

        textEditorTextSpans
            .AddRange(typeScriptSyntaxWalker.GenericKeywordSyntaxes
                .Select(x => x.TextEditorTextSpan));

        textEditorTextSpans
            .AddRange(typeScriptSyntaxWalker.GenericFunctionSyntaxes
                .Select(x => x.TextEditorTextSpan));

        return Task.FromResult(textEditorTextSpans.ToImmutableArray());
    }
}