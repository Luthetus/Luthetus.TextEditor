using System.Collections.Immutable;
using Luthetus.Common.RazorLib.Misc;
using Luthetus.TextEditor.RazorLib.Model;
using Luthetus.TextEditor.RazorLib.Character;
using Luthetus.TextEditor.RazorLib.Decoration;
using Luthetus.TextEditor.RazorLib.Options;
using Luthetus.TextEditor.RazorLib.Cursor;
using Luthetus.TextEditor.RazorLib.Measurement;
using Luthetus.TextEditor.RazorLib.Virtualization;
using Luthetus.Common.RazorLib.Reactive;
using Luthetus.TextEditor.RazorLib.Lexing;
using Luthetus.TextEditor.RazorLib.Semantics;

namespace Luthetus.TextEditor.RazorLib.ViewModel;

/// <summary>Stores the state of the user interface.<br/><br/>For example, the user's <see cref="TextEditorCursor"/> instances are stored here.<br/><br/>Each <see cref="TextEditorViewModel"/> has a unique underlying <see cref="TextEditorModel"/>.<br/><br/>Therefore, if one has a <see cref="TextEditorModel"/> of a text file named "myHomework.txt", then arbitrary amount of <see cref="TextEditorViewModel"/>(s) can reference that <see cref="TextEditorModel"/>.<br/><br/>For example, maybe one has a main text editor, but also a peek window open of the same underlying <see cref="TextEditorModel"/>. The main text editor is one <see cref="TextEditorViewModel"/> and the peek window is a separate <see cref="TextEditorViewModel"/>. Both of those <see cref="TextEditorViewModel"/>(s) are referencing the same <see cref="TextEditorModel"/>. Therefore typing into the peek window will also result in the main text editor re-rendering with the updated text and vice versa.</summary>
public record TextEditorViewModel
{
    public TextEditorViewModel(
        TextEditorViewModelKey viewModelKey,
        TextEditorModelKey modelKey,
        ITextEditorService textEditorService,
        VirtualizationResult<List<RichCharacter>> virtualizationResult,
        bool displayCommandBar)
    {
        ViewModelKey = viewModelKey;
        ModelKey = modelKey;
        TextEditorService = textEditorService;
        VirtualizationResult = virtualizationResult;
        DisplayCommandBar = displayCommandBar;

        DisplayTracker = new(
            () => textEditorService.ViewModel.FindOrDefault(viewModelKey),
            () => textEditorService.ViewModel.FindBackingModelOrDefault(viewModelKey));
    }

    private const int _clearTrackingOfUniqueIdentifiersWhenCountIs = 250;

    private readonly object _validateRenderLock = new();
    private readonly object _trackingOfUniqueIdentifiersLock = new();

    private ElementMeasurementsInPixels _mostRecentBodyMeasurementsInPixels = new(0, 0, 0, 0, 0, 0, 0, CancellationToken.None);
    private BatchScrollEvents _batchScrollEvents = new();

    public IThrottle ThrottleRemeasure { get; } = new Throttle(IThrottle.DefaultThrottleTimeSpan);
    public IThrottle ThrottleCalculateVirtualizationResult { get; } = new Throttle(IThrottle.DefaultThrottleTimeSpan);

    public TextEditorCursor PrimaryCursor { get; } = new(true);
    public DisplayTracker DisplayTracker { get; }

