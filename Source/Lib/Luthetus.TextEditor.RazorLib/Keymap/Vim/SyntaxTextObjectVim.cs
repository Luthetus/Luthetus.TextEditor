﻿using System.Collections.Immutable;
using Luthetus.Common.RazorLib.Keyboard;
using Luthetus.TextEditor.RazorLib.Commands.Vim;
using Luthetus.TextEditor.RazorLib.Commands;
using Luthetus.TextEditor.RazorLib.Commands.Default;
using Luthetus.TextEditor.RazorLib.Cursor;
using Microsoft.AspNetCore.Components.Web;

namespace Luthetus.TextEditor.RazorLib.Keymap.Vim;

public static class SyntaxTextObjectVim
{
    public static bool TryLex(
        KeyboardEventArgs keyboardEventArgs,
        bool hasTextSelection,
        out VimGrammarToken? vimGrammarToken)
    {
        if (!keyboardEventArgs.CtrlKey)
        {
            switch (keyboardEventArgs.Key)
            {
                case "w":
                case "e":
                case "b":
                case "h":
                case "j":
                case "k":
                case "l":
                case "$":
                case "0":
                    {
                        vimGrammarToken = new VimGrammarToken(
                            VimGrammarKind.TextObject,
                            keyboardEventArgs);

                        return true;
                    }
            }
        }

        vimGrammarToken = null;
        return false;
    }

    public static bool TryParse(TextEditorKeymapVim textEditorKeymapVim,
        ImmutableArray<VimGrammarToken> sentenceSnapshot,
        int indexInSentence,
        KeyboardEventArgs keyboardEventArgs,
        bool hasTextSelection,
        out TextEditorCommand? textEditorCommand)
    {
        var currentToken = sentenceSnapshot[indexInSentence];

        var shiftKey =
            textEditorKeymapVim.ActiveVimMode == VimMode.Visual ||
            textEditorKeymapVim.ActiveVimMode == VimMode.VisualLine;

        if (!currentToken.KeyboardEventArgs.CtrlKey)
        {
            switch (currentToken.KeyboardEventArgs.Key)
            {
                case "w":
                    textEditorCommand = TextEditorCommandVimFacts.Motions.Word;
                    return true;
                case "e":
                    textEditorCommand = TextEditorCommandVimFacts.Motions.End;
                    return true;
                case "b":
                    textEditorCommand = TextEditorCommandVimFacts.Motions.Back;
                    return true;
                case "h":
                    {
                        // Move the cursor 1 column to the left

                        textEditorCommand = new TextEditorCommand(
             textEditorCommandParameter =>
             {
                 TextEditorCursor.MoveCursor(
                     new KeyboardEventArgs
                     {
                         Key = KeyboardKeyFacts.MovementKeys.ARROW_LEFT,
                         ShiftKey = shiftKey
                     },
                     textEditorCommandParameter.PrimaryCursorSnapshot.UserCursor,
                     textEditorCommandParameter.Model);

                 return Task.CompletedTask;
             },
             true,
             "Vim::h",
             "vim_h");

                        return true;
                    }
                case "j":
                    {
                        // Move the cursor 1 row down

                        textEditorCommand = new TextEditorCommand(
                            textEditorCommandParameter =>
                            {
                                TextEditorCursor.MoveCursor(
                                    new KeyboardEventArgs
                                    {
                                        Key = KeyboardKeyFacts.MovementKeys.ARROW_DOWN,
                                        ShiftKey = shiftKey
                                    },
                                    textEditorCommandParameter.PrimaryCursorSnapshot.UserCursor,
                                    textEditorCommandParameter.Model);

                                return Task.CompletedTask;
                            },
                            true,
                            "Vim::j",
                            "vim_j");

                        return true;
                    }
                case "k":
                    {
                        // Move the cursor 1 row up

                        textEditorCommand = new TextEditorCommand(
                            textEditorCommandParameter =>
                            {
                                TextEditorCursor.MoveCursor(
                                    new KeyboardEventArgs
                                    {
                                        Key = KeyboardKeyFacts.MovementKeys.ARROW_UP,
                                        ShiftKey = shiftKey
                                    },
                                    textEditorCommandParameter.PrimaryCursorSnapshot.UserCursor,
                                    textEditorCommandParameter.Model);

                                return Task.CompletedTask;
                            },
                            true,
                            "Vim::k",
                            "vim_k");

                        return true;
                    }
                case "l":
                    {
                        // Move the cursor 1 column to the right

                        textEditorCommand = new TextEditorCommand(
                            textEditorCommandParameter =>
                            {
                                TextEditorCursor.MoveCursor(
                                    new KeyboardEventArgs
                                    {
                                        Key = KeyboardKeyFacts.MovementKeys.ARROW_RIGHT,
                                        ShiftKey = shiftKey
                                    },
                                    textEditorCommandParameter.PrimaryCursorSnapshot.UserCursor,
                                    textEditorCommandParameter.Model);

                                return Task.CompletedTask;
                            },
                            true,
                            "Vim::l",
                            "vim_l");

                        return true;
                    }
                case "$":
                    {
                        // Move the cursor to the end of the current line.

                        textEditorCommand = new TextEditorCommand(
                            textEditorCommandParameter =>
                            {
                                TextEditorCursor.MoveCursor(
                                    new KeyboardEventArgs
                                    {
                                        Key = KeyboardKeyFacts.MovementKeys.END,
                                        ShiftKey = shiftKey
                                    },
                                    textEditorCommandParameter.PrimaryCursorSnapshot.UserCursor,
                                    textEditorCommandParameter.Model);

                                return Task.CompletedTask;
                            },
                            true,
                            "Vim::$",
                            "vim_$");

                        return true;
                    }
                case "0":
                    {
                        // Move the cursor to the start of the current line.

                        textEditorCommand = new TextEditorCommand(
                            textEditorCommandParameter =>
                            {
                                TextEditorCursor.MoveCursor(
                                    new KeyboardEventArgs
                                    {
                                        Key = KeyboardKeyFacts.MovementKeys.HOME,
                                        ShiftKey = shiftKey
                                    },
                                    textEditorCommandParameter.PrimaryCursorSnapshot.UserCursor,
                                    textEditorCommandParameter.Model);

                                return Task.CompletedTask;
                            },
                            true,
                            "Vim::0",
                            "vim_0");

                        return true;
                    }
            }
        }

        textEditorCommand = TextEditorCommandDefaultFacts.DoNothingDiscard;
        return true;
    }
}