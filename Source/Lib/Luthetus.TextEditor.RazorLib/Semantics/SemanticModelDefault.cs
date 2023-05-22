using Luthetus.TextEditor.RazorLib.Model;
using Luthetus.TextEditor.RazorLib.Lexing;
using System.Collections.Immutable;

namespace Luthetus.TextEditor.RazorLib.Semantics;

public class SemanticModelDefault : ISemanticModel
{
    public ImmutableList<TextEditorTextSpan> DiagnosticTextSpans { get; set; } = ImmutableList<TextEditorTextSpan>.Empty;
    public ImmutableList<TextEditorTextSpan> SymbolTextSpans { get; } = ImmutableList<TextEditorTextSpan>.Empty;

    public SymbolDefinition? GoToDefinition(
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