    public TextEditorViewModelKey ViewModelKey { get; init; }
    public TextEditorModelKey ModelKey { get; init; }
    public ITextEditorService TextEditorService { get; init; }
    public VirtualizationResult<List<RichCharacter>> VirtualizationResult { get; init; }
    public bool DisplayCommandBar { get; init; }
    public Action<TextEditorModel>? OnSaveRequested { get; init; }
    public Func<TextEditorModel, string>? GetTabDisplayNameFunc { get; init; }
    /// <summary><see cref="FirstPresentationLayer"/> is painted prior to any internal workings of the text editor.<br/><br/>Therefore the selected text background is rendered after anything in the <see cref="FirstPresentationLayer"/>.<br/><br/>When using the <see cref="FirstPresentationLayer"/> one might find their css overriden by for example, text being selected.</summary>
    public ImmutableList<TextEditorPresentationModel> FirstPresentationLayer { get; init; } = ImmutableList<TextEditorPresentationModel>.Empty;
    /// <summary><see cref="LastPresentationLayer"/> is painted after any internal workings of the text editor.<br/><br/>Therefore the selected text background is rendered before anything in the <see cref="LastPresentationLayer"/>.<br/><br/>When using the <see cref="LastPresentationLayer"/> one might find the selected text background not being rendered with the text selection css if it were overriden by something in the <see cref="LastPresentationLayer"/>.</summary>
    public ImmutableList<TextEditorPresentationModel> LastPresentationLayer { get; init; } = ImmutableList<TextEditorPresentationModel>.Empty;

    /// <summary>In order to prevent infinite loops, track the unique identifiers. Note, this HashSet is cleared when the options change or the count >= <see cref="_clearTrackingOfUniqueIdentifiersWhenCountIs"/>.</summary>
    public HashSet<RenderStateKey> SeenModelRenderStateKeys { get; init; } = new();
    /// <summary>In order to prevent infinite loops, track the unique identifiers. Note, this HashSet is cleared when the count is >= <see cref="_clearTrackingOfUniqueIdentifiersWhenCountIs"/>.</summary>
    public HashSet<RenderStateKey> SeenOptionsRenderStateKeys { get; init; } = new();

    public string CommandBarValue { get; set; } = string.Empty;
    public bool ShouldSetFocusAfterNextRender { get; set; }


    public string BodyElementId => $"luth_te_text-editor-content_{ViewModelKey.Guid}";
    public string PrimaryCursorContentId => $"luth_te_text-editor-content_{ViewModelKey.Guid}_primary-cursor";
    public string GutterElementId => $"luth_te_text-editor-gutter_{ViewModelKey.Guid}";

    public void CursorMovePageTop()
    {
        var localMostRecentlyRenderedVirtualizationResult = VirtualizationResult;

        if (localMostRecentlyRenderedVirtualizationResult?.Entries.Any() ?? false)
        {
            var firstEntry = localMostRecentlyRenderedVirtualizationResult.Entries.First();

            PrimaryCursor.IndexCoordinates = (firstEntry.Index, 0);
        }
    }

    public void CursorMovePageBottom()
    {
        var localMostRecentlyRenderedVirtualizationResult = VirtualizationResult;

        var textEditor = TextEditorService.ViewModel.FindBackingModelOrDefault(
            ViewModelKey);

        if (textEditor is not null &&
            (localMostRecentlyRenderedVirtualizationResult?.Entries.Any() ?? false))
        {
            var lastEntry = localMostRecentlyRenderedVirtualizationResult.Entries.Last();

            var lastEntriesRowLength = textEditor.GetLengthOfRow(lastEntry.Index);

            PrimaryCursor.IndexCoordinates = (lastEntry.Index, lastEntriesRowLength);
        }
    }

    public async Task MutateScrollHorizontalPositionByPixelsAsync(double pixels)
    {
        _batchScrollEvents.MutateScrollHorizontalPositionByPixels += pixels;

        await _batchScrollEvents.ThrottleMutateScrollHorizontalPositionByPixels.FireAsync(async () =>
        {
            var batch = _batchScrollEvents.MutateScrollHorizontalPositionByPixels;
            _batchScrollEvents.MutateScrollHorizontalPositionByPixels -= batch;

            await TextEditorService.ViewModel.MutateScrollHorizontalPositionAsync(
                BodyElementId,
                GutterElementId,
                batch);
        });
    }

