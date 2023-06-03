using Fluxor;
using Luthetus.Common.RazorLib.JavaScriptObjects;
using Luthetus.TextEditor.RazorLib.Model;
using Luthetus.TextEditor.RazorLib.Store.ViewModel;
using Luthetus.TextEditor.RazorLib.Measurement;
using Luthetus.TextEditor.RazorLib.ViewModel;
using Microsoft.JSInterop;
using System.Runtime.CompilerServices;

namespace Luthetus.TextEditor.RazorLib;

public partial interface ITextEditorService
{
    public interface IViewModelApi
    {
        public void Dispose(TextEditorViewModelKey textEditorViewModelKey);
        public Task<ElementMeasurementsInPixels> MeasureElementInPixelsAsync(string elementId);
        public Task<CharacterWidthAndRowHeight> MeasureCharacterWidthAndRowHeightAsync(string measureCharacterWidthAndRowHeightElementId, int countOfTestCharacters);
        public TextEditorViewModel? FindOrDefault(TextEditorViewModelKey textEditorViewModelKey);
        public Task FocusPrimaryCursorAsync(string primaryCursorContentId);
        public string? GetAllText(TextEditorViewModelKey textEditorViewModelKey);
        public TextEditorModel? FindBackingModelOrDefault(TextEditorViewModelKey textEditorViewModelKey);
        public Task MutateScrollHorizontalPositionAsync(string bodyElementId, string gutterElementId, double pixels);
        public Task MutateScrollVerticalPositionAsync(string bodyElementId, string gutterElementId, double pixels);
        public void Register(TextEditorViewModelKey textEditorViewModelKey, TextEditorModelKey textEditorModelKey);
        public Task SetGutterScrollTopAsync(string gutterElementId, double scrollTopInPixels);
        public Task SetScrollPositionAsync(string bodyElementId, string gutterElementId, double? scrollLeftInPixels, double? scrollTopInPixels);
        public void With(TextEditorViewModelKey textEditorViewModelKey, Func<TextEditorViewModel, TextEditorViewModel> withFunc);
        public void SetCursorShouldBlink(bool cursorShouldBlink);

        public bool CursorShouldBlink { get; }
        public event Action? CursorShouldBlinkChanged;
    }

    public class ViewModelApi : IViewModelApi
    {
        private readonly ITextEditorService _textEditorService;
        private readonly IDispatcher _dispatcher;
        private readonly LuthetusTextEditorOptions _luthetusTextEditorOptions;

        // TODO: Perhaps do not reference IJSRuntime but instead wrap it in a 'IUiProvider' or something like that. The 'IUiProvider' would then expose methods that allow the TextEditorViewModel to adjust the scrollbars. 
        private readonly IJSRuntime _jsRuntime;

        public ViewModelApi(
            IDispatcher dispatcher,
            LuthetusTextEditorOptions luthetusTextEditorOptions,
            IJSRuntime jsRuntime,
            ITextEditorService textEditorService)
        {
            _dispatcher = dispatcher;
            _luthetusTextEditorOptions = luthetusTextEditorOptions;
            _jsRuntime = jsRuntime;
            _textEditorService = textEditorService;
        }

        private Task _cursorShouldBlinkTask = Task.CompletedTask;

        private CancellationTokenSource _cursorShouldBlinkCancellationTokenSource = new();
        private TimeSpan _blinkingCursorTaskDelay = TimeSpan.FromMilliseconds(1000);

        public bool CursorShouldBlink { get; private set; } = true;
        public event Action? CursorShouldBlinkChanged;

        /// <summary>(2023-06-03) Previously this logic was in the TextEditorCursorDisplay itself. The Task.Run() would get re-executed upon each cancellation. With this version, the Task.Run() session is re-used with the while loop. As well, all the text editor cursors are blinking in sync.</summary>
        public void SetCursorShouldBlink(bool cursorShouldBlink)
        {
            if (!cursorShouldBlink)
            {
                if (CursorShouldBlink)
                {
                    // Change true -> false THEREFORE: notify subscribers
                    CursorShouldBlink = cursorShouldBlink;
                    CursorShouldBlinkChanged?.Invoke();
                }

                // Single Threaded Applications flicker every "_blinkingCursorTaskDelay" event while holding a key down if this line is not included
                _cursorShouldBlinkCancellationTokenSource.Cancel();

                if (_cursorShouldBlinkTask.IsCompleted)
                {
                    // Considering that just before entering this if block we cancel the cancellation token source. I want to ensure we get a new one if a new Task session beings.
                    _cursorShouldBlinkCancellationTokenSource = new();

                    _cursorShouldBlinkTask = Task.Run(async () =>
                    {
                        while (true)
                        {
                            try
                            {
                                var cancellationToken = _cursorShouldBlinkCancellationTokenSource.Token;

                                await Task
                                    .Delay(_blinkingCursorTaskDelay, cancellationToken)
                                    .ConfigureAwait(false);

                                // Change false -> true THEREFORE: notify subscribers
                                CursorShouldBlink = true;
                                CursorShouldBlinkChanged?.Invoke();
                                break;
                            }
                            catch (TaskCanceledException)
                            {
                                // Single Threaded Applications cannot exit the while loop unless they cancel the token themselves.
                                _cursorShouldBlinkCancellationTokenSource.Cancel();
                                _cursorShouldBlinkCancellationTokenSource = new();
                            }
                        }
                    });
                }
            }
        }

