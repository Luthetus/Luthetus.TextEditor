using Luthetus.TextEditor.RazorLib.Lexing;
using System.Collections.Immutable;
using Luthetus.TextEditor.RazorLib.Analysis;

namespace Luthetus.TextEditor.RazorLib.Semantics;

public record SemanticResult : ISemanticResult
{
    public ImmutableList<(TextEditorDiagnostic diagnostic, TextEditorTextSpan textSpan)> DiagnosticTextSpanTuples { get; init; } = ImmutableList<(TextEditorDiagnostic diagnostic, TextEditorTextSpan textSpan)>.Empty;
    public ImmutableList<(string message, TextEditorTextSpan textSpan)> SymbolMessageTextSpanTuples { get; init; } = ImmutableList<(string message, TextEditorTextSpan textSpan)>.Empty;
}