    public async Task MutateScrollVerticalPositionByPixelsAsync(double pixels)
    {
        _batchScrollEvents.MutateScrollVerticalPositionByPixels += pixels;

        await _batchScrollEvents.ThrottleMutateScrollVerticalPositionByPixels.FireAsync(async () =>
        {
            var batch = _batchScrollEvents.MutateScrollVerticalPositionByPixels;
            _batchScrollEvents.MutateScrollVerticalPositionByPixels -= batch;

            await TextEditorService.ViewModel.MutateScrollVerticalPositionAsync(
                BodyElementId,
                GutterElementId,
                batch);
        });
    }

    public async Task MutateScrollVerticalPositionByPagesAsync(double pages)
    {
        await MutateScrollVerticalPositionByPixelsAsync(
            pages * _mostRecentBodyMeasurementsInPixels.Height);
    }

    public async Task MutateScrollVerticalPositionByLinesAsync(double lines)
    {
        await MutateScrollVerticalPositionByPixelsAsync(
            lines * VirtualizationResult.CharacterWidthAndRowHeight.RowHeightInPixels);
    }

    /// <summary>If a parameter is null the JavaScript will not modify that value</summary>
    public async Task SetScrollPositionAsync(double? scrollLeft, double? scrollTop)
    {
        await _batchScrollEvents.ThrottleSetScrollPosition.FireAsync(async () =>
        {
            await TextEditorService.ViewModel.SetScrollPositionAsync(
                BodyElementId,
                GutterElementId,
                scrollLeft,
                scrollTop);
        });
    }

    public async Task FocusAsync()
    {
        await TextEditorService.ViewModel.FocusPrimaryCursorAsync(
            PrimaryCursorContentId);
    }

    public async Task RemeasureAsync(
        TextEditorOptions options,
        string measureCharacterWidthAndRowHeightElementId,
        int countOfTestCharacters,
        CancellationToken cancellationToken)
    {
        await ThrottleRemeasure.FireAsync(async () =>
        {
            lock (_trackingOfUniqueIdentifiersLock)
            {
                if (SeenOptionsRenderStateKeys.Contains(options.RenderStateKey))
                    return;
            }

            var characterWidthAndRowHeight = await TextEditorService.ViewModel.MeasureCharacterWidthAndRowHeightAsync(
                measureCharacterWidthAndRowHeightElementId,
                countOfTestCharacters);

            VirtualizationResult.CharacterWidthAndRowHeight = characterWidthAndRowHeight;

            lock (_trackingOfUniqueIdentifiersLock)
            {
                if (SeenOptionsRenderStateKeys.Count > _clearTrackingOfUniqueIdentifiersWhenCountIs)
                    SeenOptionsRenderStateKeys.Clear();

                SeenOptionsRenderStateKeys.Add(options.RenderStateKey);
            }

            TextEditorService.ViewModel.With(
                ViewModelKey,
                previousViewModel => previousViewModel with
                {
                    // Clear the SeenModelRenderStateKeys because one needs to recalculate the virtualization result now that the options have changed.
                    SeenModelRenderStateKeys = new(), 
                    VirtualizationResult = previousViewModel.VirtualizationResult with
                    {
                        CharacterWidthAndRowHeight = characterWidthAndRowHeight
                    }
                });
        });
    }

