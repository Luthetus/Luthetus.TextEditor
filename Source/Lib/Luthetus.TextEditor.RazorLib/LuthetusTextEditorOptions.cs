using System.Collections.Immutable;
using Luthetus.Common.RazorLib;
using Luthetus.Common.RazorLib.Theme;
using Luthetus.TextEditor.RazorLib;
using Luthetus.TextEditor.RazorLib.Autocomplete;
using Luthetus.TextEditor.RazorLib.Find;
using Luthetus.TextEditor.RazorLib.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Luthetus.TextEditor.RazorLib;

public record LuthetusTextEditorOptions
{
    public bool InitializeFluxor { get; init; } = true;
    public ThemeKey? InitialThemeKey { get; init; }
    public ImmutableArray<ThemeRecord>? CustomThemeRecords { get; init; } = LuthetusTextEditorCustomThemeFacts.AllCustomThemes;
    public ThemeRecord InitialTheme { get; init; } = ThemeFacts.VisualStudioDarkThemeClone;
    /// <summary>
    /// Default value if left null is: <see cref="AutocompleteService"/>
    /// <br/><br/>
    /// Additionally one can override this value with their own.
    /// </summary>
    public Func<IServiceProvider, IAutocompleteService> AutocompleteServiceFactory { get; init; } = serviceProvider =>
        new AutocompleteService(serviceProvider.GetRequiredService<IAutocompleteIndexer>());
    /// <summary>
    /// Default value if left null is: <see cref="AutocompleteIndexer"/>
    /// <br/><br/>
    /// Additionally one can override this value with their own.
    /// </summary>
    public Func<IServiceProvider, IAutocompleteIndexer> AutocompleteIndexerFactory { get; init; } = serviceProvider =>
        new AutocompleteIndexer(serviceProvider.GetRequiredService<ITextEditorService>());

    public Type SettingsComponentRendererType { get; init; } = typeof(TextEditorSettings);
    public bool SettingsDialogComponentIsResizable { get; init; } = true;

    public Type FindComponentRendererType { get; init; } = typeof(TextEditorFindDisplay);
    public bool FindDialogComponentIsResizable { get; init; } = true;

    public ImmutableArray<ITextEditorFindProvider> FindProviders { get; init; } = FindFacts.DefaultFindProviders;

    /// <summary>
    /// Provide null if one wishes to not have Luthetus.CommonServices initialized from within Luthetus.TextEditor
    /// but instead, one wishes to initialize Luthetus.CommonServices themselves manually.
    /// </summary>
    public LuthetusCommonOptions? LuthetusCommonOptions { get; init; } = new();
}