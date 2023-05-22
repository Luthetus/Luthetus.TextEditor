using Luthetus.TextEditor.RazorLib;
using Luthetus.TextEditor.RazorLib.Store.Options;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using Microsoft.AspNetCore.Components;

namespace Luthetus.TextEditor.RazorLib.Options;

public partial class InputTextEditorShowNewLines : FluxorComponent
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

    public bool GlobalShowNewlines
    {
        get => TextEditorOptionsStateWrap.Value.Options.ShowNewlines ?? default;
        set => TextEditorService.Options.SetShowNewlines(value);
    }
}