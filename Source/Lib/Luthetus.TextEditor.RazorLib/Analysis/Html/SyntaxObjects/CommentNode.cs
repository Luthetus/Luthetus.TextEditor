using System.Collections.Immutable;
using Luthetus.TextEditor.RazorLib.Analysis.Html.SyntaxEnums;
using Luthetus.TextEditor.RazorLib.Lexing;

namespace Luthetus.TextEditor.RazorLib.Analysis.Html.SyntaxObjects;

public class CommentNode : IHtmlSyntaxNode
{
    public CommentNode(
        TextEditorTextSpan textEditorTextSpan)
    {
        TextEditorTextSpan = textEditorTextSpan;

        Children = ImmutableArray<IHtmlSyntax>.Empty;
    }

    public TextEditorTextSpan TextEditorTextSpan { get; }
    public ImmutableArray<IHtmlSyntax> Children { get; }

    public HtmlSyntaxKind HtmlSyntaxKind => HtmlSyntaxKind.CommentNode;
}