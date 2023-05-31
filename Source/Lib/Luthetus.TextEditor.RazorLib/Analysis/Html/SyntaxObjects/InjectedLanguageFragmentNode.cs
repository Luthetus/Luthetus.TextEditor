using System.Collections.Immutable;
using Luthetus.TextEditor.RazorLib.Analysis.Html.SyntaxEnums;
using Luthetus.TextEditor.RazorLib.Lexing;

namespace Luthetus.TextEditor.RazorLib.Analysis.Html.SyntaxObjects;

public class InjectedLanguageFragmentNode : IHtmlSyntaxNode
{
    public InjectedLanguageFragmentNode(
        ImmutableArray<IHtmlSyntax> childHtmlSyntaxes,
        TextEditorTextSpan textEditorTextSpan)
    {
        TextEditorTextSpan = textEditorTextSpan;

        Children = childHtmlSyntaxes;
    }

    public TextEditorTextSpan TextEditorTextSpan { get; }
    public ImmutableArray<IHtmlSyntax> Children { get; }

    public HtmlSyntaxKind HtmlSyntaxKind => HtmlSyntaxKind.InjectedLanguageFragmentNode;
}