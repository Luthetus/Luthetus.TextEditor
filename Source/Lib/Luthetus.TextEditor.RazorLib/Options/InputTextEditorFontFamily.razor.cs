﻿using Luthetus.TextEditor.RazorLib;
using Microsoft.AspNetCore.Components;

namespace Luthetus.TextEditor.RazorLib.Options;

public partial class InputTextEditorFontFamily : ComponentBase, IDisposable
{
    [Inject]
    private ITextEditorService TextEditorService { get; set; } = null!;

    [Parameter]
    public string CssClassString { get; set; } = string.Empty;
    [Parameter]
    public string CssStyleString { get; set; } = string.Empty;

    public string FontFamily
    {
        get => TextEditorService.OptionsWrap.Value.Options.CommonOptions.FontFamily ?? "unset";
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                TextEditorService.Options.SetFontFamily(null);

            TextEditorService.Options.SetFontFamily(value.Trim());
        }
    }

    protected override void OnInitialized()
    {
        TextEditorService.OptionsWrap.StateChanged += OptionsWrapOnStateChanged;

        base.OnInitialized();
    }

    private async void OptionsWrapOnStateChanged(object? sender, EventArgs e)
    {
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        TextEditorService.OptionsWrap.StateChanged -= OptionsWrapOnStateChanged;
    }
}