        public void With(
            TextEditorViewModelKey textEditorViewModelKey,
            Func<TextEditorViewModel, TextEditorViewModel> withFunc)
        {
            _dispatcher.Dispatch(
                new TextEditorViewModelsCollection.SetViewModelWithAction(
                    textEditorViewModelKey,
                    withFunc));
        }

        /// <summary>
        /// If a parameter is null the JavaScript will not modify that value
        /// </summary>
        public async Task SetScrollPositionAsync(
            string bodyElementId,
            string gutterElementId,
            double? scrollLeftInPixels,
            double? scrollTopInPixels)
        {
            await _jsRuntime.InvokeVoidAsync(
                "luthetusTextEditor.setScrollPosition",
                bodyElementId,
                gutterElementId,
                scrollLeftInPixels,
                scrollTopInPixels);
        }

        public async Task SetGutterScrollTopAsync(
            string gutterElementId,
            double scrollTopInPixels)
        {
            await _jsRuntime.InvokeVoidAsync(
                "luthetusTextEditor.setGutterScrollTop",
                gutterElementId,
                scrollTopInPixels);
        }

        public void Register(
            TextEditorViewModelKey textEditorViewModelKey,
            TextEditorModelKey textEditorModelKey)
        {
            _dispatcher.Dispatch(
                new TextEditorViewModelsCollection.RegisterAction(
                    textEditorViewModelKey,
                    textEditorModelKey,
                    _textEditorService));
        }

        public async Task MutateScrollVerticalPositionAsync(
            string bodyElementId,
            string gutterElementId,
            double pixels)
        {
            await _jsRuntime.InvokeVoidAsync(
                "luthetusTextEditor.mutateScrollVerticalPositionByPixels",
                bodyElementId,
                gutterElementId,
                pixels);
        }

        public async Task MutateScrollHorizontalPositionAsync(
            string bodyElementId,
            string gutterElementId,
            double pixels)
        {
            await _jsRuntime.InvokeVoidAsync(
                "luthetusTextEditor.mutateScrollHorizontalPositionByPixels",
                bodyElementId,
                gutterElementId,
                pixels);
        }

        public TextEditorModel? FindBackingModelOrDefault(
            TextEditorViewModelKey textEditorViewModelKey)
        {
            var textEditorViewModelsCollection = _textEditorService.ViewModelsCollectionWrap
                .Value;

            var viewModel = textEditorViewModelsCollection.ViewModelsList
                .FirstOrDefault(x =>
                    x.ViewModelKey == textEditorViewModelKey);

            if (viewModel is null)
                return null;

            return _textEditorService.Model.FindOrDefault(viewModel.ModelKey);
        }

        public string? GetAllText(
            TextEditorViewModelKey textEditorViewModelKey)
        {
            var textEditorModel = FindBackingModelOrDefault(textEditorViewModelKey);

            return textEditorModel is null
                ? null
                : _textEditorService.Model.GetAllText(textEditorModel.ModelKey);
        }

        public async Task FocusPrimaryCursorAsync(string primaryCursorContentId)
        {
            await _jsRuntime.InvokeVoidAsync(
                "luthetusTextEditor.focusHtmlElementById",
                primaryCursorContentId);
        }

        public TextEditorViewModel? FindOrDefault(
            TextEditorViewModelKey textEditorViewModelKey)
        {
            return _textEditorService.ViewModelsCollectionWrap.Value.ViewModelsList
                .FirstOrDefault(x =>
                    x.ViewModelKey == textEditorViewModelKey);
        }

        public async Task<ElementMeasurementsInPixels> MeasureElementInPixelsAsync(
            string elementId)
        {
            return await _jsRuntime.InvokeAsync<ElementMeasurementsInPixels>(
                "luthetusTextEditor.getElementMeasurementsInPixelsById",
                elementId);
        }

        public async Task<CharacterWidthAndRowHeight> MeasureCharacterWidthAndRowHeightAsync(
            string measureCharacterWidthAndRowHeightElementId,
            int countOfTestCharacters)
        {
            return await _jsRuntime.InvokeAsync<CharacterWidthAndRowHeight>(
                    "luthetusTextEditor.measureCharacterWidthAndRowHeight",
                    measureCharacterWidthAndRowHeightElementId,
                    countOfTestCharacters);
        }

        public void Dispose(TextEditorViewModelKey textEditorViewModelKey)
        {
            _dispatcher.Dispatch(
                new TextEditorViewModelsCollection.DisposeAction(
                    textEditorViewModelKey));
        }
    }
}