    public async Task CalculateVirtualizationResultAsync(
        TextEditorModel? model,
        ElementMeasurementsInPixels? bodyMeasurementsInPixels,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;

        await ThrottleCalculateVirtualizationResult.FireAsync(async () =>
        {
            if (model is null)
                return;

            lock (_trackingOfUniqueIdentifiersLock)
            {
                if (SeenModelRenderStateKeys.Contains(model.RenderStateKey))
                    return;
            }

            var localCharacterWidthAndRowHeight = VirtualizationResult.CharacterWidthAndRowHeight;

            if (bodyMeasurementsInPixels is null)
            {
                bodyMeasurementsInPixels = await TextEditorService.ViewModel
                    .MeasureElementInPixelsAsync(BodyElementId);
            }

            _mostRecentBodyMeasurementsInPixels = bodyMeasurementsInPixels;

            bodyMeasurementsInPixels = bodyMeasurementsInPixels with
            {
                MeasurementsExpiredCancellationToken = cancellationToken
            };

            var verticalStartingIndex = (int)Math.Floor(
                bodyMeasurementsInPixels.ScrollTop /
                localCharacterWidthAndRowHeight.RowHeightInPixels);

            var verticalTake = (int)Math.Ceiling(
                bodyMeasurementsInPixels.Height /
                localCharacterWidthAndRowHeight.RowHeightInPixels);

            // Vertical Padding (render some offscreen data)
            {
                verticalTake += 1;
            }

            // Check index boundaries
            {
                verticalStartingIndex = Math.Max(0, verticalStartingIndex);


                if (verticalStartingIndex + verticalTake >
                    model.RowEndingPositions.Length)
                {
                    verticalTake = model.RowEndingPositions.Length -
                                    verticalStartingIndex;
                }

                verticalTake = Math.Max(0, verticalTake);
            }

            var horizontalStartingIndex = (int)Math.Floor(
                bodyMeasurementsInPixels.ScrollLeft /
                localCharacterWidthAndRowHeight.CharacterWidthInPixels);

            var horizontalTake = (int)Math.Ceiling(
                bodyMeasurementsInPixels.Width /
                localCharacterWidthAndRowHeight.CharacterWidthInPixels);

            var virtualizedEntries = model
                .GetRows(verticalStartingIndex, verticalTake)
                .Select((row, index) =>
                {
                    index += verticalStartingIndex;

                    var localHorizontalStartingIndex = horizontalStartingIndex;
                    var localHorizontalTake = horizontalTake;

                    // Adjust for tab key width
                    {
                        var maxValidColumnIndex = row.Count - 1;

                        var parameterForGetTabsCountOnSameRowBeforeCursor =
                            localHorizontalStartingIndex > maxValidColumnIndex
                                ? maxValidColumnIndex
                                : localHorizontalStartingIndex;

                        var tabsOnSameRowBeforeCursor = model
                            .GetTabsCountOnSameRowBeforeCursor(
                                index,
                                parameterForGetTabsCountOnSameRowBeforeCursor);

                        // 1 of the character width is already accounted for
                        var extraWidthPerTabKey = TextEditorModel.TAB_WIDTH - 1;

                        localHorizontalStartingIndex -= extraWidthPerTabKey * tabsOnSameRowBeforeCursor;
                    }

                    if (localHorizontalStartingIndex + localHorizontalTake > row.Count)
                        localHorizontalTake = row.Count - localHorizontalStartingIndex;

                    localHorizontalTake = Math.Max(0, localHorizontalTake);

                    var horizontallyVirtualizedRow = row
                        .Skip(localHorizontalStartingIndex)
                        .Take(localHorizontalTake)
                        .ToList();

                    var widthInPixels =
                        horizontallyVirtualizedRow.Count *
                        localCharacterWidthAndRowHeight.CharacterWidthInPixels;

                    var leftInPixels =
                        // do not change this to localHorizontalStartingIndex
                        horizontalStartingIndex *
                        localCharacterWidthAndRowHeight.CharacterWidthInPixels;

                    var topInPixels =
                        index *
                        localCharacterWidthAndRowHeight.RowHeightInPixels;

                    return new VirtualizationEntry<List<RichCharacter>>(
                        index,
                        horizontallyVirtualizedRow,
                        widthInPixels,
                        localCharacterWidthAndRowHeight.RowHeightInPixels,
                        leftInPixels,
                        topInPixels);
                }).ToImmutableArray();

            var totalWidth =
                model.MostCharactersOnASingleRowTuple.rowLength *
                localCharacterWidthAndRowHeight.CharacterWidthInPixels;

            var totalHeight =
                model.RowEndingPositions.Length *
                localCharacterWidthAndRowHeight.RowHeightInPixels;

            // Add vertical margin so the user can scroll beyond the final row of content
            double marginScrollHeight;
            {
                var percentOfMarginScrollHeightByPageUnit = 0.4;

                marginScrollHeight = bodyMeasurementsInPixels.Height *
                                        percentOfMarginScrollHeightByPageUnit;

                totalHeight += marginScrollHeight;
            }

            var leftBoundaryWidthInPixels =
                horizontalStartingIndex *
                localCharacterWidthAndRowHeight.CharacterWidthInPixels;

            var leftBoundary = new VirtualizationBoundary(
                leftBoundaryWidthInPixels,
                totalHeight,
                0,
                0);

            var rightBoundaryLeftInPixels =
                leftBoundary.WidthInPixels +
                localCharacterWidthAndRowHeight.CharacterWidthInPixels *
                horizontalTake;

            var rightBoundaryWidthInPixels =
                totalWidth -
                rightBoundaryLeftInPixels;

            var rightBoundary = new VirtualizationBoundary(
                rightBoundaryWidthInPixels,
                totalHeight,
                rightBoundaryLeftInPixels,
                0);

            var topBoundaryHeightInPixels =
                verticalStartingIndex *
                localCharacterWidthAndRowHeight.RowHeightInPixels;

            var topBoundary = new VirtualizationBoundary(
                totalWidth,
                topBoundaryHeightInPixels,
                0,
                0);

            var bottomBoundaryTopInPixels =
                topBoundary.HeightInPixels +
                localCharacterWidthAndRowHeight.RowHeightInPixels *
                verticalTake;

            var bottomBoundaryHeightInPixels =
                totalHeight -
                bottomBoundaryTopInPixels;

            var bottomBoundary = new VirtualizationBoundary(
                totalWidth,
                bottomBoundaryHeightInPixels,
                0,
                bottomBoundaryTopInPixels);

            var virtualizationResult = new VirtualizationResult<List<RichCharacter>>(
                virtualizedEntries,
                leftBoundary,
                rightBoundary,
                topBoundary,
                bottomBoundary,
                bodyMeasurementsInPixels with
                {
                    ScrollWidth = totalWidth,
                    ScrollHeight = totalHeight,
                    MarginScrollHeight = marginScrollHeight
                },
                localCharacterWidthAndRowHeight);

            lock (_trackingOfUniqueIdentifiersLock)
            {
                if (SeenModelRenderStateKeys.Count > _clearTrackingOfUniqueIdentifiersWhenCountIs)
                    SeenModelRenderStateKeys.Clear();

                SeenModelRenderStateKeys.Add(model.RenderStateKey);
            }

            TextEditorService.ViewModel.With(
                ViewModelKey,
                previousViewModel => previousViewModel with
                {
                    VirtualizationResult = virtualizationResult,
                });
        });
    }

    public void UpdateSemanticPresentationModel()
    {
        TextEditorService.ViewModel.With(
            ViewModelKey,
            inViewModel =>
            {
                var outPresentationLayer = inViewModel.FirstPresentationLayer;

                var inPresentationModel = outPresentationLayer
                    .FirstOrDefault(x =>
                        x.TextEditorPresentationKey == SemanticFacts.PresentationKey);

                if (inPresentationModel is null)
                {
                    inPresentationModel = SemanticFacts.EmptyPresentationModel;

                    outPresentationLayer = outPresentationLayer.Add(
                        inPresentationModel);
                }

                var model = TextEditorService.ViewModel
                    .FindBackingModelOrDefault(ViewModelKey);

                var outPresentationModel = inPresentationModel with
                {
                    TextEditorTextSpans = model?.SemanticModel?.SemanticResult?.DiagnosticTextSpanTuples.Select(x => x.textSpan).ToImmutableList()
                        ?? ImmutableList<TextEditorTextSpan>.Empty
                };

                outPresentationLayer = outPresentationLayer.Replace(
                    inPresentationModel,
                    outPresentationModel);

                return inViewModel with
                {
                    FirstPresentationLayer = outPresentationLayer,
                };
            });
    }
}
