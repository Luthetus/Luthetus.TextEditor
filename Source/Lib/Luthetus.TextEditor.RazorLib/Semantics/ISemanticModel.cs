using Luthetus.TextEditor.RazorLib.Model;
using Luthetus.TextEditor.RazorLib.Lexing;

namespace Luthetus.TextEditor.RazorLib.Semantics;

public interface ISemanticModel
{
    public ISemanticResult? SemanticResult { get; }

    public TextEditorSymbolDefinition? GoToDefinition(
        TextEditorModel model,
        TextEditorTextSpan textSpan);

    public void Parse(
        TextEditorModel model);
}
