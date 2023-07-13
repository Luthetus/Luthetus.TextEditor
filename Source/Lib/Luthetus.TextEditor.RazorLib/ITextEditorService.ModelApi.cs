using Fluxor;
using Luthetus.TextEditor.RazorLib.Model;
using Luthetus.TextEditor.RazorLib.Store.Model;
using Luthetus.TextEditor.RazorLib.Decoration;
using Luthetus.TextEditor.RazorLib.Lexing;
using Luthetus.TextEditor.RazorLib.Row;
using Luthetus.TextEditor.RazorLib.ViewModel;
using System.Collections.Immutable;

namespace Luthetus.TextEditor.RazorLib;

public partial interface ITextEditorService
{
    public interface IModelApi
    {
        public void DeleteTextByMotion(TextEditorModelsCollection.DeleteTextByMotionAction deleteTextByMotionAction);
        public void DeleteTextByRange(TextEditorModelsCollection.DeleteTextByRangeAction deleteTextByRangeAction);
        public void Dispose(TextEditorModelKey textEditorModelKey);
        public TextEditorModel? FindOrDefault(TextEditorModelKey textEditorModelKey);
        public string? GetAllText(TextEditorModelKey textEditorModelKey);
        public ImmutableArray<TextEditorViewModel> GetViewModelsOrEmpty(TextEditorModelKey textEditorModelKey);
        public void HandleKeyboardEvent(TextEditorModelsCollection.KeyboardEventAction keyboardEventAction);
        public void InsertText(TextEditorModelsCollection.InsertTextAction insertTextAction);
        public void RedoEdit(TextEditorModelKey textEditorModelKey);
        /// <summary>It is recommended to use the <see cref="RegisterTemplated" /> method as it will internally reference the <see cref="ITextEditorLexer" /> and <see cref="IDecorationMapper" /> that correspond to the desired text editor.</summary>
        public void RegisterCustom(TextEditorModel model);
        /// <summary>As an example, for a C# Text Editor one would pass in a <see cref="WellKnownModelKind" /> of <see cref="WellKnownModelKind.CSharp" />.<br /><br />For a Plain Text Editor one would pass in a <see cref="WellKnownModelKind" /> of <see cref="WellKnownModelKind.Plain" />.</summary>
        public void RegisterTemplated(TextEditorModelKey textEditorModelKey, WellKnownModelKind wellKnownModelKind, ResourceUri resourceUri, DateTime resourceLastWriteTime, string fileExtension, string initialContent);
        public void Reload(TextEditorModelKey textEditorModelKey, string content, DateTime resourceLastWriteTime);
        public void SetResourceData(TextEditorModelKey textEditorModelKey, ResourceUri resourceUri, DateTime resourceLastWriteTime);
        public void SetUsingRowEndingKind(TextEditorModelKey textEditorModelKey, RowEndingKind rowEndingKind);
        public void UndoEdit(TextEditorModelKey textEditorModelKey);
        public TextEditorModel? FindOrDefaultByResourceUri(ResourceUri resourceUri);
    }

    public class ModelApi : IModelApi
    {
        private readonly ITextEditorService _textEditorService;
        private readonly IDispatcher _dispatcher;
        private readonly LuthetusTextEditorOptions _luthetusTextEditorOptions;

        public ModelApi(
            IDispatcher dispatcher,
            LuthetusTextEditorOptions luthetusTextEditorOptions,
            ITextEditorService textEditorService)
        {
            _dispatcher = dispatcher;
            _luthetusTextEditorOptions = luthetusTextEditorOptions;
            _textEditorService = textEditorService;
        }

        public TextEditorModel? FindOrDefaultByResourceUri(
            ResourceUri resourceUri)
        {
            return _textEditorService.ModelsCollectionWrap.Value.TextEditorList
                .FirstOrDefault(x =>
                    x.ResourceUri == resourceUri);
        }

        public void UndoEdit(
            TextEditorModelKey textEditorModelKey)
        {
            var undoEditAction = new TextEditorModelsCollection.UndoEditAction(
                textEditorModelKey);

            _dispatcher.Dispatch(undoEditAction);
        }

        public void SetUsingRowEndingKind(
            TextEditorModelKey textEditorModelKey,
            RowEndingKind rowEndingKind)
        {
            _dispatcher.Dispatch(
                new TextEditorModelsCollection.SetUsingRowEndingKindAction(
                    textEditorModelKey,
                    rowEndingKind));
        }

