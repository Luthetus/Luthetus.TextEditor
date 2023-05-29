using Luthetus.TextEditor.RazorLib.Lexing;
using Luthetus.TextEditor.RazorLib.Model;
using Luthetus.TextEditor.RazorLib.ViewModel;
using Microsoft.AspNetCore.Components;

namespace Luthetus.TextEditor.RazorLib.Options;

public partial class TextEditorSettingsPreview : ComponentBase
{
    [Inject]
    private ITextEditorService TextEditorService { get; set; } = null!;

    [Parameter]
    public string TopLevelDivElementCssClassString { get; set; } = string.Empty;
    [Parameter]
    public string InputElementCssClassString { get; set; } = string.Empty;
    [Parameter]
    public string LabelElementCssClassString { get; set; } = string.Empty;
    [Parameter]
    public string PreviewElementCssClassString { get; set; } = string.Empty;

    public static readonly TextEditorModelKey SettingsPreviewTextEditorModelKey = TextEditorModelKey.NewTextEditorModelKey();
    public static readonly TextEditorViewModelKey SettingsPreviewTextEditorViewModelKey = TextEditorViewModelKey.NewTextEditorViewModelKey();

    protected override void OnInitialized()
    {
        TextEditorService.Model.RegisterTemplated(
            SettingsPreviewTextEditorModelKey,
            WellKnownModelKind.Plain,
            new ResourceUri("SettingsPreviewTextEditorModelKey"),
            DateTime.UtcNow,
            "Settings Preview",
            "Preview settings here");

        TextEditorService.ViewModel.Register(
            SettingsPreviewTextEditorViewModelKey,
            SettingsPreviewTextEditorModelKey);

        base.OnInitialized();
    }
}