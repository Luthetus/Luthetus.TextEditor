using Microsoft.AspNetCore.Components.Web;

namespace Luthetus.TextEditor.RazorLib.Keymap.Vim;

public record VimGrammarToken(
    VimGrammarKind VimGrammarKind,
    KeyboardEventArgs KeyboardEventArgs);