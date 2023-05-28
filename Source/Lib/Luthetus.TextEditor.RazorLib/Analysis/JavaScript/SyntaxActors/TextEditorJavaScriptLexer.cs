using System.Collections.Immutable;
using Luthetus.Common.RazorLib.Misc;
using Luthetus.TextEditor.RazorLib.Analysis.GenericLexer;
using Luthetus.TextEditor.RazorLib.Analysis.GenericLexer.SyntaxActors;
using Luthetus.TextEditor.RazorLib.Analysis.JavaScript.Facts;
using Luthetus.TextEditor.RazorLib.Lexing;

namespace Luthetus.TextEditor.RazorLib.Analysis.JavaScript.SyntaxActors;

public class TextEditorJavaScriptLexer : ITextEditorLexer
{
    public static readonly GenericPreprocessorDefinition JavaScriptPreprocessorDefinition = new(
        "#",
        ImmutableArray<DeliminationExtendedSyntaxDefinition>.Empty);

    public static readonly GenericLanguageDefinition JavaScriptLanguageDefinition = new GenericLanguageDefinition(
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
        JavaScriptKeywords.ALL,
        JavaScriptPreprocessorDefinition);

    private readonly GenericSyntaxTree _javaScriptSyntaxTree;

    public TextEditorJavaScriptLexer(string resourceUri)
    {
        _javaScriptSyntaxTree = new GenericSyntaxTree(JavaScriptLanguageDefinition);
        ResourceUri = resourceUri;
    }

    public RenderStateKey ModelRenderStateKey { get; private set; } = RenderStateKey.Empty;

    public string ResourceUri { get; }

    public Task<ImmutableArray<TextEditorTextSpan>> Lex(
        string text,
        RenderStateKey modelRenderStateKey)
    {
        var javaScriptSyntaxUnit = _javaScriptSyntaxTree
            .ParseText(text);

        var javaScriptSyntaxWalker = new GenericSyntaxWalker();

        javaScriptSyntaxWalker.Visit(javaScriptSyntaxUnit.GenericDocumentSyntax);

        var textEditorTextSpans = new List<TextEditorTextSpan>();

        textEditorTextSpans
            .AddRange(javaScriptSyntaxWalker.GenericStringSyntaxes
                .Select(x => x.TextEditorTextSpan));

        textEditorTextSpans
            .AddRange(javaScriptSyntaxWalker.GenericCommentSingleLineSyntaxes
                .Select(x => x.TextEditorTextSpan));

        textEditorTextSpans
            .AddRange(javaScriptSyntaxWalker.GenericCommentMultiLineSyntaxes
                .Select(x => x.TextEditorTextSpan));

        textEditorTextSpans
            .AddRange(javaScriptSyntaxWalker.GenericKeywordSyntaxes
                .Select(x => x.TextEditorTextSpan));

        textEditorTextSpans
            .AddRange(javaScriptSyntaxWalker.GenericFunctionSyntaxes
                .Select(x => x.TextEditorTextSpan));

        return Task.FromResult(textEditorTextSpans.ToImmutableArray());
    }
}