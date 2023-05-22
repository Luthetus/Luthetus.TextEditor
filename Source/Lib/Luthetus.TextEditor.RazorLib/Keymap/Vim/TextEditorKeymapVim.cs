﻿using Luthetus.Common.RazorLib.Dimensions;
using Luthetus.Common.RazorLib.Keyboard;
using Luthetus.TextEditor.RazorLib.Commands.Vim;
using Luthetus.TextEditor.RazorLib.Model;
using Luthetus.TextEditor.RazorLib.Commands;
using Luthetus.TextEditor.RazorLib.Commands.Default;
using Luthetus.TextEditor.RazorLib.Cursor;
using Luthetus.TextEditor.RazorLib.Keymap.Default;
using Luthetus.TextEditor.RazorLib.Options;
using Luthetus.TextEditor.RazorLib.ViewModel;
using Microsoft.AspNetCore.Components.Web;

namespace Luthetus.TextEditor.RazorLib.Keymap.Vim;

public class TextEditorKeymapVim : ITextEditorKeymap
{
    public KeymapKey KeymapKey => KeymapFacts.VimKeymapDefinition.KeymapKey;
    public string KeymapDisplayName => KeymapFacts.VimKeymapDefinition.DisplayName;

    private readonly TextEditorKeymapDefault _textEditorKeymapDefault = new();

    public VimMode ActiveVimMode { get; set; } = VimMode.Normal;

    public VimSentence VimSentence { get; } = new();

    public string GetCursorCssClassString()
    {
        switch (ActiveVimMode)
        {
            case VimMode.Normal:
            case VimMode.Visual:
            case VimMode.VisualLine:
                return TextCursorKindFacts.BlockCssClassString;
            default:
                return string.Empty;
        }
    }

    public string GetCursorCssStyleString(
        TextEditorModel textEditorModel,
        TextEditorViewModel textEditorViewModel,
        TextEditorOptions textEditorOptions)
    {
        var characterWidthInPixels = textEditorViewModel.VirtualizationResult.CharacterWidthAndRowHeight.CharacterWidthInPixels;

        switch (ActiveVimMode)
        {
            case VimMode.Normal:
            case VimMode.Visual:
            case VimMode.VisualLine:
                {
                    var characterWidthInPixelsInvariantCulture = characterWidthInPixels
                        .ToCssValue();

                    return $"width: {characterWidthInPixelsInvariantCulture}px;";
                }
        }

        return string.Empty;
    }

    public TextEditorCommand? Map(
        KeyboardEventArgs keyboardEventArgs,
        bool hasTextSelection)
    {
        if (KeyboardKeyFacts.IsMovementKey(keyboardEventArgs.Key))
        {
            if (ActiveVimMode == VimMode.Visual ||
                ActiveVimMode == VimMode.VisualLine)
            {
                keyboardEventArgs.ShiftKey = true;
            }

            TextEditorCommand? modifiedCommand = null;

            if (keyboardEventArgs.CtrlKey)
            {
                modifiedCommand = _textEditorKeymapDefault.DefaultCtrlModifiedKeymap(
                    keyboardEventArgs,
                    hasTextSelection);
            }

            if (modifiedCommand is null &&
                keyboardEventArgs.AltKey)
            {
                modifiedCommand = _textEditorKeymapDefault.DefaultAltModifiedKeymap(
                    keyboardEventArgs,
                    hasTextSelection);
            }

            if (modifiedCommand is null)
            {
                modifiedCommand = new TextEditorCommand(
                    textEditorCommandParameter =>
                    {
                        TextEditorCursor.MoveCursor(
                            keyboardEventArgs,
                            textEditorCommandParameter.PrimaryCursorSnapshot.UserCursor,
                            textEditorCommandParameter.Model);

                        return Task.CompletedTask;
                    },
                    true,
                    "MoveCursor",
                    "MoveCursor");
            }

            if (ActiveVimMode == VimMode.Visual)
                return TextEditorCommandVimFacts.Motions
                    .GetVisual(modifiedCommand, $"{nameof(TextEditorKeymapVim)}");

            if (ActiveVimMode == VimMode.VisualLine)
                return TextEditorCommandVimFacts.Motions
                    .GetVisualLine(modifiedCommand, $"{nameof(TextEditorKeymapVim)}");

            return modifiedCommand;
        }

        if (TryMapToVimKeymap(
                keyboardEventArgs,
                hasTextSelection,
                out var command))
        {
            return command;
        }

        if (keyboardEventArgs.CtrlKey)
        {
            return _textEditorKeymapDefault.DefaultCtrlModifiedKeymap(
                keyboardEventArgs,
                hasTextSelection);
        }

        if (keyboardEventArgs.AltKey)
        {
            return _textEditorKeymapDefault.DefaultAltModifiedKeymap(
                keyboardEventArgs,
                hasTextSelection);
        }

        if (hasTextSelection)
        {
            return _textEditorKeymapDefault.DefaultHasSelectionModifiedKeymap(
                keyboardEventArgs,
                hasTextSelection);
        }

        return keyboardEventArgs.Key switch
        {
            KeyboardKeyFacts.MetaKeys.PAGE_DOWN => TextEditorCommandDefaultFacts.ScrollPageDown,
            KeyboardKeyFacts.MetaKeys.PAGE_UP => TextEditorCommandDefaultFacts.ScrollPageUp,
            _ => null,
        };
    }

