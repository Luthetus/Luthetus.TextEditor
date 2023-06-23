using Fluxor;
using Luthetus.Common.RazorLib;
using Luthetus.Common.RazorLib.Theme;
using Microsoft.Extensions.DependencyInjection;

namespace Luthetus.TextEditor.RazorLib;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLuthetusTextEditor(
        this IServiceCollection services,
        Func<LuthetusTextEditorOptions, LuthetusTextEditorOptions>? configure = null)
    {
        var textEditorOptions = new LuthetusTextEditorOptions();

        if (configure is not null)
            textEditorOptions = configure.Invoke(textEditorOptions);

        if (textEditorOptions.LuthetusCommonOptions is not null)
        {
            services.AddLuthetusCommonServices(options =>
                textEditorOptions.LuthetusCommonOptions);
        }

        services
            .AddSingleton(textEditorOptions)
            .AddScoped(serviceProvider => textEditorOptions.AutocompleteServiceFactory.Invoke(serviceProvider))
            .AddScoped(serviceProvider => textEditorOptions.AutocompleteIndexerFactory.Invoke(serviceProvider))
            .AddScoped<IThemeRecordsCollectionService, ThemeRecordsCollectionService>()
            .AddScoped<ITextEditorService, TextEditorService>();

        return services;
    }
}