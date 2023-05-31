using Luthetus.TextEditor.RazorLib.Analysis.Html.SyntaxEnums;

namespace Luthetus.TextEditor.RazorLib.Analysis.Html;

public interface IHtmlSyntax
{
    public HtmlSyntaxKind HtmlSyntaxKind { get; }
}
