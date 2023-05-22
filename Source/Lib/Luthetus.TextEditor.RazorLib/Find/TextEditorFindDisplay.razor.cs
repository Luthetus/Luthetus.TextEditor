using Luthetus.TextEditor.RazorLib.Store.Find;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using Microsoft.AspNetCore.Components;

namespace Luthetus.TextEditor.RazorLib.Find;

public partial class TextEditorFindDisplay : FluxorComponent
{
    [Inject]
    private IState<TextEditorFindProviderState> FindProviderState { get; set; } = null!;
    [Inject]
    private IDispatcher Dispatcher { get; set; } = null!;

    private CancellationTokenSource _doSearchCancellationTokenSource = new();
    private bool _isSearching;
    private bool _disposed;

    private string SearchQuery
    {
        get => FindProviderState.Value.SearchQuery;
        set
        {
            if (value is not null)
            {
                Dispatcher.Dispatch(
                    new TextEditorFindProviderState.SetSearchQueryAction(
                        value));
            }
        }
    }

    private async Task DoSearchOnClickAsync(TextEditorFindProviderState findProviderState)
    {
        var activeFindProvider = findProviderState.ActiveFindProviderOrDefault();

        if (activeFindProvider is null)
            return;

        try
        {
            _isSearching = true;
            await InvokeAsync(StateHasChanged);

            _doSearchCancellationTokenSource.Cancel();
            _doSearchCancellationTokenSource = new();

            var cancellationToken = _doSearchCancellationTokenSource.Token;

            await activeFindProvider.SearchAsync(
                findProviderState.SearchQuery,
                cancellationToken);
        }
        finally
        {
            _isSearching = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _doSearchCancellationTokenSource.Cancel();
        }

        _disposed = true;

        base.Dispose(disposing);
    }
}