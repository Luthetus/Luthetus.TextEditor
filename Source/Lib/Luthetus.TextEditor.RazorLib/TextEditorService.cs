using Luthetus.Common.RazorLib.BackgroundTaskCase;
using Luthetus.Common.RazorLib.Storage;
using Luthetus.Common.RazorLib.Store.ThemeCase;
using Luthetus.Common.RazorLib.Theme;
using Luthetus.TextEditor.RazorLib.Store.Diff;
using Luthetus.TextEditor.RazorLib.Store.Find;
using Luthetus.TextEditor.RazorLib.Store.Group;
using Luthetus.TextEditor.RazorLib.Store.Model;
using Luthetus.TextEditor.RazorLib.Store.Options;
using Luthetus.TextEditor.RazorLib.Store.ViewModel;
using Fluxor;
using Microsoft.JSInterop;
using static Luthetus.TextEditor.RazorLib.ITextEditorService;

namespace Luthetus.TextEditor.RazorLib;

public class TextEditorService : ITextEditorService
{
    private readonly IDispatcher _dispatcher;
    private readonly LuthetusTextEditorOptions _luthetusTextEditorOptions;
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;
    private readonly IStorageService _storageService;

    // TODO: Perhaps do not reference IJSRuntime but instead wrap it in a 'IUiProvider' or something like that. The 'IUiProvider' would then expose methods that allow the TextEditorViewModel to adjust the scrollbars. 
    private readonly IJSRuntime _jsRuntime;

    public TextEditorService(
        IState<TextEditorModelsCollection> modelsCollectionWrap,
        IState<TextEditorViewModelsCollection> viewModelsCollectionWrap,
        IState<TextEditorGroupsCollection> groupsCollectionWrap,
        IState<TextEditorDiffsCollection> diffsCollectionWrap,
        IState<ThemeRecordsCollection> themeRecordsCollectionWrap,
        IState<TextEditorOptionsState> textEditorOptionsWrap,
        IState<TextEditorFindProviderState> textEditorFindProvidersCollectionWrap,
        IDispatcher dispatcher,
        LuthetusTextEditorOptions luthetusTextEditorOptions,
        IBackgroundTaskQueue backgroundTaskQueue,
        IStorageService storageService,
        IJSRuntime jsRuntime)
    {
        ModelsCollectionWrap = modelsCollectionWrap;
        ViewModelsCollectionWrap = viewModelsCollectionWrap;
        GroupsCollectionWrap = groupsCollectionWrap;
        DiffsCollectionWrap = diffsCollectionWrap;
        ThemeRecordsCollectionWrap = themeRecordsCollectionWrap;
        OptionsWrap = textEditorOptionsWrap;
        FindProviderState = textEditorFindProvidersCollectionWrap;

        _dispatcher = dispatcher;
        _luthetusTextEditorOptions = luthetusTextEditorOptions;
        _backgroundTaskQueue = backgroundTaskQueue;
        _storageService = storageService;
        _jsRuntime = jsRuntime;

        Model = new ModelApi(
            _dispatcher,
            _luthetusTextEditorOptions,
            this);

        ViewModel = new ViewModelApi(
            _dispatcher,
            _luthetusTextEditorOptions,
            _jsRuntime,
            this);

        Group = new GroupApi(
            _dispatcher,
            _luthetusTextEditorOptions,
            this);

        Diff = new DiffApi(
            _dispatcher,
            _luthetusTextEditorOptions,
            this);

        Options = new OptionsApi(
            _dispatcher,
            _luthetusTextEditorOptions,
            _storageService,
            this);

        FindProvider = new FindProviderApi(
            _dispatcher,
            _luthetusTextEditorOptions,
            this);
    }

    public IState<TextEditorModelsCollection> ModelsCollectionWrap { get; }
    public IState<TextEditorViewModelsCollection> ViewModelsCollectionWrap { get; }
    public IState<TextEditorGroupsCollection> GroupsCollectionWrap { get; }
    public IState<TextEditorDiffsCollection> DiffsCollectionWrap { get; }
    public IState<ThemeRecordsCollection> ThemeRecordsCollectionWrap { get; }
    public IState<TextEditorOptionsState> OptionsWrap { get; }
    public IState<TextEditorFindProviderState> FindProviderState { get; }

    public string StorageKey => "luth_te_text-editor-options";

    public string ThemeCssClassString => ThemeRecordsCollectionWrap.Value.ThemeRecordsList
            .FirstOrDefault(x =>
                x.ThemeKey == OptionsWrap.Value.Options.CommonOptions
                    .ThemeKey)
            ?.CssClassString
        ?? ThemeFacts.VisualStudioDarkThemeClone.CssClassString;

    public IModelApi Model { get; }
    public IViewModelApi ViewModel { get; }
    public IGroupApi Group { get; }
    public IDiffApi Diff { get; }
    public IOptionsApi Options { get; }
    public IFindProviderApi FindProvider { get; }
}