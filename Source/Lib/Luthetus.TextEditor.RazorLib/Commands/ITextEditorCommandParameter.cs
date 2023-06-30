using System.Collections.Immutable;
using Luthetus.Common.RazorLib.Clipboard;
using Luthetus.TextEditor.RazorLib.Model;
using Luthetus.TextEditor.RazorLib.Cursor;
using Luthetus.TextEditor.RazorLib.ViewModel;

namespace Luthetus.TextEditor.RazorLib.Commands;

public interface ITextEditorCommandParameter
{
    public TextEditorModel Model { get; }
    public TextEditorCursorSnapshot PrimaryCursorSnapshot { get; }
    public ImmutableArray<TextEditorCursorSnapshot> CursorSnapshots { get; }
    public IClipboardService ClipboardService { get; }
    public ITextEditorService TextEditorService { get; }
    public TextEditorViewModel ViewModel { get; }
}