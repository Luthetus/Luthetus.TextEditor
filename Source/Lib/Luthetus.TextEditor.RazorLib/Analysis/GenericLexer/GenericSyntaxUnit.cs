using Luthetus.TextEditor.RazorLib.Analysis.GenericLexer.SyntaxObjects;

namespace Luthetus.TextEditor.RazorLib.Analysis.GenericLexer;

public class GenericSyntaxUnit
{
    public GenericSyntaxUnit(
        GenericDocumentSyntax genericDocumentSyntax,
        TextEditorDiagnosticBag textEditorDiagnosticBag)
    {
        GenericDocumentSyntax = genericDocumentSyntax;
        TextEditorDiagnosticBag = textEditorDiagnosticBag;
    }

    public GenericDocumentSyntax GenericDocumentSyntax { get; }
    public TextEditorDiagnosticBag TextEditorDiagnosticBag { get; }
}