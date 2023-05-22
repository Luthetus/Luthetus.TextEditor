﻿using System.Collections.Immutable;
using Luthetus.TextEditor.RazorLib.Model;
using Luthetus.TextEditor.RazorLib.Cursor;
using Luthetus.TextEditor.RazorLib.Model;
using Luthetus.TextEditor.RazorLib.Row;
using Luthetus.TextEditor.RazorLib.Store;
using Microsoft.AspNetCore.Components.Web;

namespace Luthetus.TextEditor.RazorLib.Store.Model;

public partial class TextEditorModelsCollection
{
    public record DeleteTextByMotionAction(
        TextEditorModelKey TextEditorModelKey,
        ImmutableArray<TextEditorCursorSnapshot> CursorSnapshots,
        MotionKind MotionKind,
        CancellationToken CancellationToken);

    public record DeleteTextByRangeAction(
        TextEditorModelKey TextEditorModelKey,
        ImmutableArray<TextEditorCursorSnapshot> CursorSnapshots,
        int Count,
        CancellationToken CancellationToken);

    public record DisposeAction(TextEditorModelKey TextEditorModelKey);

    public record ForceRerenderAction(TextEditorModelKey TextEditorModelKey);

    public record InsertTextAction(
        TextEditorModelKey TextEditorModelKey,
        ImmutableArray<TextEditorCursorSnapshot> CursorSnapshots,
        string Content,
        CancellationToken CancellationToken);

    public record KeyboardEventAction(
        TextEditorModelKey TextEditorModelKey,
        ImmutableArray<TextEditorCursorSnapshot> CursorSnapshots,
        KeyboardEventArgs KeyboardEventArgs,
        CancellationToken CancellationToken);

    public record RedoEditAction(TextEditorModelKey TextEditorModelKey);

    public record RegisterAction(TextEditorModel TextEditorModel);

    public record ReloadAction(
        TextEditorModelKey TextEditorModelKey,
        string Content,
        DateTime ResourceLastWriteTime);

    public record SetResourceDataAction(
        TextEditorModelKey TextEditorModelKey,
        string ResourceUri,
        DateTime ResourceLastWriteTime);

    public record SetUsingRowEndingKindAction(
        TextEditorModelKey TextEditorModelKey,
        RowEndingKind RowEndingKind);

    public record UndoEditAction(TextEditorModelKey TextEditorModelKey);
}