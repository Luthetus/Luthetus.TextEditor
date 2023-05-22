using Luthetus.TextEditor.RazorLib;
using Luthetus.TextEditor.RazorLib.Store.Options;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using Microsoft.AspNetCore.Components;

namespace Luthetus.TextEditor.RazorLib.Options;

public partial class InputTextEditorHeight : FluxorComponent
{
    [Inject]
    private IState<TextEditorOptionsState> TextEditorOptionsStateWrap { get; set; } = null!;
    [Inject]
    private ITextEditorService TextEditorService { get; set; } = null!;

    [CascadingParameter(Name = "InputElementCssClass")]
    public string CascadingInputElementCssClass { get; set; } = string.Empty;

    [Parameter]
    public string TopLevelDivElementCssClassString { get; set; } = string.Empty;
    [Parameter]
    public string InputElementCssClassString { get; set; } = string.Empty;
    [Parameter]
    public string LabelElementCssClassString { get; set; } = string.Empty;
    [Parameter]
    public string CheckboxElementCssClassString { get; set; } = string.Empty;

    private const int MINIMUM_HEIGHT_IN_PIXELS = 200;

    private int TextEditorHeight
    {
        get => TextEditorOptionsStateWrap.Value.Options.TextEditorHeightInPixels
               ?? MINIMUM_HEIGHT_IN_PIXELS;
        set
        {
            if (value < MINIMUM_HEIGHT_IN_PIXELS)
                value = MINIMUM_HEIGHT_IN_PIXELS;

            TextEditorService.Options.SetHeight(value);
        }
    }

    public string GetIsDisabledCssClassString(bool globalHeightInPixelsValueIsNull)
    {
        return globalHeightInPixelsValueIsNull
            ? "bte_disabled"
            : "";
    }

    private void ToggleUseGlobalHeightInPixels(bool globalHeightInPixelsValueIsNull)
    {
        if (globalHeightInPixelsValueIsNull)
            TextEditorService.Options.SetHeight(MINIMUM_HEIGHT_IN_PIXELS);
        else
            TextEditorService.Options.SetHeight(null);
    }
}