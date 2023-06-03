using Luthetus.Common.RazorLib.Dimensions;
using Luthetus.TextEditor.RazorLib.Model;
using Luthetus.TextEditor.RazorLib.HelperComponents;
using Luthetus.TextEditor.RazorLib.Html;
using Luthetus.TextEditor.RazorLib.Options;
using Luthetus.TextEditor.RazorLib.ViewModel;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Luthetus.TextEditor.RazorLib.Cursor;

public partial class TextEditorCursorDisplay : ComponentBase, IDisposable
{
    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;
    [Inject]
    private ITextEditorService TextEditorService { get; set; } = null!;

    [CascadingParameter]
    public TextEditorModel TextEditorModel { get; set; } = null!;
    [CascadingParameter]
    public TextEditorViewModel TextEditorViewModel { get; set; } = null!;
    [CascadingParameter]
    public TextEditorOptions GlobalTextEditorOptions { get; set; } = null!;
    [CascadingParameter(Name = "ProportionalFontMeasurementsContainerElementId")]
    public string ProportionalFontMeasurementsContainerElementId { get; set; } = null!;

    [Parameter, EditorRequired]
    public TextEditorCursor TextEditorCursor { get; set; } = null!;
    [Parameter, EditorRequired]
    public string ScrollableContainerId { get; set; } = null!;
    [Parameter, EditorRequired]
    public bool IsFocusTarget { get; set; }
    [Parameter, EditorRequired]
    public int TabIndex { get; set; }
    [Parameter]
    public bool IncludeContextMenuHelperComponent { get; set; }
    [Parameter]
    public RenderFragment OnContextMenuRenderFragment { get; set; } = null!;
    [Parameter]
    public RenderFragment AutoCompleteMenuRenderFragment { get; set; } = null!;

    private readonly Guid _intersectionObserverMapKey = Guid.NewGuid();

    private ElementReference? _textEditorCursorDisplayElementReference;
    private TextEditorMenuKind _textEditorMenuKind;
    private int _textEditorMenuShouldGetFocusRequestCount;

    private string _previouslyObservedTextEditorCursorDisplayId = string.Empty;
    private double _leftRelativeToParentInPixels;

    public string TextEditorCursorDisplayId => TextEditorCursor.IsPrimaryCursor
        ? TextEditorViewModel.PrimaryCursorContentId
        : string.Empty;

    public string CursorStyleCss => GetCursorStyleCss();
    public string CaretRowStyleCss => GetCaretRowStyleCss();
    public string MenuStyleCss => GetMenuStyleCss();
    public string BlinkAnimationCssClass => TextEditorService.ViewModel.CursorShouldBlink
        ? "luth_te_blink"
        : string.Empty;

    public TextEditorMenuKind TextEditorMenuKind => _textEditorMenuKind;

    protected override void OnInitialized()
    {
        TextEditorService.ViewModel.CursorShouldBlinkChanged += ViewModel_CursorShouldBlinkChanged;

        base.OnInitialized();
    }

