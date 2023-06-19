﻿using Luthetus.Common.RazorLib.Dimensions;
using Luthetus.Common.RazorLib.Misc;
using Luthetus.TextEditor.RazorLib.Model;
using Luthetus.TextEditor.RazorLib.Virtualization;
using Microsoft.AspNetCore.Components;

namespace Luthetus.TextEditor.RazorLib.ViewModel.InternalComponents;

public partial class GutterSection : ComponentBase
{
    [Inject]
    private ITextEditorService TextEditorService { get; set; } = null!;

    [CascadingParameter]
    public TextEditorModel TextEditorModel { get; set; } = null!;
    [CascadingParameter]
    public TextEditorViewModel TextEditorViewModel { get; set; } = null!;

    private RenderStateKey _previousRenderStateKey = RenderStateKey.Empty;

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        var viewModel = TextEditorViewModel;

        // TODO: Does 'SetGutterScrollTopAsync' need to be throttled? 
        TextEditorService.ViewModel.SetGutterScrollTopAsync(
            viewModel.GutterElementId,
            viewModel.VirtualizationResult.ElementMeasurementsInPixels.ScrollTop);

        return base.OnAfterRenderAsync(firstRender);
    }

    private string GetGutterStyleCss(int index)
    {
        var topInPixelsInvariantCulture =
            (index * TextEditorViewModel.VirtualizationResult.CharacterWidthAndRowHeight.RowHeightInPixels)
            .ToCssValue();

        var top = $"top: {topInPixelsInvariantCulture}px;";

        var heightInPixelsInvariantCulture =
            TextEditorViewModel.VirtualizationResult.CharacterWidthAndRowHeight.RowHeightInPixels
                .ToCssValue();

        var height =
            $"height: {heightInPixelsInvariantCulture}px;";

        var mostDigitsInARowLineNumber = TextEditorModel.RowCount
            .ToString()
            .Length;

        var widthInPixels = mostDigitsInARowLineNumber *
                            TextEditorViewModel.VirtualizationResult.CharacterWidthAndRowHeight.CharacterWidthInPixels;

        widthInPixels += TextEditorModel.GUTTER_PADDING_LEFT_IN_PIXELS +
                         TextEditorModel.GUTTER_PADDING_RIGHT_IN_PIXELS;

        var widthInPixelsInvariantCulture = widthInPixels.ToCssValue();

        var width = $"width: {widthInPixelsInvariantCulture}px;";

        var paddingLeftInPixelsInvariantCulture = TextEditorModel.GUTTER_PADDING_LEFT_IN_PIXELS
            .ToCssValue();

        var paddingLeft = $"padding-left: {paddingLeftInPixelsInvariantCulture}px;";

        var paddingRightInPixelsInvariantCulture = TextEditorModel.GUTTER_PADDING_RIGHT_IN_PIXELS
            .ToCssValue();

        var paddingRight = $"padding-right: {paddingRightInPixelsInvariantCulture}px;";

        return $"{width} {height} {top} {paddingLeft} {paddingRight}";
    }

    private string GetGutterSectionStyleCss()
    {
        var mostDigitsInARowLineNumber = TextEditorModel.RowCount
            .ToString()
            .Length;

        var widthInPixels = mostDigitsInARowLineNumber *
                            TextEditorViewModel.VirtualizationResult.CharacterWidthAndRowHeight.CharacterWidthInPixels;

        widthInPixels += TextEditorModel.GUTTER_PADDING_LEFT_IN_PIXELS +
                         TextEditorModel.GUTTER_PADDING_RIGHT_IN_PIXELS;

        var widthInPixelsInvariantCulture = widthInPixels.ToCssValue();

        var width = $"width: {widthInPixelsInvariantCulture}px;";

        return width;
    }

    private IVirtualizationResultWithoutTypeMask GetVirtualizationResult()
    {
        var mostDigitsInARowLineNumber = TextEditorModel.RowCount
            .ToString()
            .Length;

        var widthOfGutterInPixels = mostDigitsInARowLineNumber *
                            TextEditorViewModel.VirtualizationResult.CharacterWidthAndRowHeight.CharacterWidthInPixels;

        widthOfGutterInPixels += TextEditorModel.GUTTER_PADDING_LEFT_IN_PIXELS +
                         TextEditorModel.GUTTER_PADDING_RIGHT_IN_PIXELS;

        var topBoundaryNarrow = TextEditorViewModel.VirtualizationResult.TopVirtualizationBoundary with
        {
            WidthInPixels = widthOfGutterInPixels
        };

        var bottomBoundaryNarrow = TextEditorViewModel.VirtualizationResult.BottomVirtualizationBoundary with
        {
            WidthInPixels = widthOfGutterInPixels
        };

        return TextEditorViewModel.VirtualizationResult with
        {
            TopVirtualizationBoundary = topBoundaryNarrow,
            BottomVirtualizationBoundary = bottomBoundaryNarrow
        };
    }
}