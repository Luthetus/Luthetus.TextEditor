using System.Collections.Immutable;
using Luthetus.Common.RazorLib.Clipboard;
using Luthetus.TextEditor.RazorLib.Model;
using Luthetus.TextEditor.RazorLib.Cursor;
using Luthetus.TextEditor.RazorLib.ViewModel;
using Luthetus.TextEditor.RazorLib.Semantics;

namespace Luthetus.TextEditor.RazorLib.Commands;

public class TextEditorCommandParameter : ITextEditorCommandParameter
{
    public TextEditorCommandParameter(
        TextEditorModel textEditor,
        ImmutableArray<TextEditorCursorSnapshot> cursorSnapshots,
        IClipboardService clipboardService,
        ITextEditorService textEditorService,
        TextEditorViewModel textEditorViewModel,
        Action<TextEditorSymbolDefinition>? handleGotoDefinitionWithinDifferentFileAction)
    {
        Model = textEditor;
        CursorSnapshots = cursorSnapshots;
        ClipboardService = clipboardService;
        TextEditorService = textEditorService;
        ViewModel = textEditorViewModel;
        HandleGotoDefinitionWithinDifferentFileAction = handleGotoDefinitionWithinDifferentFileAction;
    }

    public TextEditorModel Model { get; }

    public TextEditorCursorSnapshot PrimaryCursorSnapshot => CursorSnapshots
        .First(x => x.UserCursor.IsPrimaryCursor);

    public ImmutableArray<TextEditorCursorSnapshot> CursorSnapshots { get; }
    public IClipboardService ClipboardService { get; }
    public ITextEditorService TextEditorService { get; }
    public TextEditorViewModel ViewModel { get; }
    public Action<TextEditorSymbolDefinition>? HandleGotoDefinitionWithinDifferentFileAction { get; }
}