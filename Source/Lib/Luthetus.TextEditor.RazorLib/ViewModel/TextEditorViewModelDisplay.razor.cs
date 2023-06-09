﻿using System.Collections.Immutable;
using Fluxor;
using Luthetus.Common.RazorLib.Clipboard;
using Luthetus.Common.RazorLib.Dimensions;
using Luthetus.Common.RazorLib.JavaScriptObjects;
using Luthetus.Common.RazorLib.Keyboard;
using Luthetus.Common.RazorLib.Misc;
using Luthetus.TextEditor.RazorLib.Commands;
using Luthetus.TextEditor.RazorLib.Model;
using Luthetus.TextEditor.RazorLib.Store.Model;
using Luthetus.TextEditor.RazorLib.Store.Options;
using Luthetus.TextEditor.RazorLib.Store.ViewModel;
using Luthetus.TextEditor.RazorLib.Autocomplete;
using Luthetus.TextEditor.RazorLib.Commands.Default;
using Luthetus.TextEditor.RazorLib.Cursor;
using Luthetus.TextEditor.RazorLib.HelperComponents;
using Luthetus.TextEditor.RazorLib.Options;
using Luthetus.TextEditor.RazorLib.ViewModel.InternalComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Luthetus.Common.RazorLib.Reactive;
using Luthetus.Common.RazorLib.BackgroundTaskCase.BaseTypes;
using Luthetus.TextEditor.RazorLib.HostedServiceCase.TextEditorCase;

namespace Luthetus.TextEditor.RazorLib.ViewModel;

public partial class TextEditorViewModelDisplay : ComponentBase, IDisposable
{
    [Inject]
    protected IState<TextEditorModelsCollection> ModelsCollectionWrap { get; set; } = null!;
    [Inject]
    protected IState<TextEditorViewModelsCollection> ViewModelsCollectionWrap { get; set; } = null!;
    [Inject]
    protected IState<TextEditorOptionsState> GlobalOptionsWrap { get; set; } = null!;
    [Inject]
    protected ITextEditorService TextEditorService { get; set; } = null!;
    [Inject]
    private IAutocompleteIndexer AutocompleteIndexer { get; set; } = null!;
    [Inject]
    private IDispatcher Dispatcher { get; set; } = null!;
    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;
    [Inject]
    private IClipboardService ClipboardService { get; set; } = null!;
    [Inject]
    private ITextEditorBackgroundTaskQueue TextEditorBackgroundTaskQueue { get; set; } = null!;

    [Parameter, EditorRequired]
    public TextEditorViewModelKey TextEditorViewModelKey { get; set; } = null!;
    [Parameter]
    public string WrapperStyleCssString { get; set; } = string.Empty;
    [Parameter]
    public string WrapperClassCssString { get; set; } = string.Empty;
    [Parameter]
    public string TextEditorStyleCssString { get; set; } = string.Empty;
    [Parameter]
    public string TextEditorClassCssString { get; set; } = string.Empty;
    /// <summary>TabIndex is used for the html attribute named: 'tabindex'</summary>
    [Parameter]
    public int TabIndex { get; set; } = -1;
    [Parameter]
    public RenderFragment? ContextMenuRenderFragmentOverride { get; set; }
    [Parameter]
    public RenderFragment? AutoCompleteMenuRenderFragmentOverride { get; set; }
    /// <summary>If left null, the default <see cref="HandleAfterOnKeyDownAsync"/> will be used.</summary>
    [Parameter]
    public Func<TextEditorModel, ImmutableArray<TextEditorCursorSnapshot>, KeyboardEventArgs, Func<TextEditorMenuKind, bool, Task>, Task>? AfterOnKeyDownAsync { get; set; }
    /// <summary>If set to false the <see cref="TextEditorHeader"/> will NOT render above the text editor.</summary>
    [Parameter]
    public bool IncludeHeaderHelperComponent { get; set; } = true;
    /// <summary><see cref="HeaderButtonKinds"/> contains the enum value that represents a button displayed in the optional component: <see cref="TextEditorHeader"/>.</summary>
    [Parameter]
    public ImmutableArray<TextEditorHeaderButtonKind>? HeaderButtonKinds { get; set; }
    /// <summary>If set to false the <see cref="TextEditorFooter"/> will NOT render below the text editor.</summary>
    [Parameter]
    public bool IncludeFooterHelperComponent { get; set; } = true;
    [Parameter]
    public bool IncludeContextMenuHelperComponent { get; set; } = true;

    private readonly Guid _textEditorHtmlElementId = Guid.NewGuid();
    private readonly IThrottle _throttleApplySyntaxHighlighting = new Throttle(TimeSpan.FromMilliseconds(500));
    private readonly SemaphoreSlim _generalOnStateChangedEventHandlerSemaphoreSlim = new(1, 1);
    private readonly TimeSpan _mouseStoppedMovingDelay = TimeSpan.FromMilliseconds(400);
    /// <summary>Using this lock in order to avoid the Dispose implementation decrementing when it shouldn't</summary>
    private readonly object _toRenderViewModelDataLock = new();

