using Fluxor;
using Luthetus.Common.RazorLib.Misc;
using Luthetus.TextEditor.RazorLib.Store.Diff;
using Luthetus.TextEditor.RazorLib.Store.ViewModel;
using Luthetus.TextEditor.RazorLib.Diff;
using Luthetus.TextEditor.RazorLib.Lexing;
using Luthetus.TextEditor.RazorLib.ViewModel;
using System.Collections.Immutable;

namespace Luthetus.TextEditor.RazorLib;

public partial interface ITextEditorService
{
    public interface IDiffApi
    {
        public TextEditorDiffResult? Calculate(TextEditorDiffKey diffKey, CancellationToken cancellationToken);
        public void Dispose(TextEditorDiffKey diffKey);
        public TextEditorDiffModel? FindOrDefault(TextEditorDiffKey diffKey);
        public void Register(TextEditorDiffKey diffKey, TextEditorViewModelKey beforeViewModelKey, TextEditorViewModelKey afterViewModelKey);
    }

    public class DiffApi : IDiffApi
    {
        private readonly ITextEditorService _textEditorService;
        private readonly IDispatcher _dispatcher;
        private readonly LuthetusTextEditorOptions _luthetusTextEditorOptions;

        public DiffApi(
            IDispatcher dispatcher,
            LuthetusTextEditorOptions luthetusTextEditorOptions,
            ITextEditorService textEditorService)
        {
            _dispatcher = dispatcher;
            _luthetusTextEditorOptions = luthetusTextEditorOptions;
            _textEditorService = textEditorService;
        }

        public void Register(
            TextEditorDiffKey diffKey,
            TextEditorViewModelKey beforeViewModelKey,
            TextEditorViewModelKey afterViewModelKey)
        {
            _dispatcher.Dispatch(
                new TextEditorDiffsCollection.RegisterAction(
                    diffKey,
                    beforeViewModelKey,
                    afterViewModelKey));
        }

        public TextEditorDiffModel? FindOrDefault(
            TextEditorDiffKey diffKey)
        {
            return _textEditorService.DiffsCollectionWrap.Value.DiffModelsList
                .FirstOrDefault(x =>
                    x.DiffKey == diffKey);
        }

        public void Dispose(
            TextEditorDiffKey diffKey)
        {
            _dispatcher.Dispatch(
                new TextEditorDiffsCollection.DisposeAction(
                    diffKey));
        }

        public TextEditorDiffResult? Calculate(
            TextEditorDiffKey textEditorDiffKey,
            CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return null;

            var textEditorDiff = FindOrDefault(textEditorDiffKey);

            if (textEditorDiff is null)
                return null;

            var beforeViewModel = _textEditorService.ViewModel
                .FindOrDefault(textEditorDiff.BeforeViewModelKey);

            var afterViewModel = _textEditorService.ViewModel
                .FindOrDefault(textEditorDiff.AfterViewModelKey);

            if (beforeViewModel is null ||
                afterViewModel is null)
            {
                return null;
            }

            var beforeModel = _textEditorService.Model
                .FindOrDefault(beforeViewModel.ModelKey);

            var afterModel = _textEditorService.Model
                .FindOrDefault(afterViewModel.ModelKey);

            if (beforeModel is null ||
                afterModel is null)
            {
                return null;
            }

            var beforeText = beforeModel.GetAllText();
            var afterText = afterModel.GetAllText();

            var diffResult = TextEditorDiffResult.Calculate(
                beforeModel.ResourceUri,
                beforeText,
                afterModel.ResourceUri,
                afterText);

            ChangeFirstPresentationLayer(
                beforeViewModel.ViewModelKey,
                diffResult.BeforeLongestCommonSubsequenceTextSpans);

            ChangeFirstPresentationLayer(
                afterViewModel.ViewModelKey,
                diffResult.AfterLongestCommonSubsequenceTextSpans);

            return diffResult;
        }

        private void ChangeFirstPresentationLayer(
            TextEditorViewModelKey viewModelKey,
            ImmutableList<TextEditorTextSpan> longestCommonSubsequenceTextSpans)
        {
            _dispatcher.Dispatch(
                new TextEditorViewModelsCollection.SetViewModelWithAction(
                    viewModelKey,
                    inViewModel =>
                    {
                        var outPresentationLayer = inViewModel.FirstPresentationLayer;

                        var inPresentationModel = outPresentationLayer
                            .FirstOrDefault(x =>
                                x.TextEditorPresentationKey == DiffFacts.PresentationKey);

                        if (inPresentationModel is null)
                        {
                            inPresentationModel = DiffFacts.EmptyPresentationModel;

                            outPresentationLayer = outPresentationLayer.Add(
                                inPresentationModel);
                        }

                        var outPresentationModel = inPresentationModel with
                        {
                            TextEditorTextSpans = longestCommonSubsequenceTextSpans
                        };

                        outPresentationLayer = outPresentationLayer.Replace(
                            inPresentationModel,
                            outPresentationModel);

                        return inViewModel with
                        {
                            FirstPresentationLayer = outPresentationLayer,
                            RenderStateKey = RenderStateKey.NewRenderStateKey()
                        };
                    }));
        }
    }
}
