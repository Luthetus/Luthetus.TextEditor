using Luthetus.TextEditor.RazorLib.Decoration;

namespace Luthetus.TextEditor.RazorLib.Semantics;

public class TextEditorSemanticDecorationMapper : IDecorationMapper
{
    public string Map(byte decorationByte)
    {
        var decoration = (TextEditorSemanticDecorationKind)decorationByte;

        return decoration switch
        {
            TextEditorSemanticDecorationKind.None => string.Empty,
            TextEditorSemanticDecorationKind.DiagnosticError => "luth_te_semantic-diagnostic-error",
            TextEditorSemanticDecorationKind.DiagnosticHint => "luth_te_semantic-diagnostic-hint",
            TextEditorSemanticDecorationKind.DiagnosticSuggestion => "luth_te_semantic-diagnostic-suggestion",
            TextEditorSemanticDecorationKind.DiagnosticWarning => "luth_te_semantic-diagnostic-warning",
            TextEditorSemanticDecorationKind.DiagnosticOther => "luth_te_semantic-diagnostic-other",
            _ => string.Empty,
        };
    }
}