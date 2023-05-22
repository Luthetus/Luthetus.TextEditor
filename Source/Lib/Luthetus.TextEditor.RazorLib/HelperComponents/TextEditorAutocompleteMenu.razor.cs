﻿using System.Collections.Immutable;
using Luthetus.Common.RazorLib.Keyboard;
using Luthetus.Common.RazorLib.Menu;
using Luthetus.TextEditor.RazorLib;
using Luthetus.TextEditor.RazorLib.Model;
using Luthetus.TextEditor.RazorLib.Store.Model;
using Luthetus.TextEditor.RazorLib.Autocomplete;
using Luthetus.TextEditor.RazorLib.Cursor;
using Luthetus.TextEditor.RazorLib.ViewModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Luthetus.TextEditor.RazorLib.HelperComponents;

public partial class TextEditorAutocompleteMenu : ComponentBase
{
    [Inject]
    private ITextEditorService TextEditorService { get; set; } = null!;
    [Inject]
    private IAutocompleteService AutocompleteService { get; set; } = null!;

    [CascadingParameter]
    public TextEditorModel TextEditorModel { get; set; } = null!;
    [CascadingParameter]
    public TextEditorViewModel TextEditorViewModel { get; set; } = null!;
    [CascadingParameter(Name = "SetShouldDisplayMenuAsync")]
    public Func<TextEditorMenuKind, bool, Task> SetShouldDisplayMenuAsync { get; set; } = null!;
    [CascadingParameter(Name = "TextEditorMenuShouldTakeFocusFunc")]
    public Func<bool> TextEditorMenuShouldTakeFocusFunc { get; set; } = null!;

    private ElementReference? _textEditorAutocompleteMenuElementReference;
    private MenuDisplay? _autocompleteMenuDisplay;

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (TextEditorMenuShouldTakeFocusFunc.Invoke())
        {
            _autocompleteMenuDisplay?.SetFocusToFirstOptionInMenuAsync();
        }

        return base.OnAfterRenderAsync(firstRender);
    }

    private async Task HandleOnKeyDownAsync(KeyboardEventArgs keyboardEventArgs)
    {
        if (KeyboardKeyFacts.MetaKeys.ESCAPE == keyboardEventArgs.Key)
            await SetShouldDisplayMenuAsync.Invoke(TextEditorMenuKind.None, true);
    }

    private async Task ReturnFocusToThisAsync()
    {
        try
        {
            await SetShouldDisplayMenuAsync
                .Invoke(TextEditorMenuKind.None, true);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private MenuRecord GetMenuRecord()
    {
        var cursorSnapshots =
            TextEditorCursorSnapshot.TakeSnapshots(
                TextEditorViewModel.PrimaryCursor);

        var primaryCursorSnapshot = cursorSnapshots
            .First(x => x.UserCursor.IsPrimaryCursor);

        if (primaryCursorSnapshot.ImmutableCursor.ColumnIndex > 0)
        {
            var word = TextEditorModel.ReadPreviousWordOrDefault(
                primaryCursorSnapshot.ImmutableCursor.RowIndex,
                primaryCursorSnapshot.ImmutableCursor.ColumnIndex);

            List<MenuOptionRecord> menuOptionRecords = new();

            if (word is not null)
            {
                var autocompleteOptions = AutocompleteService
                    .GetAutocompleteOptions(word);

                menuOptionRecords = autocompleteOptions
                    .Select(option => new MenuOptionRecord(
                        option,
                        MenuOptionKind.Other,
                        () =>
                            SelectMenuOption(() =>
                                InsertAutocompleteMenuOption(
                                    word,
                                    option,
                                    TextEditorViewModel))))
                    .ToList();
            }

            if (!menuOptionRecords.Any())
            {
                menuOptionRecords.Add(new MenuOptionRecord(
                    "No results",
                    MenuOptionKind.Other));
            }

            return new MenuRecord(
                menuOptionRecords
                    .ToImmutableArray());
        }

        return new MenuRecord(
            new MenuOptionRecord[]
            {
                new(
                    "No results",
                    MenuOptionKind.Other)
            }.ToImmutableArray());
    }

    private void SelectMenuOption(Func<Task> menuOptionAction)
    {
        // IBackgroundTaskQueue does not work well here because
        // this Task does not need to be tracked.
        _ = Task.Run(async () =>
        {
            try
            {
                await SetShouldDisplayMenuAsync.Invoke(TextEditorMenuKind.None, true);
                await menuOptionAction();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }, CancellationToken.None);
    }

    private Task InsertAutocompleteMenuOption(
        string word,
        string option,
        TextEditorViewModel textEditorViewModel)
    {
        var insertTextTextEditorModelAction = new TextEditorModelsCollection.InsertTextAction(
            textEditorViewModel.ModelKey,
            TextEditorCursorSnapshot.TakeSnapshots(textEditorViewModel.PrimaryCursor),
            option.Substring(word.Length),
            CancellationToken.None);

        TextEditorService.Model.InsertText(insertTextTextEditorModelAction);

        return Task.CompletedTask;
    }
}