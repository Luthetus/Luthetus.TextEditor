using Luthetus.TextEditor.RazorLib.Model;
using Luthetus.TextEditor.RazorLib.Lexing;
using System.Collections.Immutable;
using Luthetus.TextEditor.RazorLib.Analysis;

namespace Luthetus.TextEditor.RazorLib.Semantics;

public class SemanticModelDefault : ISemanticModel
{
    public ImmutableList<(TextEditorDiagnostic diagnostic, TextEditorTextSpan textSpan)> DiagnosticTextSpanTuples { get; private set; } = ImmutableList<(TextEditorDiagnostic diagnostic, TextEditorTextSpan textSpan)>.Empty;
    public ImmutableList<(string message, TextEditorTextSpan textSpan)> SymbolMessageTextSpanTuples { get; private set; } = ImmutableList<(string message, TextEditorTextSpan textSpan)>.Empty;

    public TextEditorSymbolDefinition? GoToDefinition(
        TextEditorModel model,
        TextEditorTextSpan textSpan)
    {
        return null;
    }

    public void Parse(
        TextEditorModel model)
    {
    }
}