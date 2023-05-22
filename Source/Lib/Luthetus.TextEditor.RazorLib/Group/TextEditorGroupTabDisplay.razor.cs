using Luthetus.TextEditor.RazorLib;
using Luthetus.TextEditor.RazorLib.Store.Model;
using Luthetus.TextEditor.RazorLib.Store.ViewModel;
using Fluxor;
using Luthetus.TextEditor.RazorLib.ViewModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Luthetus.TextEditor.RazorLib.Group;

public partial class TextEditorGroupTabDisplay : ComponentBase
{
    [Inject]
    private ITextEditorService TextEditorService { get; set; } = null!;
    [Inject]
    private IState<TextEditorViewModelsCollection> TextEditorViewModelsCollectionWrap { get; set; } = null!;
    [Inject]
    private IState<TextEditorModelsCollection> TextEditorModelsCollectionWrap { get; set; } = null!;

    [Parameter, EditorRequired]
    public TextEditorViewModelKey TextEditorViewModelKey { get; set; } = null!;
    [Parameter, EditorRequired]
    public TextEditorGroup TextEditorGroup { get; set; } = null!;

    private string IsActiveCssClass => TextEditorGroup.ActiveViewModelKey == TextEditorViewModelKey
        ? "bcrl_active"
        : string.Empty;

    private void OnClickSetActiveTextEditorViewModel()
    {
        TextEditorService.Group.SetActiveViewModel(
            TextEditorGroup.GroupKey,
            TextEditorViewModelKey);
    }

    private void OnMouseDown(MouseEventArgs mouseEventArgs)
    {
        if (mouseEventArgs.Button == 1)
            CloseTabOnClick();
    }

    private void CloseTabOnClick()
    {
        TextEditorService.Group.RemoveViewModel(
            TextEditorGroup.GroupKey,
            TextEditorViewModelKey);
    }
}