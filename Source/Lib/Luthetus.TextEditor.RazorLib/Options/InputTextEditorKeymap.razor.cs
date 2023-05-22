﻿using Luthetus.TextEditor.RazorLib;
using Luthetus.TextEditor.RazorLib.Keymap;
using Luthetus.TextEditor.RazorLib.Store.Options;
using Luthetus.TextEditor.RazorLib.Keymap;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using Microsoft.AspNetCore.Components;

namespace Luthetus.TextEditor.RazorLib.Options;

public partial class InputTextEditorKeymap : FluxorComponent
{
    [Inject]
    private IState<TextEditorOptionsState> TextEditorOptionsStateWrap { get; set; } = null!;
    [Inject]
    private ITextEditorService TextEditorService { get; set; } = null!;

    [Parameter]
    public string TopLevelDivElementCssClassString { get; set; } = string.Empty;
    [Parameter]
    public string InputElementCssClassString { get; set; } = string.Empty;
    [Parameter]
    public string LabelElementCssClassString { get; set; } = string.Empty;

    private void SelectedKeymapChanged(ChangeEventArgs changeEventArgs)
    {
        var allKeymapDefinitions = KeymapFacts.AllKeymapDefinitions;

        var chosenKeymapGuidString = changeEventArgs.Value?.ToString() ?? string.Empty;

        if (Guid.TryParse(chosenKeymapGuidString,
                out var chosenKeymapKeyGuid))
        {
            var chosenKeymapKey = new KeymapKey(chosenKeymapKeyGuid);

            var foundKeymap = allKeymapDefinitions
                .FirstOrDefault(x => x.KeymapKey == chosenKeymapKey);

            if (foundKeymap is not null)
                TextEditorService.Options.SetKeymap(foundKeymap);
        }
        else
        {
            TextEditorService.Options.SetKeymap(KeymapFacts.DefaultKeymapDefinition);
        }
    }
}