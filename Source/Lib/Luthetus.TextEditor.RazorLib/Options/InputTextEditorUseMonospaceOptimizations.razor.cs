using Luthetus.TextEditor.RazorLib.Store.Options;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using Microsoft.AspNetCore.Components;

namespace Luthetus.TextEditor.RazorLib.Options;

public partial class InputTextEditorUseMonospaceOptimizations : FluxorComponent
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

    public bool UseMonospaceOptimizations
    {
        get => TextEditorOptionsStateWrap.Value.Options.UseMonospaceOptimizations;
        set => TextEditorService.Options.SetUseMonospaceOptimizations(value);
    }
}