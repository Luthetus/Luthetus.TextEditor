﻿using Luthetus.TextEditor.RazorLib.Cursor;
using Luthetus.TextEditor.RazorLib.Store.Model;
using Luthetus.TextEditor.RazorLib.Commands.Default;
using Luthetus.TextEditor.RazorLib.Keymap;
using Luthetus.TextEditor.RazorLib.Keymap.Vim;

namespace Luthetus.TextEditor.RazorLib.Commands.Vim;

public static partial class TextEditorCommandVimFacts
{
    public static class Verbs
    {
        public static readonly TextEditorCommand DeleteLine = new(
            async textEditorCommandParameter =>
            {
                await TextEditorCommandDefaultFacts.Cut.DoAsyncFunc
                    .Invoke(textEditorCommandParameter);
            },
            true,
            "Vim::Delete(Line)",
            "Vim::Delete(Line)");

        public static readonly TextEditorCommand ChangeLine = new(
            async textEditorCommandParameter =>
            {
                await DeleteLine.DoAsyncFunc
                    .Invoke(textEditorCommandParameter);

                var activeKeymapDefinition = textEditorCommandParameter.TextEditorService.OptionsWrap.Value.Options.KeymapDefinition
                                             ?? KeymapFacts.DefaultKeymapDefinition;
                
                if (activeKeymapDefinition.Keymap
                    is TextEditorKeymapVim vimKeymap)
                {
                    vimKeymap.ActiveVimMode = VimMode.Insert;
                }
            },
            true,
            "Vim::Change(Line)",
            "Vim::Change(Line)");

        public static TextEditorCommand GetDeleteMotion(TextEditorCommand innerTextEditorCommand) => new(
            async textEditorCommandParameter =>
            {
                var textEditorCursorForMotion = new TextEditorCursor(
                    textEditorCommandParameter
                        .PrimaryCursorSnapshot.UserCursor.IndexCoordinates,
                    true);

                var textEditorCommandParameterForMotion = new TextEditorCommandParameter(
                    textEditorCommandParameter.Model,
                    TextEditorCursorSnapshot.TakeSnapshots(textEditorCursorForMotion),
                    textEditorCommandParameter.ClipboardService,
                    textEditorCommandParameter.TextEditorService,
                    textEditorCommandParameter.ViewModel);

                var motionResult = await VimMotionResult
                    .GetResultAsync(
                        textEditorCommandParameter,
                        textEditorCursorForMotion,
                        async () =>
                            await innerTextEditorCommand.DoAsyncFunc
                                .Invoke(textEditorCommandParameterForMotion));

                var cursorForDeletion = new TextEditorCursor(
                    (motionResult.LowerPositionIndexImmutableCursor.RowIndex,
                        motionResult.LowerPositionIndexImmutableCursor.ColumnIndex),
                    true);

                var deleteTextTextEditorModelAction = new TextEditorModelsCollection.DeleteTextByRangeAction(
                    textEditorCommandParameter.Model.ModelKey,
                    TextEditorCursorSnapshot.TakeSnapshots(cursorForDeletion),
                    motionResult.PositionIndexDisplacement,
                    CancellationToken.None);

                textEditorCommandParameter
                    .TextEditorService.Model
                    .DeleteTextByRange(deleteTextTextEditorModelAction);
            },
            true,
            $"Vim::Delete({innerTextEditorCommand.DisplayName})",
            $"Vim::Delete({innerTextEditorCommand.DisplayName})");
        
        public static TextEditorCommand GetChangeMotion(TextEditorCommand innerTextEditorCommand) => new(
            async textEditorCommandParameter =>
            {
                var deleteMotion = GetDeleteMotion(innerTextEditorCommand);

                await deleteMotion.DoAsyncFunc
                    .Invoke(textEditorCommandParameter);
                
                var activeKeymapDefinition = textEditorCommandParameter.TextEditorService.OptionsWrap.Value.Options.KeymapDefinition
                                             ?? KeymapFacts.DefaultKeymapDefinition;
                
                if (activeKeymapDefinition.Keymap
                    is TextEditorKeymapVim textEditorKeymapVim)
                {
                    textEditorKeymapVim.ActiveVimMode = VimMode.Insert;
                }
            },
            true,
            $"Vim::Change({innerTextEditorCommand.DisplayName})",
            $"Vim::Change({innerTextEditorCommand.DisplayName})");
        
        public static readonly TextEditorCommand ChangeSelection = new(
            async textEditorCommandParameter =>
            {
                await TextEditorCommandDefaultFacts.Cut.DoAsyncFunc
                    .Invoke(textEditorCommandParameter);

                var activeKeymapDefinition = textEditorCommandParameter.TextEditorService.OptionsWrap.Value.Options.KeymapDefinition
                                             ?? KeymapFacts.DefaultKeymapDefinition;
                
                if (activeKeymapDefinition.Keymap
                    is TextEditorKeymapVim textEditorKeymapVim)
                {
                    textEditorKeymapVim.ActiveVimMode = VimMode.Insert;
                }
            },
            true,
            "Vim::Change(Selection)",
            "Vim::Change(Selection)");
        
        public static readonly TextEditorCommand Yank = new(
            async textEditorCommandParameter =>
            {
                await TextEditorCommandDefaultFacts.Copy.DoAsyncFunc
                    .Invoke(textEditorCommandParameter);
                
                await TextEditorCommandDefaultFacts.ClearTextSelection.DoAsyncFunc
                    .Invoke(textEditorCommandParameter);
            },
            true,
            "Vim::Change(Selection)",
            "Vim::Change(Selection)");
        
        public static readonly TextEditorCommand NewLineBelow = new(
            async textEditorCommandParameter =>
            {
                await TextEditorCommandDefaultFacts.NewLineBelow.DoAsyncFunc
                    .Invoke(textEditorCommandParameter);
                
                var activeKeymapDefinition = textEditorCommandParameter.TextEditorService.OptionsWrap.Value.Options.KeymapDefinition
                                             ?? KeymapFacts.DefaultKeymapDefinition;
                
                if (activeKeymapDefinition.Keymap
                    is TextEditorKeymapVim textEditorKeymapVim)
                {
                    textEditorKeymapVim.ActiveVimMode = VimMode.Insert;
                }
            },
            true,
            "Vim::NewLineBelow()",
            "Vim::NewLineBelow()");
        
        public static readonly TextEditorCommand NewLineAbove = new(
            async textEditorCommandParameter =>
            {
                await TextEditorCommandDefaultFacts.NewLineAbove.DoAsyncFunc
                    .Invoke(textEditorCommandParameter);
                
                var activeKeymapDefinition = textEditorCommandParameter.TextEditorService.OptionsWrap.Value.Options.KeymapDefinition
                                             ?? KeymapFacts.DefaultKeymapDefinition;
                
                if (activeKeymapDefinition.Keymap
                    is TextEditorKeymapVim textEditorKeymapVim)
                {
                    textEditorKeymapVim.ActiveVimMode = VimMode.Insert;
                }
            },
            true,
            "Vim::NewLineAbove()",
            "Vim::NewLineAbove()");
    }
}