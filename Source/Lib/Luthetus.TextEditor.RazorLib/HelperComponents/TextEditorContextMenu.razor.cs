using System.Collections.Immutable;
using Luthetus.Common.RazorLib.Clipboard;
using Luthetus.Common.RazorLib.Keyboard;
using Luthetus.Common.RazorLib.Menu;
using Luthetus.TextEditor.RazorLib.Model;
using Luthetus.TextEditor.RazorLib.Commands;
using Luthetus.TextEditor.RazorLib.Commands.Default;
using Luthetus.TextEditor.RazorLib.Cursor;
using Luthetus.TextEditor.RazorLib.ViewModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Luthetus.TextEditor.RazorLib.HelperComponents;

public partial class TextEditorContextMenu : ComponentBase
{
    [Inject]
    private IClipboardService ClipboardService { get; set; } = null!;
    [Inject]
    private ITextEditorService TextEditorService { get; set; } = null!;

    [CascadingParameter]
    public TextEditorModel TextEditorModel { get; set; } = null!;
    [CascadingParameter]
    public TextEditorViewModel TextEditorViewModel { get; set; } = null!;
    [CascadingParameter(Name = "SetShouldDisplayMenuAsync")]
    public Func<TextEditorMenuKind, bool, Task> SetShouldDisplayMenuAsync { get; set; } = null!;

    private ElementReference? _textEditorContextMenuElementReference;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (_textEditorContextMenuElementReference is not null)
            {
                try
                {
                    await _textEditorContextMenuElementReference.Value
                        .FocusAsync();
                }
                catch (Exception)
                {
                    // 2023-04-18: The app has had a bug where it "freezes" and must be restarted.
                    //             This bug is seemingly happening randomly. I have a suspicion
                    //             that there are race-condition exceptions occurring with "FocusAsync"
                    //             on an ElementReference.
                }
            }
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    private TextEditorCommandParameter ConstructTextEditorCommandParameter()
    {
        return new TextEditorCommandParameter(
            TextEditorModel,
            TextEditorCursorSnapshot.TakeSnapshots(TextEditorViewModel.PrimaryCursor),
            ClipboardService,
            TextEditorService,
            TextEditorViewModel,
            null);
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
            await SetShouldDisplayMenuAsync.Invoke(
                TextEditorMenuKind.None,
                true);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private MenuRecord GetMenuRecord()
    {
        List<MenuOptionRecord> menuOptionRecords = new();

        var cut = new MenuOptionRecord(
            "Cut",
            MenuOptionKind.Other,
            () => SelectMenuOption(CutMenuOption));

        menuOptionRecords.Add(cut);

        var copy = new MenuOptionRecord(
            "Copy",
            MenuOptionKind.Other,
            () => SelectMenuOption(CopyMenuOption));

        menuOptionRecords.Add(copy);

        var paste = new MenuOptionRecord(
            "Paste",
            MenuOptionKind.Other,
            () => SelectMenuOption(PasteMenuOption));

        menuOptionRecords.Add(paste);

        if (!menuOptionRecords.Any())
        {
            menuOptionRecords.Add(new MenuOptionRecord(
                "No Context Menu Options for this item",
                MenuOptionKind.Other));
        }

        return new MenuRecord(
            menuOptionRecords
                .ToImmutableArray());
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

    private async Task CutMenuOption()
    {
        var textEditorCommandParameter = ConstructTextEditorCommandParameter();

        var command = TextEditorCommandDefaultFacts.Cut;

        await command.DoAsyncFunc.Invoke(
            textEditorCommandParameter);
    }

    private async Task CopyMenuOption()
    {
        var textEditorCommandParameter = ConstructTextEditorCommandParameter();

        var command = TextEditorCommandDefaultFacts.Copy;

        await command.DoAsyncFunc.Invoke(
            textEditorCommandParameter);
    }

    private async Task PasteMenuOption()
    {
        var textEditorCommandParameter = ConstructTextEditorCommandParameter();

        var command = TextEditorCommandDefaultFacts.Paste;

        await command.DoAsyncFunc.Invoke(
            textEditorCommandParameter);
    }
}