    public bool TryMapToVimKeymap(
        KeyboardEventArgs keyboardEventArgs,
        bool hasTextSelection,
        out TextEditorCommand? command)
    {
        switch (ActiveVimMode)
        {
            case VimMode.Normal:
            case VimMode.Visual:
            case VimMode.VisualLine:
            case VimMode.Command:
                {
                    if (TryMapToVimNormalModeKeymap(
                            keyboardEventArgs,
                            hasTextSelection,
                            out command) &&
                        command is not null)
                    {
                        return true;
                    }

                    goto default;
                }
            case VimMode.Insert:
                {
                    if (TryMapToVimInsertModeKeymap(
                            keyboardEventArgs,
                            hasTextSelection,
                            out command))
                    {
                        return true;
                    }

                    goto default;
                }
            default:
                {
                    command = null;
                    return false;
                }
        }
    }

    public bool TryMapToVimNormalModeKeymap(
        KeyboardEventArgs keyboardEventArgs,
        bool hasTextSelection,
        out TextEditorCommand? command)
    {
        if (keyboardEventArgs.Key == KeyboardKeyFacts.MetaKeys.ESCAPE)
        {
            ActiveVimMode = VimMode.Normal;

            command = TextEditorCommandDefaultFacts.ClearTextSelection;
            return true;
        }

        switch (keyboardEventArgs.Key)
        {
            case "i":
                {
                    ActiveVimMode = VimMode.Insert;
                    command = TextEditorCommandDefaultFacts.DoNothingDiscard;
                    return true;
                }
            case "v":
                {
                    if (ActiveVimMode == VimMode.Visual)
                    {
                        ActiveVimMode = VimMode.Normal;

                        command = TextEditorCommandDefaultFacts.ClearTextSelection;
                        return true;
                    }

                    ActiveVimMode = VimMode.Visual;

                    command = new TextEditorCommand(
                        textEditorCommandParameter =>
                        {
                            var positionIndex =
                                textEditorCommandParameter.Model.GetPositionIndex(
                                    textEditorCommandParameter.PrimaryCursorSnapshot.ImmutableCursor.RowIndex,
                                    textEditorCommandParameter.PrimaryCursorSnapshot.ImmutableCursor.ColumnIndex);

                            textEditorCommandParameter.PrimaryCursorSnapshot.UserCursor.Selection.AnchorPositionIndex =
                                positionIndex;

                            textEditorCommandParameter.PrimaryCursorSnapshot.UserCursor.Selection
                                    .EndingPositionIndex =
                                positionIndex + 1;

                            return Task.CompletedTask;
                        },
                        true,
                        keyboardEventArgs.Key,
                        keyboardEventArgs.Key);

                    return true;
                }
            case "V":
                {
                    if (ActiveVimMode == VimMode.VisualLine)
                    {
                        ActiveVimMode = VimMode.Normal;

                        command = TextEditorCommandDefaultFacts.ClearTextSelection;
                        return true;
                    }

                    ActiveVimMode = VimMode.VisualLine;

                    command = new TextEditorCommand(
                        textEditorCommandParameter =>
                        {
                            var startOfRowPositionIndexInclusive =
                                textEditorCommandParameter.Model.GetPositionIndex(
                                    textEditorCommandParameter.PrimaryCursorSnapshot.ImmutableCursor.RowIndex,
                                    0);

                            textEditorCommandParameter.PrimaryCursorSnapshot.UserCursor.Selection.AnchorPositionIndex =
                                startOfRowPositionIndexInclusive;

                            var endOfRowPositionIndexExclusive = textEditorCommandParameter.Model.RowEndingPositions[
                                    textEditorCommandParameter.PrimaryCursorSnapshot.ImmutableCursor.RowIndex]
                                .positionIndex;

                            textEditorCommandParameter.PrimaryCursorSnapshot.UserCursor.Selection
                                    .EndingPositionIndex =
                                endOfRowPositionIndexExclusive;

                            return Task.CompletedTask;
                        },
                        true,
                        keyboardEventArgs.Key,
                        keyboardEventArgs.Key);

                    return true;
                }
            case ":":
                {
                    command = new TextEditorCommand(textEditorCommandParameter =>
                        {
                            textEditorCommandParameter.TextEditorService.ViewModel.With(
                                textEditorCommandParameter.ViewModel.ViewModelKey,
                                previousViewModel => previousViewModel with
                                {
                                    DisplayCommandBar = true
                                });
                            return Task.CompletedTask;
                        },
                        false,
                        "Command Mode",
                        "Command Mode");

                    return true;
                }
            case "u":
                {
                    command = new TextEditorCommand(
                        async textEditorCommandParameter =>
                        {
                            textEditorCommandParameter.TextEditorService.Model.UndoEdit(
                                textEditorCommandParameter.Model.ModelKey);

                            await textEditorCommandParameter.Model
                                .ApplySyntaxHighlightingAsync();
                        },
                        false,
                        "Undo",
                        "undo");

                    return true;
                }
            case "r":
                {
                    if (keyboardEventArgs.CtrlKey)
                    {
                        command = new TextEditorCommand(
                            async textEditorCommandParameter =>
                            {
                                textEditorCommandParameter.TextEditorService.Model.RedoEdit(
                                    textEditorCommandParameter.Model.ModelKey);

                                await textEditorCommandParameter.Model
                                    .ApplySyntaxHighlightingAsync();
                            },
                            false,
                            "Redo",
                            "redo");

                        return true;
                    }

                    goto default;
                }
            case "o":
                {
                    command = TextEditorCommandVimFacts.Verbs.NewLineBelow;
                    return true;
                }
            case "O":
                {
                    command = TextEditorCommandVimFacts.Verbs.NewLineAbove;
                    return true;
                }
            default:
                {
                    if (keyboardEventArgs.Key == "Shift")
                    {
                        command = TextEditorCommandDefaultFacts.DoNothingDiscard;
                        return false;
                    }

                    var success = VimSentence.TryLex(
                        this,
                        keyboardEventArgs,
                        hasTextSelection,
                        out command);

                    return success;
                }
        }
    }

    public bool TryMapToVimInsertModeKeymap(
        KeyboardEventArgs keyboardEventArgs,
        bool hasTextSelection,
        out TextEditorCommand? command)
    {
        switch (keyboardEventArgs.Key)
        {
            case KeyboardKeyFacts.MetaKeys.ESCAPE:
                {
                    ActiveVimMode = VimMode.Normal;
                    command = TextEditorCommandDefaultFacts.DoNothingDiscard;
                    return true;
                }
            default:
                {
                    command = null;
                    return false;
                }
        }
    }
}