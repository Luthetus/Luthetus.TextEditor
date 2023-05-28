using System.Collections.Immutable;
using Luthetus.Common.RazorLib.Misc;
using Luthetus.TextEditor.RazorLib.Lexing;

namespace Luthetus.TextEditor.RazorLib.Analysis.Css.SyntaxActors;

public class TextEditorCssLexer : ITextEditorLexer
{
    public TextEditorCssLexer(ResourceUri resourceUri)
    {
        ResourceUri = resourceUri;
    }

    public RenderStateKey ModelRenderStateKey { get; private set; } = RenderStateKey.Empty;
 
    public ResourceUri ResourceUri { get; }

    public Task<ImmutableArray<TextEditorTextSpan>> Lex(
        string sourceText,
        RenderStateKey modelRenderStateKey)
    {
        var cssSyntaxUnit = CssSyntaxTree.ParseText(
            ResourceUri,
            sourceText);

        var syntaxNodeRoot = cssSyntaxUnit.CssDocumentSyntax;

        var cssSyntaxWalker = new CssSyntaxWalker();

        cssSyntaxWalker.Visit(syntaxNodeRoot);

        List<TextEditorTextSpan> textEditorTextSpans = new();

        textEditorTextSpans.AddRange(
            cssSyntaxWalker.CssIdentifierSyntaxes.Select(identifier =>
                identifier.TextEditorTextSpan));

        textEditorTextSpans.AddRange(
            cssSyntaxWalker.CssCommentSyntaxes.Select(comment =>
                comment.TextEditorTextSpan));

        textEditorTextSpans.AddRange(
            cssSyntaxWalker.CssPropertyNameSyntaxes.Select(propertyName =>
                propertyName.TextEditorTextSpan));

        textEditorTextSpans.AddRange(
            cssSyntaxWalker.CssPropertyValueSyntaxes.Select(propertyValue =>
                propertyValue.TextEditorTextSpan));

        return Task.FromResult(textEditorTextSpans.ToImmutableArray());
    }
}