    private async void ViewModel_CursorShouldBlinkChanged()
    {
        await InvokeAsync(StateHasChanged);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        var model = TextEditorModel;
        var viewModel = TextEditorViewModel;

        if (!GlobalTextEditorOptions.UseMonospaceOptimizations)
        {
            var textOffsettingCursor = model
                .GetTextOffsettingCursor(TextEditorCursor)
                .EscapeHtml();

            var guid = Guid.NewGuid();

            var nextLeftRelativeToParentInPixels = await JsRuntime.InvokeAsync<double>(
                "luthetusTextEditor.calculateProportionalLeftOffset",
                ProportionalFontMeasurementsContainerElementId,
                $"luth_te_proportional-font-measurement-parent_{viewModel.ViewModelKey.Guid}_cursor_{guid}",
                $"luth_te_proportional-font-measurement-cursor_{viewModel.ViewModelKey.Guid}_cursor_{guid}",
                textOffsettingCursor,
                true);

            var previousLeftRelativeToParentInPixels = _leftRelativeToParentInPixels;

            _leftRelativeToParentInPixels = nextLeftRelativeToParentInPixels;

            if ((int)nextLeftRelativeToParentInPixels != (int)previousLeftRelativeToParentInPixels)
                await InvokeAsync(StateHasChanged);
        }

        if (_previouslyObservedTextEditorCursorDisplayId != TextEditorCursorDisplayId)
        {
            if (IsFocusTarget)
            {
                await JsRuntime.InvokeVoidAsync(
                    "luthetusTextEditor.initializeTextEditorCursorIntersectionObserver",
                    _intersectionObserverMapKey.ToString(),
                    DotNetObjectReference.Create(this),
                    ScrollableContainerId,
                    TextEditorCursorDisplayId);

                _previouslyObservedTextEditorCursorDisplayId = TextEditorCursorDisplayId;
            }
        }

        var rowIndex = TextEditorCursor.IndexCoordinates.rowIndex;

        // Ensure cursor stays within the row count index range
        if (rowIndex > model.RowCount - 1)
            rowIndex = model.RowCount - 1;

        var columnIndex = TextEditorCursor.IndexCoordinates.columnIndex;

        var rowLength = model.GetLengthOfRow(rowIndex);

        // Ensure cursor stays within the column count index range for the current row
        if (columnIndex > rowLength)
            columnIndex = rowLength - 1;

        rowIndex = Math.Max(0, rowIndex);
        columnIndex = Math.Max(0, columnIndex);

        TextEditorCursor.IndexCoordinates = (rowIndex, columnIndex);

        if (TextEditorCursor.ShouldRevealCursor)
        {
            TextEditorCursor.ShouldRevealCursor = false;

            if (!TextEditorCursor.IsIntersecting)
            {
                await JsRuntime.InvokeVoidAsync(
                    "luthetusTextEditor.scrollElementIntoView",
                    TextEditorCursorDisplayId);
            }
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    [JSInvokable]
    public Task OnCursorPassedIntersectionThresholdAsync(bool cursorIsIntersecting)
    {
        TextEditorCursor.IsIntersecting = cursorIsIntersecting;
        return Task.CompletedTask;
    }

    private string GetCursorStyleCss()
    {
        var textEditor = TextEditorModel;

        var leftInPixels = 0d;

        // Tab key column offset
        {
            var tabsOnSameRowBeforeCursor = textEditor
                .GetTabsCountOnSameRowBeforeCursor(
                    TextEditorCursor.IndexCoordinates.rowIndex,
                    TextEditorCursor.IndexCoordinates.columnIndex);

            // 1 of the character width is already accounted for

            var extraWidthPerTabKey = TextEditorModel.TAB_WIDTH - 1;

            leftInPixels += extraWidthPerTabKey *
                            tabsOnSameRowBeforeCursor *
                            TextEditorViewModel.VirtualizationResult.CharacterWidthAndRowHeight.CharacterWidthInPixels;
        }

        leftInPixels += TextEditorViewModel.VirtualizationResult.CharacterWidthAndRowHeight.CharacterWidthInPixels *
                        TextEditorCursor.IndexCoordinates.columnIndex;

        var leftInPixelsInvariantCulture = leftInPixels.ToCssValue();

        var left = $"left: {leftInPixelsInvariantCulture}px;";

        var topInPixelsInvariantCulture = (TextEditorViewModel.VirtualizationResult.CharacterWidthAndRowHeight.RowHeightInPixels *
                                 TextEditorCursor.IndexCoordinates.rowIndex)
            .ToCssValue();

        var top =
            $"top: {topInPixelsInvariantCulture}px;";

        var heightInPixelsInvariantCulture = TextEditorViewModel.VirtualizationResult.CharacterWidthAndRowHeight.RowHeightInPixels
            .ToCssValue();

        var height = $"height: {heightInPixelsInvariantCulture}px;";

        var widthInPixelsInvariantCulture = GlobalTextEditorOptions.CursorWidthInPixels!.Value
            .ToCssValue();

        var width = $"width: {widthInPixelsInvariantCulture}px;";

        var keymapStyling = GlobalTextEditorOptions.KeymapDefinition!.Keymap
            .GetCursorCssStyleString(
                TextEditorModel,
                TextEditorViewModel,
                GlobalTextEditorOptions);

        return $"{left} {top} {height} {width} {keymapStyling}";
    }

    private string GetCaretRowStyleCss()
    {
        var textEditor = TextEditorModel;

        var topInPixelsInvariantCulture = (TextEditorViewModel.VirtualizationResult.CharacterWidthAndRowHeight.RowHeightInPixels *
                                 TextEditorCursor.IndexCoordinates.rowIndex)
            .ToCssValue();

        var top = $"top: {topInPixelsInvariantCulture}px;";

        var heightInPixelsInvariantCulture =
            TextEditorViewModel.VirtualizationResult.CharacterWidthAndRowHeight.RowHeightInPixels
                .ToCssValue();

        var height = $"height: {heightInPixelsInvariantCulture}px;";

        var widthOfBodyInPixelsInvariantCulture = (textEditor.MostCharactersOnASingleRowTuple.rowLength *
                                         TextEditorViewModel.VirtualizationResult.CharacterWidthAndRowHeight
                                             .CharacterWidthInPixels)
            .ToCssValue();

        var width = $"width: {widthOfBodyInPixelsInvariantCulture}px;";

        return $"{top} {width} {height}";
    }

    private string GetMenuStyleCss()
    {
        var textEditor = TextEditorModel;

        var leftInPixels = 0d;

        // Tab key column offset
        {
            var tabsOnSameRowBeforeCursor = textEditor
                .GetTabsCountOnSameRowBeforeCursor(
                    TextEditorCursor.IndexCoordinates.rowIndex,
                    TextEditorCursor.IndexCoordinates.columnIndex);

            // 1 of the character width is already accounted for

            var extraWidthPerTabKey = TextEditorModel.TAB_WIDTH - 1;

            leftInPixels += extraWidthPerTabKey * tabsOnSameRowBeforeCursor *
                            TextEditorViewModel.VirtualizationResult.CharacterWidthAndRowHeight.CharacterWidthInPixels;
        }

        leftInPixels += TextEditorViewModel.VirtualizationResult.CharacterWidthAndRowHeight.CharacterWidthInPixels *
                        TextEditorCursor.IndexCoordinates.columnIndex;

        var leftInPixelsInvariantCulture = leftInPixels
            .ToCssValue();

        var left = $"left: {leftInPixelsInvariantCulture}px;";

        var topInPixelsInvariantCulture = (TextEditorViewModel.VirtualizationResult.CharacterWidthAndRowHeight.RowHeightInPixels *
                                 (TextEditorCursor.IndexCoordinates.rowIndex + 1))
            .ToCssValue();

        // Top is 1 row further than the cursor so it does not cover text at cursor position.
        var top =
            $"top: {topInPixelsInvariantCulture}px;";

        var minWidthInPixelsInvariantCulture =
            (TextEditorViewModel.VirtualizationResult.CharacterWidthAndRowHeight.CharacterWidthInPixels * 16)
            .ToCssValue();

        var minWidth = $"min-Width: {minWidthInPixelsInvariantCulture}px;";

        var minHeightInPixelsInvariantCulture =
            (TextEditorViewModel.VirtualizationResult.CharacterWidthAndRowHeight.RowHeightInPixels * 4)
            .ToCssValue();

        var minHeight = $"min-height: {minHeightInPixelsInvariantCulture}px;";

        return $"{left} {top} {minWidth} {minHeight}";
    }

    public async Task FocusAsync()
    {
        try
        {
            if (_textEditorCursorDisplayElementReference is not null)
                await _textEditorCursorDisplayElementReference.Value.FocusAsync();
        }
        catch (Exception)
        {
            // 2023-04-18: The app has had a bug where it "freezes" and must be restarted.
            //             This bug is seemingly happening randomly. I have a suspicion
            //             that there are race-condition exceptions occurring with "FocusAsync"
            //             on an ElementReference.
        }
    }

    public void PauseBlinkAnimation()
    {
        TextEditorService.ViewModel.SetCursorShouldBlink(false);
    }

    private void HandleOnKeyDown()
    {
        PauseBlinkAnimation();
    }

    public async Task SetShouldDisplayMenuAsync(
        TextEditorMenuKind textEditorMenuKind,
        bool shouldFocusCursor = true)
    {
        // Clear the counter of requests for the Menu to take focus
        _ = TextEditorMenuShouldTakeFocus();

        _textEditorMenuKind = textEditorMenuKind;

        await InvokeAsync(StateHasChanged);

        if (shouldFocusCursor && _textEditorMenuKind == TextEditorMenuKind.None)
            await FocusAsync();
    }

    public async Task SetFocusToActiveMenuAsync()
    {
        _textEditorMenuShouldGetFocusRequestCount++;
        await InvokeAsync(StateHasChanged);
    }

    private bool TextEditorMenuShouldTakeFocus()
    {
        if (_textEditorMenuShouldGetFocusRequestCount > 0)
        {
            _textEditorMenuShouldGetFocusRequestCount = 0;

            return true;
        }

        return false;
    }

    private int GetTabIndex()
    {
        if (!IsFocusTarget)
            return -1;

        return TabIndex;
    }

    public void Dispose()
    {
        TextEditorService.ViewModel.CursorShouldBlinkChanged -= ViewModel_CursorShouldBlinkChanged;

        if (IsFocusTarget)
        {
            // IBackgroundTaskQueue does not work well here because
            // this Task does not need to be tracked.
            _ = Task.Run(async () =>
            {
                try
                {
                    await JsRuntime.InvokeVoidAsync(
                        "luthetusTextEditor.disposeTextEditorCursorIntersectionObserver",
                        CancellationToken.None,
                        _intersectionObserverMapKey.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }, CancellationToken.None);
        }
    }
}