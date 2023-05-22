﻿using Luthetus.TextEditor.RazorLib.Commands;
using Luthetus.TextEditor.RazorLib.Cursor;

namespace Luthetus.TextEditor.RazorLib.Keymap.Vim;

public record VimMotionResult(
    ImmutableTextEditorCursor LowerPositionIndexImmutableCursor,
    int LowerPositionIndex,
    ImmutableTextEditorCursor HigherPositionIndexImmutableCursor,
    int HigherPositionIndex,
    int PositionIndexDisplacement)
{
    public static async Task<VimMotionResult> GetResultAsync(
        ITextEditorCommandParameter textEditorCommandParameter,
        TextEditorCursor textEditorCursorForMotion,
        Func<Task> motionCommandParameter)
    {
        await motionCommandParameter.Invoke();

        var beforeMotionImmutableCursor = textEditorCommandParameter.PrimaryCursorSnapshot.ImmutableCursor;

        var beforeMotionPositionIndex = textEditorCommandParameter.Model
            .GetPositionIndex(
                beforeMotionImmutableCursor.RowIndex,
                beforeMotionImmutableCursor.ColumnIndex);

        var afterMotionImmutableCursor = new ImmutableTextEditorCursor(
            textEditorCursorForMotion);

        var afterMotionPositionIndex = textEditorCommandParameter.Model
            .GetPositionIndex(
                afterMotionImmutableCursor.RowIndex,
                afterMotionImmutableCursor.ColumnIndex);

        if (beforeMotionPositionIndex > afterMotionPositionIndex)
        {
            return new VimMotionResult(
                afterMotionImmutableCursor,
                afterMotionPositionIndex,
                beforeMotionImmutableCursor,
                beforeMotionPositionIndex,
                beforeMotionPositionIndex - afterMotionPositionIndex);
        }

        return new VimMotionResult(
            beforeMotionImmutableCursor,
            beforeMotionPositionIndex,
            afterMotionImmutableCursor,
            afterMotionPositionIndex,
            afterMotionPositionIndex - beforeMotionPositionIndex);
    }
}