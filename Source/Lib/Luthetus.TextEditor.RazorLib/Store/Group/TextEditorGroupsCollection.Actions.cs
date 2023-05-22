using Luthetus.TextEditor.RazorLib.Group;
using Luthetus.TextEditor.RazorLib.ViewModel;

namespace Luthetus.TextEditor.RazorLib.Store.Group;

public partial class TextEditorGroupsCollection
{
    public record AddViewModelToGroupAction(
        TextEditorGroupKey TextEditorGroupKey,
        TextEditorViewModelKey TextEditorViewModelKey);

    public record RegisterAction(
        TextEditorGroup TextEditorGroup);
    
    public record DisposeAction(
        TextEditorGroupKey TextEditorGroupKey);

    public record RemoveViewModelFromGroupAction(
        TextEditorGroupKey TextEditorGroupKey,
        TextEditorViewModelKey TextEditorViewModelKey);

    public record SetActiveViewModelOfGroupAction(
        TextEditorGroupKey TextEditorGroupKey,
        TextEditorViewModelKey TextEditorViewModelKey);
}