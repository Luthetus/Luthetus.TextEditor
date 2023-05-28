using System.Collections.Immutable;
using Luthetus.Common.RazorLib.Misc;
using Luthetus.TextEditor.RazorLib.Lexing;

namespace Luthetus.TextEditor.RazorLib.Analysis.Html.SyntaxActors;

public class TextEditorHtmlLexer : ITextEditorLexer
{
    public TextEditorHtmlLexer(ResourceUri resourceUri)
    {
        ResourceUri = resourceUri;
    }

    public RenderStateKey ModelRenderStateKey { get; private set; } = RenderStateKey.Empty;

    public ResourceUri ResourceUri { get; }

    public Task<ImmutableArray<TextEditorTextSpan>> Lex(
        string sourceText,
        RenderStateKey modelRenderStateKey)
    {
        var htmlSyntaxUnit = HtmlSyntaxTree.ParseText(
            ResourceUri,
            sourceText);

        var syntaxNodeRoot = htmlSyntaxUnit.RootTagSyntax;

        var htmlSyntaxWalker = new HtmlSyntaxWalker();

        htmlSyntaxWalker.Visit(syntaxNodeRoot);

        List<TextEditorTextSpan> textEditorTextSpans = new();

        // Tag Names
        {
            textEditorTextSpans.AddRange(htmlSyntaxWalker.TagNameSyntaxes
                .Select(tn => tn.TextEditorTextSpan));
        }

        // InjectedLanguageFragmentSyntaxes
        {
            textEditorTextSpans.AddRange(htmlSyntaxWalker.InjectedLanguageFragmentSyntaxes
                .Select(ilf => ilf.TextEditorTextSpan));
        }

        // Attribute Names
        {
            textEditorTextSpans.AddRange(htmlSyntaxWalker.AttributeNameSyntaxes
                .Select(an => an.TextEditorTextSpan));
        }

        // Attribute Values
        {
            textEditorTextSpans.AddRange(htmlSyntaxWalker.AttributeValueSyntaxes
                .Select(av => av.TextEditorTextSpan));
        }

        // Comments
        {
            textEditorTextSpans.AddRange(htmlSyntaxWalker.CommentSyntaxes
                .Select(c => c.TextEditorTextSpan));
        }

        return Task.FromResult(textEditorTextSpans.ToImmutableArray());
    }
}