using Luthetus.TextEditor.RazorLib.Model;
using Luthetus.TextEditor.RazorLib.Lexing;
using System.Collections.Immutable;

namespace Luthetus.TextEditor.RazorLib.Semantics;

public interface ISemanticModel
{
    public ImmutableList<TextEditorTextSpan> DiagnosticTextSpans { get; set; }
    public ImmutableList<TextEditorTextSpan> SymbolTextSpans { get; }

    public SymbolDefinition? GoToDefinition(
        TextEditorModel model,
        TextEditorTextSpan textSpan);

    public void Parse(
        TextEditorModel model);
}
