﻿using Luthetus.Common.RazorLib.Dimensions;
using Luthetus.TextEditor.RazorLib.Model;
using Luthetus.TextEditor.RazorLib.Cursor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Luthetus.Common.RazorLib.JavaScriptObjects;

namespace Luthetus.TextEditor.RazorLib.ViewModel.InternalComponents;

public partial class BodySection : ComponentBase
{
    [Inject]
    private ITextEditorService TextEditorService { get; set; } = null!;

    [CascadingParameter]
    public TextEditorModel TextEditorModel { get; set; } = null!;
    [CascadingParameter]
    public TextEditorViewModel TextEditorViewModel { get; set; } = null!;

    /// <summary>TabIndex is used for the html attribute named: 'tabindex'</summary>
    [Parameter, EditorRequired]
    public int TabIndex { get; set; } = -1;
    [Parameter, EditorRequired]
    public RenderFragment? ContextMenuRenderFragmentOverride { get; set; }
    [Parameter, EditorRequired]
    public RenderFragment? AutoCompleteMenuRenderFragmentOverride { get; set; }
    [Parameter, EditorRequired]
    public bool IncludeContextMenuHelperComponent { get; set; }
    [Parameter, EditorRequired]
    public (string message, RelativeCoordinates relativeCoordinates)? MouseStoppedEventMostRecentResult { get; set; }

    private RowSection? _rowSectionComponent;

    public TextEditorCursorDisplay? TextEditorCursorDisplayComponent => _rowSectionComponent?.TextEditorCursorDisplayComponent;

    private string GetBodyStyleCss()
    {
        var mostDigitsInARowLineNumber = TextEditorModel.RowCount
            .ToString()
            .Length;

        var gutterWidthInPixels = mostDigitsInARowLineNumber *
                                  TextEditorViewModel.VirtualizationResult.CharacterWidthAndRowHeight.CharacterWidthInPixels;

        gutterWidthInPixels += TextEditorModel.GUTTER_PADDING_LEFT_IN_PIXELS +
                         TextEditorModel.GUTTER_PADDING_RIGHT_IN_PIXELS;

        var gutterWidthInPixelsInvariantCulture = gutterWidthInPixels
            .ToCssValue();

        var left = $"left: {gutterWidthInPixelsInvariantCulture}px;";

        var width = $"width: calc(100% - {gutterWidthInPixelsInvariantCulture}px);";

        return $"{width} {left}";
    }
}