    /// <summary>This accounts for one who might hold down Left Mouse Button from outside the TextEditorDisplay's content div then move their mouse over the content div while holding the Left Mouse Button down.</summary>
    private bool _thinksLeftMouseButtonIsDown;
    private bool _thinksTouchIsOccurring;
    private TouchEventArgs? _previousTouchEventArgs = null;
    private DateTime? _touchStartDateTime = null;
    private BodySection? _bodySectionComponent;
    private MeasureCharacterWidthAndRowHeight? _measureCharacterWidthAndRowHeightComponent;
    private Task _mouseStoppedMovingTask = Task.CompletedTask;
    private CancellationTokenSource _mouseStoppedMovingCancellationTokenSource = new();
    private (string message, RelativeCoordinates relativeCoordinates)? _mouseStoppedEventMostRecentResult;
    private bool _userMouseIsInside;
    private ToRenderViewModelData? _toRenderViewModelData;
    private TextEditorRenderBatch? _currentRenderBatch;
    private TextEditorRenderBatch? _previousRenderBatch;

    private TextEditorCursorDisplay? TextEditorCursorDisplay => _bodySectionComponent?.TextEditorCursorDisplayComponent;
    private string MeasureCharacterWidthAndRowHeightElementId => $"luth_te_measure-character-width-and-row-height_{_textEditorHtmlElementId}";
    private string ContentElementId => $"luth_te_text-editor-content_{_textEditorHtmlElementId}";
    private string ProportionalFontMeasurementsContainerElementId => $"luth_te_text-editor-proportional-font-measurement-container_{_textEditorHtmlElementId}";

    protected override async Task OnParametersSetAsync()
    {
        var nextViewModel = GetViewModel();

        if (nextViewModel is not null)
        {
            lock (_toRenderViewModelDataLock)
            {
                if (_toRenderViewModelData is not null &&
                    _toRenderViewModelData.ViewModelKey != nextViewModel.ViewModelKey)
                {
                    _toRenderViewModelData.DisplayTracker.DecrementLinks(ModelsCollectionWrap);
                }

                _toRenderViewModelData = new(nextViewModel.ViewModelKey, nextViewModel.DisplayTracker);
                _toRenderViewModelData.DisplayTracker.IncrementLinks(ModelsCollectionWrap);
            }

            nextViewModel.PrimaryCursor.ShouldRevealCursor = true;
        }

        await base.OnParametersSetAsync();
    }

