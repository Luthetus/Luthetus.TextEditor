using Luthetus.TextEditor.RazorLib.Lexing;
using System.Collections.Immutable;
using Luthetus.TextEditor.RazorLib.Analysis;

namespace Luthetus.TextEditor.RazorLib.Semantics;

public interface ISemanticResult
{
    public ImmutableList<(TextEditorDiagnostic diagnostic, TextEditorTextSpan textSpan)> DiagnosticTextSpanTuples { get; }
    public ImmutableList<(string message, TextEditorTextSpan textSpan)> SymbolMessageTextSpanTuples { get; }
}