        public void SetResourceData(
            TextEditorModelKey textEditorModelKey,
            ResourceUri resourceUri,
            DateTime resourceLastWriteTime)
        {
            _dispatcher.Dispatch(
                new TextEditorModelsCollection.SetResourceDataAction(
                    textEditorModelKey,
                    resourceUri,
                    resourceLastWriteTime));
        }

        public void Reload(
            TextEditorModelKey textEditorModelKey,
            string content,
            DateTime resourceLastWriteTime)
        {
            _dispatcher.Dispatch(
                new TextEditorModelsCollection.ReloadAction(
                    textEditorModelKey,
                    content,
                    resourceLastWriteTime));
        }

        public void RegisterTemplated(
            TextEditorModelKey textEditorModelKey,
            WellKnownModelKind wellKnownModelKind,
            ResourceUri resourceUri,
            DateTime resourceLastWriteTime,
            string fileExtension,
            string initialContent)
        {
            IDecorationMapper? decorationMapper = null;

            // TODO: (2023-06-29) I'm rewritting the TextEditor 'ISemanticModel.cs' to be 'ICompilerService.cs'. This method broke in the process and is not high priority to fix yet.

            var textEditorModel = new TextEditorModel(
                resourceUri,
                resourceLastWriteTime,
                fileExtension,
                initialContent,
                null,
                decorationMapper,
                null,
                new(),
                textEditorModelKey);

            // ICommonBackgroundTaskQueue does not work well here because
            // this Task does not need to be tracked.
            _ = Task.Run(async () =>
            {
                try
                {
                    await textEditorModel.ApplySyntaxHighlightingAsync();

                    _dispatcher.Dispatch(
                        new TextEditorModelsCollection.ForceRerenderAction(
                            textEditorModel.ModelKey));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }, CancellationToken.None);

            _dispatcher.Dispatch(
                new TextEditorModelsCollection.RegisterAction(
                    textEditorModel));
        }

        public void RegisterCustom(
            TextEditorModel model)
        {
            _dispatcher.Dispatch(
                new TextEditorModelsCollection.RegisterAction(
                    model));
        }

        public void RedoEdit(
            TextEditorModelKey textEditorModelKey)
        {
            var redoEditAction = new TextEditorModelsCollection.RedoEditAction(
                textEditorModelKey);

            _dispatcher.Dispatch(redoEditAction);
        }

        public void InsertText(
            TextEditorModelsCollection.InsertTextAction insertTextAction)
        {
            _dispatcher.Dispatch(insertTextAction);
        }

        public void HandleKeyboardEvent(
            TextEditorModelsCollection.KeyboardEventAction keyboardEventAction)
        {
            _dispatcher.Dispatch(keyboardEventAction);
        }

        public ImmutableArray<TextEditorViewModel> GetViewModelsOrEmpty(
            TextEditorModelKey textEditorModelKey)
        {
            return _textEditorService.ViewModelsCollectionWrap.Value.ViewModelsList
                .Where(x => x.ModelKey == textEditorModelKey)
                .ToImmutableArray();
        }

        public string? GetAllText(
            TextEditorModelKey textEditorModelKey)
        {
            return _textEditorService.ModelsCollectionWrap.Value.TextEditorList
                .FirstOrDefault(x => x.ModelKey == textEditorModelKey)
                ?.GetAllText();
        }

        public TextEditorModel? FindOrDefault(
            TextEditorModelKey textEditorModelKey)
        {
            return _textEditorService.ModelsCollectionWrap.Value.TextEditorList
                .FirstOrDefault(x => x.ModelKey == textEditorModelKey);
        }

        public void Dispose(
            TextEditorModelKey textEditorModelKey)
        {
            _dispatcher.Dispatch(
                new TextEditorModelsCollection.DisposeAction(
                    textEditorModelKey));
        }

        public void DeleteTextByRange(
            TextEditorModelsCollection.DeleteTextByRangeAction deleteTextByRangeAction)
        {
            _dispatcher.Dispatch(deleteTextByRangeAction);
        }

        public void DeleteTextByMotion(
            TextEditorModelsCollection.DeleteTextByMotionAction deleteTextByMotionAction)
        {
            _dispatcher.Dispatch(deleteTextByMotionAction);
        }
    }
}
