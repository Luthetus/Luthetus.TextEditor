using Luthetus.TextEditor.RazorLib.Model;
using Luthetus.TextEditor.RazorLib.Store.Model;
using Luthetus.TextEditor.RazorLib.Store.Options;
using Luthetus.TextEditor.RazorLib.Store.ViewModel;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using Luthetus.TextEditor.RazorLib.ViewModel;
using Microsoft.AspNetCore.Components;

namespace Luthetus.TextEditor.RazorLib;

/// <summary>
/// <see cref="TextEditorView"/> is the
/// message broker abstraction between a
/// Blazor component and a <see cref="TextEditorModel"/>
/// </summary>
public class TextEditorView : FluxorComponent
{
    // TODO: Do not rerender so much too many things are touched by the [Inject] in this file
    //
    // [Inject]
    // protected IStateSelection<TextEditorModelsCollection, TextEditorModel?> TextEditorModelsCollectionSelection { get; set; } = null!;
    [Inject]
    protected IState<TextEditorModelsCollection> TextEditorModelsCollectionWrap { get; set; } = null!;
    [Inject]
    protected IState<TextEditorViewModelsCollection> TextEditorViewModelsCollectionWrap { get; set; } = null!;
    [Inject]
    protected IState<TextEditorOptionsState> TextEditorGlobalOptionsWrap { get; set; } = null!;
    [Inject]
    protected ITextEditorService TextEditorService { get; set; } = null!;

    [Parameter, EditorRequired]
    public TextEditorViewModelKey TextEditorViewModelKey { get; set; } = null!;

    public TextEditorModel? MutableReferenceToModel => TextEditorService.ViewModel
        .FindBackingModelOrDefault(TextEditorViewModelKey);

    public TextEditorViewModel? MutableReferenceToViewModel => TextEditorViewModelsCollectionWrap.Value.ViewModelsList
        .FirstOrDefault(x =>
            x.ViewModelKey == TextEditorViewModelKey);
}