    protected override void OnInitialized()
    {
        ViewModelsCollectionWrap.StateChanged += GeneralOnStateChangedEventHandler;
        GlobalOptionsWrap.StateChanged += GeneralOnStateChangedEventHandler;

        base.OnInitialized();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // TODO: I'm unsure if another render could occur while this 'OnAfterRenderAsync' is being invoked.
        // Therefore I'm storing the previous and current RenderBatch(s) locally.
        var localRefPreviousRenderBatch = _previousRenderBatch;
        var localRefCurrentRenderBatch = _currentRenderBatch;

        if (firstRender)
        {
            await JsRuntime.InvokeVoidAsync(
                "luthetusTextEditor.preventDefaultOnWheelEvents",
                ContentElementId);
        }

        if (localRefCurrentRenderBatch?.ViewModel is not null &&
            localRefCurrentRenderBatch?.Options is not null)
        {
            var pendingNeedForBecameDisplayedCalculations = false;

            if (_toRenderViewModelData is not null)
            {
                pendingNeedForBecameDisplayedCalculations = _toRenderViewModelData.DisplayTracker
                    .ConsumePendingNeedForBecameDisplayedCalculations();
            }

            var previousOptionsRenderStateKey = localRefPreviousRenderBatch?.Options?.RenderStateKey ?? RenderStateKey.Empty;
            var currentOptionsRenderStateKey = localRefCurrentRenderBatch.Options.RenderStateKey;

            if (previousOptionsRenderStateKey != currentOptionsRenderStateKey ||
                pendingNeedForBecameDisplayedCalculations)
            {
                QueueRemeasureBackgroundTask(
                    localRefCurrentRenderBatch,
                    MeasureCharacterWidthAndRowHeightElementId,
                    _measureCharacterWidthAndRowHeightComponent?.CountOfTestCharacters ?? 0,
                    CancellationToken.None);
            }

            if (pendingNeedForBecameDisplayedCalculations)
            {
                QueueCalculateVirtualizationResultBackgroundTask(localRefCurrentRenderBatch);
                QueueUpdateSemanticPresentationModelBackgroundTask(localRefCurrentRenderBatch);
            }
        }

        if (localRefCurrentRenderBatch?.ViewModel is not null &&
            localRefCurrentRenderBatch.ViewModel.ShouldSetFocusAfterNextRender)
        {
            localRefCurrentRenderBatch.ViewModel.ShouldSetFocusAfterNextRender = false;
            await FocusTextEditorAsync();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    public TextEditorModel? GetModel() => TextEditorService.ViewModel
        .FindBackingModelOrDefault(TextEditorViewModelKey);

    public TextEditorViewModel? GetViewModel() => ViewModelsCollectionWrap.Value.ViewModelsList
        .FirstOrDefault(x => x.ViewModelKey == TextEditorViewModelKey);

    public TextEditorOptions? GetOptions() => GlobalOptionsWrap.Value.Options;

    private async void GeneralOnStateChangedEventHandler(object? sender, EventArgs e)
    {
        await InvokeAsync(StateHasChanged);
    }

    public async Task FocusTextEditorAsync()
    {
        if (TextEditorCursorDisplay is not null)
            await TextEditorCursorDisplay.FocusAsync();
    }

    private async Task HandleOnKeyDownAsync(KeyboardEventArgs keyboardEventArgs)
    {
        if (keyboardEventArgs.Key == "Shift" ||
            keyboardEventArgs.Key == "Control" ||
            keyboardEventArgs.Key == "Alt" ||
            keyboardEventArgs.Key == "Meta")
        {
            return;
        }

        var model = GetModel();
        var viewModel = GetViewModel();

        if (model is null ||
            viewModel is null)
        {
            return;
        }

        var primaryCursorSnapshot = new TextEditorCursorSnapshot(viewModel.PrimaryCursor);

        var cursorSnapshots = new TextEditorCursorSnapshot[]
        {
            new(primaryCursorSnapshot.UserCursor),
        }.ToImmutableArray();

        var hasSelection = TextEditorSelectionHelper
            .HasSelectedText(
                primaryCursorSnapshot.ImmutableCursor.ImmutableSelection);

        var command = TextEditorService
            .OptionsWrap.Value.Options.KeymapDefinition!.Keymap.Map(
                keyboardEventArgs,
                hasSelection);

        if (KeyboardKeyFacts.WhitespaceCodes.ENTER_CODE == keyboardEventArgs.Code &&
            keyboardEventArgs.ShiftKey)
        {
            command = TextEditorCommandDefaultFacts.NewLineBelow;
        }

        if (KeyboardKeyFacts.IsMovementKey(keyboardEventArgs.Key) &&
            command is null)
        {
            if ((KeyboardKeyFacts.MovementKeys.ARROW_DOWN == keyboardEventArgs.Key ||
                 KeyboardKeyFacts.MovementKeys.ARROW_UP == keyboardEventArgs.Key) &&
                TextEditorCursorDisplay is not null &&
                TextEditorCursorDisplay.TextEditorMenuKind ==
                TextEditorMenuKind.AutoCompleteMenu)
            {
                await TextEditorCursorDisplay.SetFocusToActiveMenuAsync();
            }
            else
            {
                TextEditorCursor.MoveCursor(
                    keyboardEventArgs,
                    primaryCursorSnapshot.UserCursor,
                    model);

                TextEditorCursorDisplay?.SetShouldDisplayMenuAsync(TextEditorMenuKind.None);
            }
        }
        else if (KeyboardKeyFacts.CheckIsContextMenuEvent(keyboardEventArgs))
        {
            TextEditorCursorDisplay?.SetShouldDisplayMenuAsync(TextEditorMenuKind.ContextMenu);
        }
        else
        {
            if (command is not null)
            {
                await command.DoAsyncFunc.Invoke(
                    new TextEditorCommandParameter(
                        model,
                        cursorSnapshots,
                        ClipboardService,
                        TextEditorService,
                        viewModel));
            }
            else
            {
                if (!IsAutocompleteMenuInvoker(keyboardEventArgs))
                {
                    if (!KeyboardKeyFacts.IsMetaKey(keyboardEventArgs)
                        || (KeyboardKeyFacts.MetaKeys.ESCAPE == keyboardEventArgs.Key ||
                            KeyboardKeyFacts.MetaKeys.BACKSPACE == keyboardEventArgs.Key ||
                            KeyboardKeyFacts.MetaKeys.DELETE == keyboardEventArgs.Key))
                    {
                        TextEditorCursorDisplay?.SetShouldDisplayMenuAsync(TextEditorMenuKind.None);
                    }
                }

                _mouseStoppedEventMostRecentResult = null;

                var backgroundTask = new BackgroundTask(
                    cancellationToken =>
                    {
                        Dispatcher.Dispatch(
                            new TextEditorModelsCollection.KeyboardEventAction(
                            viewModel.ModelKey,
                            cursorSnapshots,
                            keyboardEventArgs,
                            CancellationToken.None));

                        return Task.CompletedTask;
                    },
                    "HandleOnKeyDownAsync",
                    "TODO: Describe this task",
                    false,
                    _ => Task.CompletedTask,
                    Dispatcher,
                    CancellationToken.None);

                TextEditorBackgroundTaskQueue.QueueBackgroundWorkItem(backgroundTask);
            }
        }

        if (keyboardEventArgs.Key != "Shift" &&
            keyboardEventArgs.Key != "Control" &&
            keyboardEventArgs.Key != "Alt" &&
            (command?.ShouldScrollCursorIntoView ?? true))
        {
            primaryCursorSnapshot.UserCursor.ShouldRevealCursor = true;
        }

        var afterOnKeyDownAsync = AfterOnKeyDownAsync
                                  ?? HandleAfterOnKeyDownAsync;

        var cursorDisplay = TextEditorCursorDisplay;

        if (cursorDisplay is not null)
        {
            var textEditor = model;

            await afterOnKeyDownAsync.Invoke(
                textEditor,
                cursorSnapshots,
                keyboardEventArgs,
                cursorDisplay.SetShouldDisplayMenuAsync);
        }
    }

    private void HandleOnContextMenuAsync()
    {
        TextEditorCursorDisplay?.SetShouldDisplayMenuAsync(TextEditorMenuKind.ContextMenu);
    }

    private async Task HandleContentOnDoubleClickAsync(MouseEventArgs mouseEventArgs)
    {
        var safeRefModel = GetModel();
        var safeRefViewModel = GetViewModel();

        if (safeRefModel is null ||
            safeRefViewModel is null)
            return;

        var primaryCursorSnapshot = new TextEditorCursorSnapshot(safeRefViewModel.PrimaryCursor);

        if ((mouseEventArgs.Buttons & 1) != 1 &&
            TextEditorSelectionHelper.HasSelectedText(
                primaryCursorSnapshot.ImmutableCursor.ImmutableSelection))
            // Not pressing the left mouse button
            // so assume ContextMenu is desired result.
            return;

        if (mouseEventArgs.ShiftKey)
            // Do not expand selection if user is holding shift
            return;

        var rowAndColumnIndex =
            await CalculateRowAndColumnIndex(mouseEventArgs);

        var lowerColumnIndexExpansion = safeRefModel
            .GetColumnIndexOfCharacterWithDifferingKind(
                rowAndColumnIndex.rowIndex,
                rowAndColumnIndex.columnIndex,
                true);

        lowerColumnIndexExpansion =
            lowerColumnIndexExpansion == -1
                ? 0
                : lowerColumnIndexExpansion;

        var higherColumnIndexExpansion = safeRefModel
            .GetColumnIndexOfCharacterWithDifferingKind(
                rowAndColumnIndex.rowIndex,
                rowAndColumnIndex.columnIndex,
                false);

        higherColumnIndexExpansion =
            higherColumnIndexExpansion == -1
                ? safeRefModel.GetLengthOfRow(
                    rowAndColumnIndex.rowIndex)
                : higherColumnIndexExpansion;

        // Move user's cursor position to the higher expansion
        {
            primaryCursorSnapshot.UserCursor.IndexCoordinates =
                (rowAndColumnIndex.rowIndex, higherColumnIndexExpansion);

            primaryCursorSnapshot.UserCursor.PreferredColumnIndex =
                rowAndColumnIndex.columnIndex;
        }

        // Set text selection ending to higher expansion
        {
            var cursorPositionOfHigherExpansion = safeRefModel
                .GetPositionIndex(
                    rowAndColumnIndex.rowIndex,
                    higherColumnIndexExpansion);

            primaryCursorSnapshot
                    .UserCursor.Selection.EndingPositionIndex =
                cursorPositionOfHigherExpansion;
        }

        // Set text selection anchor to lower expansion
        {
            var cursorPositionOfLowerExpansion = safeRefModel
                .GetPositionIndex(
                    rowAndColumnIndex.rowIndex,
                    lowerColumnIndexExpansion);

            primaryCursorSnapshot
                    .UserCursor.Selection.AnchorPositionIndex =
                cursorPositionOfLowerExpansion;
        }
    }

    private async Task HandleContentOnMouseDownAsync(MouseEventArgs mouseEventArgs)
    {
        var safeRefModel = GetModel();
        var safeRefViewModel = GetViewModel();

        if (safeRefModel is null ||
            safeRefViewModel is null)
            return;

        var primaryCursorSnapshot = new TextEditorCursorSnapshot(safeRefViewModel.PrimaryCursor);

        if ((mouseEventArgs.Buttons & 1) != 1 &&
            TextEditorSelectionHelper.HasSelectedText(
                primaryCursorSnapshot.ImmutableCursor.ImmutableSelection))
            // Not pressing the left mouse button
            // so assume ContextMenu is desired result.
            return;

        TextEditorCursorDisplay?.SetShouldDisplayMenuAsync(
            TextEditorMenuKind.None,
            false);

        var rowAndColumnIndex =
            await CalculateRowAndColumnIndex(mouseEventArgs);

        primaryCursorSnapshot.UserCursor.IndexCoordinates =
            (rowAndColumnIndex.rowIndex, rowAndColumnIndex.columnIndex);
        primaryCursorSnapshot.UserCursor.PreferredColumnIndex =
            rowAndColumnIndex.columnIndex;

        TextEditorCursorDisplay?.PauseBlinkAnimation();

        var cursorPositionIndex = safeRefModel
            .GetCursorPositionIndex(
                new TextEditorCursor(rowAndColumnIndex, false));

        if (mouseEventArgs.ShiftKey)
        {
            if (!TextEditorSelectionHelper.HasSelectedText(
                    primaryCursorSnapshot.ImmutableCursor.ImmutableSelection))
            {
                // If user does not yet have a selection
                // then place the text selection anchor were they were

                var cursorPositionPriorToMovementOccurring = safeRefModel
                    .GetPositionIndex(
                        primaryCursorSnapshot.ImmutableCursor.RowIndex,
                        primaryCursorSnapshot.ImmutableCursor.ColumnIndex);

                primaryCursorSnapshot.UserCursor.Selection.AnchorPositionIndex =
                    cursorPositionPriorToMovementOccurring;
            }

            // If user ALREADY has a selection
            // then do not modify the text selection anchor
        }
        else
        {
            primaryCursorSnapshot.UserCursor.Selection.AnchorPositionIndex =
                cursorPositionIndex;
        }

        primaryCursorSnapshot.UserCursor.Selection.EndingPositionIndex =
            cursorPositionIndex;

        _thinksLeftMouseButtonIsDown = true;
    }

    /// <summary>OnMouseUp is unnecessary</summary>
    private async Task HandleContentOnMouseMoveAsync(MouseEventArgs mouseEventArgs)
    {
        _userMouseIsInside = true;

        var localThinksLeftMouseButtonIsDown = _thinksLeftMouseButtonIsDown;

        // MouseStoppedMovingEvent
        {
            _mouseStoppedMovingCancellationTokenSource.Cancel();
            _mouseStoppedMovingCancellationTokenSource = new();

            var cancellationToken = _mouseStoppedMovingCancellationTokenSource.Token;

            _mouseStoppedMovingTask = Task.Run(async () =>
            {
                await Task.Delay(_mouseStoppedMovingDelay, cancellationToken);

                if (!cancellationToken.IsCancellationRequested &&
                    _userMouseIsInside)
                {
                    await HandleMouseStoppedMovingEventAsync(mouseEventArgs);
                }
            });
        }

        if (!_thinksLeftMouseButtonIsDown)
            return;

        var safeRefModel = GetModel();
        var safeRefViewModel = GetViewModel();

        if (safeRefModel is null ||
            safeRefViewModel is null)
            return;

        var primaryCursorSnapshot = new TextEditorCursorSnapshot(safeRefViewModel.PrimaryCursor);

        // Buttons is a bit flag
        // '& 1' gets if left mouse button is held
        if (localThinksLeftMouseButtonIsDown &&
            (mouseEventArgs.Buttons & 1) == 1)
        {
            var rowAndColumnIndex =
                await CalculateRowAndColumnIndex(mouseEventArgs);

            primaryCursorSnapshot.UserCursor.IndexCoordinates =
                (rowAndColumnIndex.rowIndex, rowAndColumnIndex.columnIndex);
            primaryCursorSnapshot.UserCursor.PreferredColumnIndex =
                rowAndColumnIndex.columnIndex;

            TextEditorCursorDisplay?.PauseBlinkAnimation();

            primaryCursorSnapshot.UserCursor.Selection.EndingPositionIndex =
                safeRefModel
                    .GetCursorPositionIndex(
                        new TextEditorCursor(rowAndColumnIndex, false));
        }
        else
        {
            _thinksLeftMouseButtonIsDown = false;
        }
    }

    private void HandleContentOnMouseOut(MouseEventArgs mouseEventArgs)
    {
        _userMouseIsInside = false;
    }

    private async Task<(int rowIndex, int columnIndex)> CalculateRowAndColumnIndex(
        MouseEventArgs mouseEventArgs)
    {
        var safeRefModel = GetModel();
        var safeRefViewModel = GetViewModel();
        var globalTextEditorOptions = TextEditorService.OptionsWrap.Value.Options;

        if (safeRefModel is null ||
            safeRefViewModel is null)
            return (0, 0);

        var relativeCoordinatesOnClick = await JsRuntime
            .InvokeAsync<RelativeCoordinates>(
                "luthetusTextEditor.getRelativePosition",
                safeRefViewModel.BodyElementId,
                mouseEventArgs.ClientX,
                mouseEventArgs.ClientY);

        var positionX = relativeCoordinatesOnClick.RelativeX;
        var positionY = relativeCoordinatesOnClick.RelativeY;

        // Scroll position offset
        {
            positionX += relativeCoordinatesOnClick.RelativeScrollLeft;
            positionY += relativeCoordinatesOnClick.RelativeScrollTop;
        }

        var rowIndex = (int)(positionY /
                             safeRefViewModel.VirtualizationResult.CharacterWidthAndRowHeight.RowHeightInPixels);

        rowIndex = rowIndex > safeRefModel.RowCount - 1
            ? safeRefModel.RowCount - 1
            : rowIndex;

        int columnIndexInt;

        if (!globalTextEditorOptions.UseMonospaceOptimizations)
        {
            var guid = Guid.NewGuid();

            columnIndexInt = await JsRuntime.InvokeAsync<int>(
                "luthetusTextEditor.calculateProportionalColumnIndex",
                ProportionalFontMeasurementsContainerElementId,
                $"luth_te_proportional-font-measurement-parent_{_textEditorHtmlElementId}_{guid}",
                $"luth_te_proportional-font-measurement-cursor_{_textEditorHtmlElementId}_{guid}",
                positionX,
                safeRefViewModel.VirtualizationResult.CharacterWidthAndRowHeight.CharacterWidthInPixels,
                safeRefModel.GetTextOnRow(rowIndex));

            if (columnIndexInt == -1)
            {
                var columnIndexDouble = positionX /
                                        safeRefViewModel.VirtualizationResult.CharacterWidthAndRowHeight.CharacterWidthInPixels;

                columnIndexInt = (int)Math.Round(
                    columnIndexDouble,
                    MidpointRounding.AwayFromZero);
            }
        }
        else
        {
            var columnIndexDouble = positionX /
                                    safeRefViewModel.VirtualizationResult.CharacterWidthAndRowHeight.CharacterWidthInPixels;

            columnIndexInt = (int)Math.Round(
                columnIndexDouble,
                MidpointRounding.AwayFromZero);
        }

        var lengthOfRow = safeRefModel.GetLengthOfRow(rowIndex);

        // Tab key column offset
        {
            var parameterForGetTabsCountOnSameRowBeforeCursor =
                columnIndexInt > lengthOfRow
                    ? lengthOfRow
                    : columnIndexInt;

            var tabsOnSameRowBeforeCursor = safeRefModel
                .GetTabsCountOnSameRowBeforeCursor(
                    rowIndex,
                    parameterForGetTabsCountOnSameRowBeforeCursor);

            // 1 of the character width is already accounted for
            var extraWidthPerTabKey = TextEditorModel.TAB_WIDTH - 1;

            columnIndexInt -= extraWidthPerTabKey * tabsOnSameRowBeforeCursor;
        }

        columnIndexInt = columnIndexInt > lengthOfRow
            ? lengthOfRow
            : columnIndexInt;

        rowIndex = Math.Max(rowIndex, 0);
        columnIndexInt = Math.Max(columnIndexInt, 0);

        return (rowIndex, columnIndexInt);
    }

    /// <summary>The default <see cref="AfterOnKeyDownAsync"/> will provide syntax highlighting, and autocomplete.<br/><br/>The syntax highlighting occurs on ';', whitespace, paste, undo, redo<br/><br/>The autocomplete occurs on LetterOrDigit typed or { Ctrl + Space }. Furthermore, the autocomplete is done via <see cref="IAutocompleteService"/> and the one can provide their own implementation when registering the Luthetus.TextEditor services using <see cref="LuthetusTextEditorOptions.AutocompleteServiceFactory"/></summary>
    public async Task HandleAfterOnKeyDownAsync(
        TextEditorModel textEditor,
        ImmutableArray<TextEditorCursorSnapshot> cursorSnapshots,
        KeyboardEventArgs keyboardEventArgs,
        Func<TextEditorMenuKind, bool, Task> setTextEditorMenuKind)
    {
        var primaryCursorSnapshot = cursorSnapshots
            .First(x =>
                x.UserCursor.IsPrimaryCursor);

        // Indexing can be invoked and this method still check for syntax highlighting and such
        if (IsAutocompleteIndexerInvoker(keyboardEventArgs))
        {
            if (primaryCursorSnapshot.ImmutableCursor.ColumnIndex > 0)
            {
                // All keyboardEventArgs that return true from "IsAutocompleteIndexerInvoker"
                // are to be 1 character long, as well either specific whitespace or punctuation.
                //
                // Therefore 1 character behind might be a word that can be indexed.

                var word = textEditor.ReadPreviousWordOrDefault(
                    primaryCursorSnapshot.ImmutableCursor.RowIndex,
                    primaryCursorSnapshot.ImmutableCursor.ColumnIndex);

                if (word is not null)
                    await AutocompleteIndexer.IndexWordAsync(word);
            }
        }

        if (IsAutocompleteMenuInvoker(keyboardEventArgs))
        {
            await setTextEditorMenuKind.Invoke(
                TextEditorMenuKind.AutoCompleteMenu,
                true);
        }
        else if (IsSyntaxHighlightingInvoker(keyboardEventArgs))
        {
            await _throttleApplySyntaxHighlighting.FireAsync(async () =>
            {
                // The TextEditorModel may have been changed by the time this logic is ran and
                // thus the local variable must be updated accordingly.
                var model = GetModel();
                
                var viewModel = GetViewModel();

                if (model is not null)
                {
                    textEditor = model;

                    await textEditor.ApplySyntaxHighlightingAsync();

                    if (viewModel is not null && model.CompilerService is not null)
                    {
                        // TODO: (2023-06-29) I'm rewritting the TextEditor 'ISemanticModel.cs' to be 'ICompilerService.cs'. This method broke in the process and is not high priority to fix yet.
                    }
                }
            });
        }
    }

    private async Task HandleMouseStoppedMovingEventAsync(
        MouseEventArgs mouseEventArgs)
    {
        // TODO: (2023-06-29) I'm rewritting the TextEditor 'ISemanticModel.cs' to be 'ICompilerService.cs'. This method broke in the process and is not high priority to fix yet.
    }

    private bool IsSyntaxHighlightingInvoker(KeyboardEventArgs keyboardEventArgs)
    {
        return keyboardEventArgs.Key == ";" ||
               KeyboardKeyFacts.IsWhitespaceCode(keyboardEventArgs.Code) ||
               keyboardEventArgs.CtrlKey && keyboardEventArgs.Key == "s" ||
               keyboardEventArgs.CtrlKey && keyboardEventArgs.Key == "v" ||
               keyboardEventArgs.CtrlKey && keyboardEventArgs.Key == "z" ||
               keyboardEventArgs.CtrlKey && keyboardEventArgs.Key == "y";
    }

    private bool IsAutocompleteMenuInvoker(KeyboardEventArgs keyboardEventArgs)
    {
        // Is {Ctrl + Space} or LetterOrDigit was hit without Ctrl being held
        return keyboardEventArgs.CtrlKey && keyboardEventArgs.Code == KeyboardKeyFacts.WhitespaceCodes.SPACE_CODE ||
               !keyboardEventArgs.CtrlKey &&
                !KeyboardKeyFacts.IsWhitespaceCode(keyboardEventArgs.Code) &&
                !KeyboardKeyFacts.IsMetaKey(keyboardEventArgs);
    }

    /// <summary>
    /// All keyboardEventArgs that return true from "IsAutocompleteIndexerInvoker"
    /// are to be 1 character long, as well either whitespace or punctuation.
    ///
    /// Therefore 1 character behind might be a word that can be indexed.
    /// </summary>
    private bool IsAutocompleteIndexerInvoker(KeyboardEventArgs keyboardEventArgs)
    {
        return (KeyboardKeyFacts.IsWhitespaceCode(keyboardEventArgs.Code) ||
                KeyboardKeyFacts.IsPunctuationCharacter(keyboardEventArgs.Key.First())) &&
               !keyboardEventArgs.CtrlKey;
    }

    private async Task HandleOnWheelAsync(WheelEventArgs wheelEventArgs)
    {
        var textEditorViewModel = GetViewModel();

        if (textEditorViewModel is null)
            return;

        if (wheelEventArgs.ShiftKey)
        {
            await textEditorViewModel.MutateScrollHorizontalPositionByPixelsAsync(
                wheelEventArgs.DeltaY);
        }
        else
        {
            await textEditorViewModel.MutateScrollVerticalPositionByPixelsAsync(
                wheelEventArgs.DeltaY);
        }
    }

    private string GetGlobalHeightInPixelsStyling()
    {
        var heightInPixels = TextEditorService
            .OptionsWrap
            .Value
            .Options
            .TextEditorHeightInPixels;

        if (heightInPixels is null)
            return string.Empty;

        var heightInPixelsInvariantCulture = heightInPixels.Value
            .ToCssValue();

        return $"height: {heightInPixelsInvariantCulture}px;";
    }

    private Task HandleOnTouchStartAsync(TouchEventArgs touchEventArgs)
    {
        _touchStartDateTime = DateTime.UtcNow;

        _previousTouchEventArgs = touchEventArgs;
        _thinksTouchIsOccurring = true;

        return Task.CompletedTask;
    }

    private async Task HandleOnTouchMoveAsync(TouchEventArgs touchEventArgs)
    {
        var localThinksTouchIsOccurring = _thinksTouchIsOccurring;

        if (!_thinksTouchIsOccurring)
            return;

        var previousTouchPoint = _previousTouchEventArgs?.ChangedTouches
            .FirstOrDefault(x => x.Identifier == 0);

        var currentTouchPoint = touchEventArgs.ChangedTouches
            .FirstOrDefault(x => x.Identifier == 0);

        if (previousTouchPoint is null || currentTouchPoint is null)
            return;

        var viewModel = GetViewModel();

        if (viewModel is null)
            return;

        // Natural scrolling for touch devices
        var diffX = previousTouchPoint.ClientX - currentTouchPoint.ClientX;
        var diffY = previousTouchPoint.ClientY - currentTouchPoint.ClientY;

        await viewModel.MutateScrollHorizontalPositionByPixelsAsync(diffX);
        await viewModel.MutateScrollVerticalPositionByPixelsAsync(diffY);

        _previousTouchEventArgs = touchEventArgs;
    }

    private async Task ClearTouchAsync(TouchEventArgs touchEventArgs)
    {
        var rememberStartTouchEventArgs = _previousTouchEventArgs;

        _thinksTouchIsOccurring = false;
        _previousTouchEventArgs = null;

        var clearTouchDateTime = DateTime.UtcNow;

        var touchTimespan = clearTouchDateTime - _touchStartDateTime;

        if (touchTimespan is null)
            return;

        if (touchTimespan.Value.TotalMilliseconds < 200)
        {
            var startTouchPoint = rememberStartTouchEventArgs?.ChangedTouches
                .FirstOrDefault(x => x.Identifier == 0);

            if (startTouchPoint is null)
                return;

            await HandleContentOnMouseDownAsync(new MouseEventArgs
            {
                Buttons = 1,
                ClientX = startTouchPoint.ClientX,
                ClientY = startTouchPoint.ClientY,
            });
        }
    }
    
    private void QueueRemeasureBackgroundTask(
        TextEditorRenderBatch localRefCurrentRenderBatch,
        string localMeasureCharacterWidthAndRowHeightElementId,
        int localMeasureCharacterWidthAndRowHeightComponent,
        CancellationToken cancellationToken)
    {
        if (localRefCurrentRenderBatch.ViewModel is null || localRefCurrentRenderBatch.Options is null)
            return;

        var backgroundTask = new BackgroundTask(
            async cancellationToken =>
            {
                // Get the most recent instantiation of the ViewModel with the given key.
                var viewModel = TextEditorService.ViewModel
                    .FindOrDefault(localRefCurrentRenderBatch.ViewModel.ViewModelKey);

                var options = GetOptions();

                if (viewModel is not null && options is not null)
                {
                    await viewModel.RemeasureAsync(
                        options,
                        localMeasureCharacterWidthAndRowHeightElementId,
                        localMeasureCharacterWidthAndRowHeightComponent,
                        CancellationToken.None);
                }
            },
            "TextEditor Remeasure",
            "Re-measure for a ViewModel",
            false,
            _ => Task.CompletedTask,
            Dispatcher,
            CancellationToken.None);

        TextEditorBackgroundTaskQueue.QueueBackgroundWorkItem(backgroundTask);
    }
    
    private void QueueCalculateVirtualizationResultBackgroundTask(
        TextEditorRenderBatch localRefCurrentRenderBatch)
    {
        if (localRefCurrentRenderBatch.ViewModel is null || localRefCurrentRenderBatch.Options is null)
            return;

        var backgroundTask = new BackgroundTask(
            async cancellationToken =>
            {
                // Get the most recent instantiation of the ViewModel with the given key.
                var viewModel = TextEditorService.ViewModel
                    .FindOrDefault(localRefCurrentRenderBatch.ViewModel.ViewModelKey);
                
                var model = TextEditorService.ViewModel
                    .FindBackingModelOrDefault(localRefCurrentRenderBatch.ViewModel.ViewModelKey);

                if (viewModel is not null && model is not null)
                {
                    await localRefCurrentRenderBatch.ViewModel.CalculateVirtualizationResultAsync(
                        model,
                        null,
                        CancellationToken.None);
                }
            },
            "TextEditor CalculateVirtualizationResult",
            "CalculateVirtualizationResult for a ViewModel",
            false,
            _ => Task.CompletedTask,
            Dispatcher,
            CancellationToken.None);

        TextEditorBackgroundTaskQueue.QueueBackgroundWorkItem(backgroundTask);
    }

    private void QueueUpdateSemanticPresentationModelBackgroundTask(
        TextEditorRenderBatch localRefCurrentRenderBatch)
    {
        if (localRefCurrentRenderBatch.ViewModel is null || localRefCurrentRenderBatch.Options is null)
            return;

        var backgroundTask = new BackgroundTask(
            cancellationToken =>
            {
                localRefCurrentRenderBatch.ViewModel.UpdateSemanticPresentationModel();

                return Task.CompletedTask;
            },
            "TextEditor UpdateSemanticPresentationModel",
            "UpdateSemanticPresentationModel for a ViewModel",
            false,
            _ => Task.CompletedTask,
            Dispatcher,
            CancellationToken.None);

        TextEditorBackgroundTaskQueue.QueueBackgroundWorkItem(backgroundTask);
    }

    public void Dispose()
    {
        ModelsCollectionWrap.StateChanged -= GeneralOnStateChangedEventHandler;
        ViewModelsCollectionWrap.StateChanged -= GeneralOnStateChangedEventHandler;
        GlobalOptionsWrap.StateChanged -= GeneralOnStateChangedEventHandler;

        lock (_toRenderViewModelDataLock)
        {
            if (_toRenderViewModelData is not null)
            {
                _toRenderViewModelData.DisplayTracker.DecrementLinks(ModelsCollectionWrap);
                _toRenderViewModelData = null;
            }
        }

        _mouseStoppedMovingCancellationTokenSource.Cancel();
    }
}