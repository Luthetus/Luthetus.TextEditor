using Luthetus.TextEditor.RazorLib.Analysis.Json.SyntaxObjects;

namespace Luthetus.TextEditor.RazorLib.Analysis.Json;

public class JsonSyntaxUnit
{
    public JsonSyntaxUnit(
        JsonDocumentSyntax jsonDocumentSyntax,
        TextEditorJsonDiagnosticBag textEditorJsonDiagnosticBag)
    {
        JsonDocumentSyntax = jsonDocumentSyntax;
        TextEditorJsonDiagnosticBag = textEditorJsonDiagnosticBag;
    }

    public JsonDocumentSyntax JsonDocumentSyntax { get; }
    public TextEditorJsonDiagnosticBag TextEditorJsonDiagnosticBag { get; }
}