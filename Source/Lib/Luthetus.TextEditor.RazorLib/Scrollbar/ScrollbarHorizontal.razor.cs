﻿using Fluxor;
using Luthetus.Common.RazorLib.Dimensions;
using Luthetus.Common.RazorLib.JavaScriptObjects;
using Luthetus.Common.RazorLib.Store.DragCase;
using Luthetus.TextEditor.RazorLib.Model;
using Luthetus.TextEditor.RazorLib.ViewModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Luthetus.TextEditor.RazorLib.Scrollbar;

public partial class ScrollbarHorizontal : ComponentBase, IDisposable
{
    [Inject]
    private IState<DragState> DragStateWrap { get; set; } = null!;
    [Inject]
    private IDispatcher Dispatcher { get; set; } = null!;
    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;

    [CascadingParameter]
    public TextEditorModel TextEditorModel { get; set; } = null!;
    [CascadingParameter]
    public TextEditorViewModel TextEditorViewModel { get; set; } = null!;

    private bool _thinksLeftMouseButtonIsDown;
    private RelativeCoordinates _relativeCoordinatesOnMouseDown;
    private readonly Guid _scrollbarGuid = Guid.NewGuid();

    private Func<(MouseEventArgs firstMouseEventArgs, MouseEventArgs secondMouseEventArgs), Task>? _dragEventHandler;
    private MouseEventArgs? _previousDragMouseEventArgs;

    private string ScrollbarElementId => $"luth_te_{_scrollbarGuid}";
    private string ScrollbarSliderElementId => $"luth_te_{_scrollbarGuid}-slider";

    protected override void OnInitialized()
    {
        DragStateWrap.StateChanged += DragStateWrapOnStateChanged;

        base.OnInitialized();
    }

    private string GetScrollbarHorizontalStyleCss()
    {
        var scrollbarWidthInPixels = TextEditorViewModel.VirtualizationResult.ElementMeasurementsInPixels.Width -
                                  ScrollbarFacts.SCROLLBAR_SIZE_IN_PIXELS;

        var scrollbarWidthInPixelsInvariantCulture = scrollbarWidthInPixels
            .ToCssValue();

        var width = $"width: {scrollbarWidthInPixelsInvariantCulture}px;";

        return width;
    }

    private string GetSliderHorizontalStyleCss()
    {
        var scrollbarWidthInPixels = TextEditorViewModel.VirtualizationResult.ElementMeasurementsInPixels.Width -
                                           ScrollbarFacts.SCROLLBAR_SIZE_IN_PIXELS;

        // Proportional Left
        var sliderProportionalLeftInPixels = TextEditorViewModel.VirtualizationResult.ElementMeasurementsInPixels.ScrollLeft *
                                             scrollbarWidthInPixels /
                                             TextEditorViewModel.VirtualizationResult.ElementMeasurementsInPixels.ScrollWidth;

        var sliderProportionalLeftInPixelsInvariantCulture = sliderProportionalLeftInPixels
            .ToCssValue();

        var left = $"left: {sliderProportionalLeftInPixelsInvariantCulture}px;";

        // Proportional Width
        var pageWidth = TextEditorViewModel.VirtualizationResult.ElementMeasurementsInPixels.Width;

        var sliderProportionalWidthInPixels = pageWidth *
                                              scrollbarWidthInPixels /
                                              TextEditorViewModel.VirtualizationResult.ElementMeasurementsInPixels.ScrollWidth;

        var sliderProportionalWidthInPixelsInvariantCulture = sliderProportionalWidthInPixels
            .ToCssValue();

        var width = $"width: {sliderProportionalWidthInPixelsInvariantCulture}px;";

        return $"{left} {width}";
    }

    private async Task HandleOnMouseDownAsync(MouseEventArgs mouseEventArgs)
    {
        _thinksLeftMouseButtonIsDown = true;

        _relativeCoordinatesOnMouseDown = await JsRuntime
            .InvokeAsync<RelativeCoordinates>(
                "luthetusTextEditor.getRelativePosition",
                ScrollbarSliderElementId,
                mouseEventArgs.ClientX,
                mouseEventArgs.ClientY);

        SubscribeToDragEventForScrolling();
    }

    private async void DragStateWrapOnStateChanged(object? sender, EventArgs e)
    {
        if (!DragStateWrap.Value.ShouldDisplay)
        {
            _dragEventHandler = null;
            _previousDragMouseEventArgs = null;
        }
        else
        {
            var mouseEventArgs = DragStateWrap.Value.MouseEventArgs;

            if (_dragEventHandler is not null)
            {
                if (_previousDragMouseEventArgs is not null &&
                    mouseEventArgs is not null)
                {
                    await _dragEventHandler.Invoke((_previousDragMouseEventArgs, mouseEventArgs));
                }

                _previousDragMouseEventArgs = mouseEventArgs;
                await InvokeAsync(StateHasChanged);
            }
        }
    }

    public void SubscribeToDragEventForScrolling()
    {
        _dragEventHandler = DragEventHandlerScrollAsync;
        Dispatcher.Dispatch(new DragState.SetDragStateAction(true, null));
    }

    private async Task DragEventHandlerScrollAsync(
        (MouseEventArgs firstMouseEventArgs, MouseEventArgs secondMouseEventArgs) mouseEventArgsTuple)
    {
        var localThinksLeftMouseButtonIsDown = _thinksLeftMouseButtonIsDown;

        if (!localThinksLeftMouseButtonIsDown)
            return;

        // Buttons is a bit flag
        // '& 1' gets if left mouse button is held
        if (localThinksLeftMouseButtonIsDown &&
            (mouseEventArgsTuple.secondMouseEventArgs.Buttons & 1) == 1)
        {
            var relativeCoordinatesOfDragEvent = await JsRuntime
                .InvokeAsync<RelativeCoordinates>(
                    "luthetusTextEditor.getRelativePosition",
                    ScrollbarElementId,
                    mouseEventArgsTuple.secondMouseEventArgs.ClientX,
                    mouseEventArgsTuple.secondMouseEventArgs.ClientY);

            var xPosition = relativeCoordinatesOfDragEvent.RelativeX -
                _relativeCoordinatesOnMouseDown.RelativeX;

            xPosition = Math.Max(0, xPosition);

            if (xPosition > TextEditorViewModel.VirtualizationResult.ElementMeasurementsInPixels.Height)
                xPosition = TextEditorViewModel.VirtualizationResult.ElementMeasurementsInPixels.Height;

            var scrollbarWidthInPixels = TextEditorViewModel.VirtualizationResult.ElementMeasurementsInPixels.Width -
                                         ScrollbarFacts.SCROLLBAR_SIZE_IN_PIXELS;

            var scrollLeft = xPosition *
                             TextEditorViewModel.VirtualizationResult.ElementMeasurementsInPixels.ScrollWidth /
                             scrollbarWidthInPixels;

            if (scrollLeft + TextEditorViewModel.VirtualizationResult.ElementMeasurementsInPixels.Width >
                TextEditorViewModel.VirtualizationResult.ElementMeasurementsInPixels.ScrollWidth)
            {
                scrollLeft = TextEditorViewModel.VirtualizationResult.ElementMeasurementsInPixels.ScrollWidth -
                             TextEditorViewModel.VirtualizationResult.ElementMeasurementsInPixels.Width;
            }

            await TextEditorViewModel.SetScrollPositionAsync(scrollLeft, null);
        }
        else
        {
            _thinksLeftMouseButtonIsDown = false;
        }
    }

    public void Dispose()
    {
        DragStateWrap.StateChanged -= DragStateWrapOnStateChanged;
    }
}