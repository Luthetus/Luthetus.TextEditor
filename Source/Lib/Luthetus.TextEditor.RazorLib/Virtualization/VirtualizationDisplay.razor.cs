using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Luthetus.TextEditor.RazorLib.Virtualization;

public partial class VirtualizationDisplay : ComponentBase, IDisposable
{
    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;

    [Parameter, EditorRequired]
    public IVirtualizationResultWithoutTypeMask VirtualizationResultWithoutTypeMask { get; set; } = null!;
    [Parameter, EditorRequired]
    public Action<VirtualizationRequest>? ItemsProviderFunc { get; set; }

    [Parameter]
    public bool UseHorizontalVirtualization { get; set; } = true;
    [Parameter]
    public bool UseVerticalVirtualization { get; set; } = true;

    private readonly Guid _virtualizationDisplayGuid = Guid.NewGuid();

    private string LeftBoundaryElementId =>
        $"luth_te_left-virtualization-boundary-display-{_virtualizationDisplayGuid}";

    private string RightBoundaryElementId =>
        $"luth_te_right-virtualization-boundary-display-{_virtualizationDisplayGuid}";

    private string TopBoundaryElementId =>
        $"luth_te_top-virtualization-boundary-display-{_virtualizationDisplayGuid}";

    private string BottomBoundaryElementId =>
        $"luth_te_bottom-virtualization-boundary-display-{_virtualizationDisplayGuid}";

    private VirtualizationRequest _request = null!;

    private ElementReference _scrollableParentFinder;

    private CancellationTokenSource _scrollEventCancellationTokenSource = new();

    protected override void OnInitialized()
    {
        _scrollEventCancellationTokenSource.Cancel();
        _scrollEventCancellationTokenSource = new CancellationTokenSource();

        _request = new(
            new VirtualizationScrollPosition(0, 0, 0, 0),
            _scrollEventCancellationTokenSource.Token);

        base.OnInitialized();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var boundaryIds = new List<object>();

            if (UseHorizontalVirtualization)
            {
                boundaryIds.AddRange(new[]
                {
                    LeftBoundaryElementId,
                    RightBoundaryElementId,
                });
            }

            if (UseVerticalVirtualization)
            {
                boundaryIds.AddRange(new[]
                {
                    TopBoundaryElementId,
                    BottomBoundaryElementId,
                });
            }

            await JsRuntime.InvokeVoidAsync(
                "luthetusTextEditor.initializeVirtualizationIntersectionObserver",
                _virtualizationDisplayGuid.ToString(),
                DotNetObjectReference.Create(this),
                _scrollableParentFinder,
                boundaryIds);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    [JSInvokable]
    public Task OnScrollEventAsync(VirtualizationScrollPosition scrollPosition)
    {
        _scrollEventCancellationTokenSource.Cancel();
        _scrollEventCancellationTokenSource = new CancellationTokenSource();

        _request = new VirtualizationRequest(
            scrollPosition,
            _scrollEventCancellationTokenSource.Token);

        if (ItemsProviderFunc is not null)
            ItemsProviderFunc.Invoke(_request);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _scrollEventCancellationTokenSource.Cancel();

        // ICommonBackgroundTaskQueue does not work well here because
        // this Task does not need to be tracked.
        _ = Task.Run(async () =>
        {
            await JsRuntime.InvokeVoidAsync(
                "luthetusTextEditor.disposeVirtualizationIntersectionObserver",
                CancellationToken.None,
                _virtualizationDisplayGuid.ToString());
        }, CancellationToken.None);
    }
}