using Fluxor;
using Luthetus.TextEditor.RazorLib.Store.Group;
using Luthetus.TextEditor.RazorLib.Group;
using Luthetus.TextEditor.RazorLib.ViewModel;
using System.Collections.Immutable;

namespace Luthetus.TextEditor.RazorLib;

public partial interface ITextEditorService
{
    public interface IGroupApi
    {
        public void AddViewModel(TextEditorGroupKey textEditorGroupKey, TextEditorViewModelKey textEditorViewModelKey);
        public TextEditorGroup? FindOrDefault(TextEditorGroupKey textEditorGroupKey);
        public void Register(TextEditorGroupKey textEditorGroupKey);
        public void Dispose(TextEditorGroupKey textEditorGroupKey);
        public void RemoveViewModel(TextEditorGroupKey textEditorGroupKey, TextEditorViewModelKey textEditorViewModelKey);
        public void SetActiveViewModel(TextEditorGroupKey textEditorGroupKey, TextEditorViewModelKey textEditorViewModelKey);
    }

    public class GroupApi : IGroupApi
    {
        private readonly ITextEditorService _textEditorService;
        private readonly IDispatcher _dispatcher;
        private readonly LuthetusTextEditorOptions _luthetusTextEditorOptions;

        public GroupApi(
            IDispatcher dispatcher,
            LuthetusTextEditorOptions luthetusTextEditorOptions,
            ITextEditorService textEditorService)
        {
            _dispatcher = dispatcher;
            _luthetusTextEditorOptions = luthetusTextEditorOptions;
            _textEditorService = textEditorService;
        }

        public void SetActiveViewModel(
            TextEditorGroupKey textEditorGroupKey,
            TextEditorViewModelKey textEditorViewModelKey)
        {
            _dispatcher.Dispatch(
                new TextEditorGroupsCollection.SetActiveViewModelOfGroupAction(
                    textEditorGroupKey,
                    textEditorViewModelKey));
        }

        public void RemoveViewModel(
            TextEditorGroupKey textEditorGroupKey,
            TextEditorViewModelKey textEditorViewModelKey)
        {
            _dispatcher.Dispatch(
                new TextEditorGroupsCollection.RemoveViewModelFromGroupAction(
                    textEditorGroupKey,
                    textEditorViewModelKey));
        }

        public void Register(
            TextEditorGroupKey textEditorGroupKey)
        {
            var textEditorGroup = new TextEditorGroup(
                textEditorGroupKey,
                TextEditorViewModelKey.Empty,
                ImmutableList<TextEditorViewModelKey>.Empty);

            _dispatcher.Dispatch(
                new TextEditorGroupsCollection.RegisterAction(
                    textEditorGroup));
        }
        
        public void Dispose(
            TextEditorGroupKey textEditorGroupKey)
        {
            _dispatcher.Dispatch(
                new TextEditorGroupsCollection.DisposeAction(
                    textEditorGroupKey));
        }

        public TextEditorGroup? FindOrDefault(
            TextEditorGroupKey textEditorGroupKey)
        {
            return _textEditorService.GroupsCollectionWrap.Value.GroupsList
                .FirstOrDefault(x =>
                    x.GroupKey == textEditorGroupKey);
        }

        public void AddViewModel(
            TextEditorGroupKey textEditorGroupKey,
            TextEditorViewModelKey textEditorViewModelKey)
        {
            _dispatcher.Dispatch(
                new TextEditorGroupsCollection.AddViewModelToGroupAction(
                    textEditorGroupKey,
                    textEditorViewModelKey));
        }
    }
}
