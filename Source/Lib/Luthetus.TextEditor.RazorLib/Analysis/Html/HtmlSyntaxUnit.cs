using Luthetus.TextEditor.RazorLib.Analysis.Html.SyntaxObjects;

namespace Luthetus.TextEditor.RazorLib.Analysis.Html;

public class HtmlSyntaxUnit
{
    public HtmlSyntaxUnit(
        TagNode rootTagSyntax,
        TextEditorHtmlDiagnosticBag textEditorHtmlDiagnosticBag)
    {
        TextEditorHtmlDiagnosticBag = textEditorHtmlDiagnosticBag;
        RootTagSyntax = rootTagSyntax;
    }

    public TagNode RootTagSyntax { get; }
    public TextEditorHtmlDiagnosticBag TextEditorHtmlDiagnosticBag { get; }

    public class HtmlSyntaxUnitBuilder
    {
        public HtmlSyntaxUnitBuilder(TagNode rootTagSyntax, TextEditorHtmlDiagnosticBag textEditorHtmlDiagnosticBag)
        {
            RootTagSyntax = rootTagSyntax;
            TextEditorHtmlDiagnosticBag = textEditorHtmlDiagnosticBag;
        }

        public TagNode RootTagSyntax { get; }
        public TextEditorHtmlDiagnosticBag TextEditorHtmlDiagnosticBag { get; }

        public HtmlSyntaxUnit Build()
        {
            return new HtmlSyntaxUnit(
                RootTagSyntax,
                TextEditorHtmlDiagnosticBag);
        }
    }
}