using Luthetus.Common.RazorLib.Storage;
using Luthetus.Common.RazorLib.Store.ThemeCase;
using Luthetus.TextEditor.RazorLib.Store.Diff;
using Luthetus.TextEditor.RazorLib.Store.Find;
using Luthetus.TextEditor.RazorLib.Store.Group;
using Luthetus.TextEditor.RazorLib.Store.Model;
using Luthetus.TextEditor.RazorLib.Store.Options;
using Luthetus.TextEditor.RazorLib.Store.ViewModel;
using Fluxor;

namespace Luthetus.TextEditor.RazorLib;

public partial interface ITextEditorService
{
    /// <summary>This is used when interacting with the <see cref="IStorageService"/> to set and get data.</summary>
    public string StorageKey { get; }
    public string ThemeCssClassString { get; }

    public IModelApi Model { get; }
    public IViewModelApi ViewModel { get; }
    public IGroupApi Group { get; }
    public IDiffApi Diff { get; }
    public IOptionsApi Options { get; }
    public IFindProviderApi FindProvider { get; }

    public IState<TextEditorModelsCollection> ModelsCollectionWrap { get; }
    public IState<TextEditorViewModelsCollection> ViewModelsCollectionWrap { get; }
    public IState<TextEditorGroupsCollection> GroupsCollectionWrap { get; }
    public IState<TextEditorDiffsCollection> DiffsCollectionWrap { get; }
    public IState<ThemeRecordsCollection> ThemeRecordsCollectionWrap { get; }
    public IState<TextEditorOptionsState> OptionsWrap { get; }
    public IState<TextEditorFindProviderState> FindProviderState { get; }
}