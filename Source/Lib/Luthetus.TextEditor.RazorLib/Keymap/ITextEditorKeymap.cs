using Luthetus.TextEditor.RazorLib.Model;
using Luthetus.TextEditor.RazorLib.Commands;
using Luthetus.TextEditor.RazorLib.Options;
using Luthetus.TextEditor.RazorLib.ViewModel;
using Microsoft.AspNetCore.Components.Web;

namespace Luthetus.TextEditor.RazorLib.Keymap;

public interface ITextEditorKeymap
{
    public TextEditorCommand? Map(KeyboardEventArgs keyboardEventArgs, bool hasTextSelection);
    public KeymapKey KeymapKey { get; }
    public string KeymapDisplayName { get; }

    public string GetCursorCssClassString();

    public string GetCursorCssStyleString(
        TextEditorModel textEditorModel,
        TextEditorViewModel textEditorViewModel,
        TextEditorOptions textEditorOptions);
}