using Luthetus.Common.RazorLib.Store.ThemeCase;
using Luthetus.Common.RazorLib.Theme;
using Luthetus.TextEditor.RazorLib.Store.Find;
using Luthetus.TextEditor.RazorLib.Store.Options;
using Fluxor;
using Microsoft.AspNetCore.Components;

namespace Luthetus.TextEditor.RazorLib;

public partial class LuthetusTextEditorInitializer : ComponentBase
{
    [Inject]
    private LuthetusTextEditorOptions LuthetusTextEditorOptions { get; set; } = null!;
    [Inject]
    private IDispatcher Dispatcher { get; set; } = null!;
    [Inject]
    private IThemeRecordsCollectionService ThemeRecordsCollectionService { get; set; } = null!;
    [Inject]
    private ITextEditorService TextEditorService { get; set; } = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (LuthetusTextEditorOptions.CustomThemeRecords is not null)
        {
            foreach (var themeRecord in LuthetusTextEditorOptions.CustomThemeRecords)
            {
                Dispatcher.Dispatch(
                    new ThemeRecordsCollection.RegisterAction(
                        themeRecord));
            }
        }

        var initialThemeRecord = ThemeRecordsCollectionService.ThemeRecordsCollectionWrap.Value.ThemeRecordsList
            .FirstOrDefault(x => x.ThemeKey == LuthetusTextEditorOptions.InitialThemeKey);

        if (initialThemeRecord is not null)
        {
            Dispatcher.Dispatch(
                new TextEditorOptionsState.SetThemeAction(
                    initialThemeRecord));
        }

        foreach (var findProvider in LuthetusTextEditorOptions.FindProviders)
        {
            Dispatcher.Dispatch(
                new TextEditorFindProviderState.RegisterAction(
                    findProvider));
        }

        await TextEditorService.Options.SetFromLocalStorageAsync();

        await base.OnAfterRenderAsync(firstRender);
    }
}