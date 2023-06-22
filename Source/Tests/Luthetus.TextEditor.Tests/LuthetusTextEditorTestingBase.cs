using Luthetus.Common.RazorLib.Clipboard;
using Luthetus.Common.RazorLib.Storage;
using Luthetus.TextEditor.RazorLib;
using Luthetus.TextEditor.RazorLib.Model;
using Luthetus.TextEditor.RazorLib.ViewModel;
using Fluxor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace Luthetus.TextEditor.Tests;

/// <summary>
/// Setup the dependency injection necessary
/// </summary>
public class LuthetusTextEditorTestingBase
{
    protected readonly ServiceProvider ServiceProvider;
    protected readonly ITextEditorService TextEditorService;
    protected readonly TextEditorModelKey TextEditorModelKey = TextEditorModelKey.NewTextEditorModelKey();
    protected readonly TextEditorViewModelKey TextEditorViewModelKey = TextEditorViewModelKey.NewTextEditorViewModelKey();

    protected TextEditorModel TextEditorModel => TextEditorService.Model
        .FindOrDefault(TextEditorModelKey)
            ?? throw new ApplicationException(
                $"{nameof(TextEditorService)}" +
                $".{nameof(TextEditorService.Model.FindOrDefault)}" +
                " returned null.");

    protected TextEditorViewModel TextEditorViewModel => TextEditorService.ViewModel
        .FindOrDefault(TextEditorViewModelKey)
            ?? throw new ApplicationException(
                $"{nameof(TextEditorService)}" +
                $".{nameof(TextEditorService.ViewModel.FindOrDefault)}" +
                " returned null.");

    public LuthetusTextEditorTestingBase()
    {
        var services = new ServiceCollection();

        services.AddScoped<IJSRuntime>(_ => new DoNothingJsRuntime());

        services.AddLuthetusTextEditor(inTextEditorOptions =>
        {
            var luthetusCommonOptions = inTextEditorOptions.LuthetusCommonOptions ?? new();

            var luthetusCommonFactories = luthetusCommonOptions.LuthetusCommonFactories with
            {
                ClipboardServiceFactory = _ => new InMemoryClipboardService(true),
                StorageServiceFactory = _ => new DoNothingStorageService(true)
            };

            luthetusCommonOptions = luthetusCommonOptions with
            {
                LuthetusCommonFactories = luthetusCommonFactories
            };

            return inTextEditorOptions with
            {
                CustomThemeRecords = LuthetusTextEditorCustomThemeFacts.AllCustomThemes,
                InitialThemeKey = LuthetusTextEditorCustomThemeFacts.DarkTheme.ThemeKey,
                LuthetusCommonOptions = luthetusCommonOptions
            };
        });

        services.AddFluxor(options => options
            .ScanAssemblies(
                typeof(Luthetus.Common.RazorLib.ServiceCollectionExtensions).Assembly,
                typeof(Luthetus.TextEditor.RazorLib.ServiceCollectionExtensions).Assembly));

        ServiceProvider = services.BuildServiceProvider();

        var store = ServiceProvider.GetRequiredService<IStore>();

        store.InitializeAsync().Wait();

        TextEditorService = ServiceProvider
            .GetRequiredService<ITextEditorService>();

        var textEditor = new TextEditorModel(
            new(nameof(LuthetusTextEditorTestingBase)),
            DateTime.UtcNow,
            "UnitTests",
            string.Empty,
            null,
            null,
            null,
            null,
            new(),
            TextEditorModelKey);

        TextEditorService.Model.RegisterCustom(textEditor);

        TextEditorService.ViewModel.Register(
            TextEditorViewModelKey,
            TextEditorModelKey);
    }
}