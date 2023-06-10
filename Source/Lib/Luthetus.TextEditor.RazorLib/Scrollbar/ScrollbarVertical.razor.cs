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

public partial class ScrollbarVertical : ComponentBase, IDisposable
{
    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;
    [Inject]
    private IState<DragState> DragStateWrap { get; set; } = null!;
    [Inject]
    private IDispatcher Dispatcher { get; set; } = null!;

    [CascadingParameter]
    public TextEditorModel TextEditorModel { get; set; } = null!;
    [CascadingParameter]
    public TextEditorViewModel TextEditorViewModel { get; set; } = null!;

    private bool _thinksLeftMouseButtonIsDown;
    private RelativeCoordinates _relativeCoordinatesOnMouseDown = new(0, 0, 0, 0);
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

    private string GetSliderVerticalStyleCss()
    {
        var scrollbarHeightInPixels = TextEditorViewModel.VirtualizationResult.ElementMeasurementsInPixels.Height -
                                      ScrollbarFacts.SCROLLBAR_SIZE_IN_PIXELS;

        // Proportional Top
        var sliderProportionalTopInPixels = TextEditorViewModel.VirtualizationResult.ElementMeasurementsInPixels.ScrollTop *
                                            scrollbarHeightInPixels /
                                            TextEditorViewModel.VirtualizationResult.ElementMeasurementsInPixels.ScrollHeight;

        var sliderProportionalTopInPixelsInvariantCulture = sliderProportionalTopInPixels
            .ToCssValue();

        var top = $"top: {sliderProportionalTopInPixelsInvariantCulture}px;";

        // Proportional Height
        var pageHeight = TextEditorViewModel.VirtualizationResult.ElementMeasurementsInPixels.Height;

        var sliderProportionalHeightInPixels = pageHeight *
                                               scrollbarHeightInPixels /
                                               TextEditorViewModel.VirtualizationResult.ElementMeasurementsInPixels.ScrollHeight;

        var sliderProportionalHeightInPixelsInvariantCulture = sliderProportionalHeightInPixels
            .ToCssValue();

        var height = $"height: {sliderProportionalHeightInPixelsInvariantCulture}px;";

        return $"{top} {height}";
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

            var yPosition = relativeCoordinatesOfDragEvent.RelativeY -
                _relativeCoordinatesOnMouseDown.RelativeY;

            yPosition = Math.Max(0, yPosition);

            if (yPosition > TextEditorViewModel.VirtualizationResult.ElementMeasurementsInPixels.Height)
                yPosition = TextEditorViewModel.VirtualizationResult.ElementMeasurementsInPixels.Height;

            var scrollbarHeightInPixels = TextEditorViewModel.VirtualizationResult.ElementMeasurementsInPixels.Height -
                                            ScrollbarFacts.SCROLLBAR_SIZE_IN_PIXELS;

            var scrollTop = yPosition *
                            TextEditorViewModel.VirtualizationResult.ElementMeasurementsInPixels.ScrollHeight /
                            scrollbarHeightInPixels;

            if (scrollTop + TextEditorViewModel.VirtualizationResult.ElementMeasurementsInPixels.Height >
                TextEditorViewModel.VirtualizationResult.ElementMeasurementsInPixels.ScrollHeight)
            {
                scrollTop = TextEditorViewModel.VirtualizationResult.ElementMeasurementsInPixels.ScrollHeight -
                            TextEditorViewModel.VirtualizationResult.ElementMeasurementsInPixels.Height;
            }

            await TextEditorViewModel.SetScrollPositionAsync(null, scrollTop);
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