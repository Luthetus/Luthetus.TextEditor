using Luthetus.Common.RazorLib.Misc;
using Luthetus.TextEditor.RazorLib.ViewModel;
using Fluxor;
using Luthetus.TextEditor.RazorLib.Character;
using Luthetus.TextEditor.RazorLib.Virtualization;

namespace Luthetus.TextEditor.RazorLib.Store.ViewModel;

public partial class TextEditorViewModelsCollection
{
    private class Reducer
    {
        [ReducerMethod]
        public static TextEditorViewModelsCollection ReduceRegisterAction(
            TextEditorViewModelsCollection inViewModelsCollection,
            RegisterAction registerAction)
        {
            var textEditorViewModel = inViewModelsCollection.ViewModelsList.FirstOrDefault(x =>
                x.ViewModelKey == registerAction.TextEditorViewModelKey);

            if (textEditorViewModel is not null)
                return inViewModelsCollection;

            var viewModel = new TextEditorViewModel(
                registerAction.TextEditorViewModelKey,
                registerAction.TextEditorModelKey,
                registerAction.TextEditorService,
                VirtualizationResult<List<RichCharacter>>.GetEmptyRichCharacters(),
                false)
            {
            };

            var nextViewModelsList = inViewModelsCollection.ViewModelsList
                .Add(viewModel);

            return new TextEditorViewModelsCollection
            {
                ViewModelsList = nextViewModelsList
            };
        }
        
        [ReducerMethod]
        public static TextEditorViewModelsCollection ReduceDisposeAction(
            TextEditorViewModelsCollection inViewModelsCollection,
            DisposeAction disposeAction)
        {
            var foundViewModel = inViewModelsCollection.ViewModelsList.FirstOrDefault(x =>
                x.ViewModelKey == disposeAction.TextEditorViewModelKey);

            if (foundViewModel is null)
                return inViewModelsCollection;

            var nextViewModelsList = inViewModelsCollection.ViewModelsList
                .Remove(foundViewModel);

            return new TextEditorViewModelsCollection
            {
                ViewModelsList = nextViewModelsList
            };
        }

        [ReducerMethod]
        public static TextEditorViewModelsCollection ReduceSetViewModelWithAction(
            TextEditorViewModelsCollection inViewModelsCollection,
            SetViewModelWithAction setViewModelWithAction)
        {
            var textEditorViewModel = inViewModelsCollection.ViewModelsList.FirstOrDefault(x =>
                x.ViewModelKey == setViewModelWithAction.TextEditorViewModelKey);

            if (textEditorViewModel is null)
                return inViewModelsCollection;

            var nextViewModel = setViewModelWithAction.WithFunc
                .Invoke(textEditorViewModel);

            var nextViewModelsList = inViewModelsCollection.ViewModelsList
                .Replace(textEditorViewModel, nextViewModel);

            return new TextEditorViewModelsCollection
            {
                ViewModelsList = nextViewModelsList
            };
        }
    }
}