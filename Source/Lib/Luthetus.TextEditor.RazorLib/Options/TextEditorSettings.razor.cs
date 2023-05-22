using Fluxor.Blazor.Web.Components;
using Luthetus.TextEditor.RazorLib.Autocomplete;
using Microsoft.AspNetCore.Components;

namespace Luthetus.TextEditor.RazorLib.Options;

public partial class TextEditorSettings : FluxorComponent
{
    [Inject]
    private IAutocompleteIndexer AutocompleteIndexer { get; set; } = null!;

    [Parameter]
    public string InputElementCssClass { get; set; } = string.Empty;
}