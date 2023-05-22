using Luthetus.TextEditor.RazorLib.Model;
using Luthetus.TextEditor.RazorLib.ViewModel;

namespace Luthetus.TextEditor.RazorLib.Store.ViewModel;

public partial class TextEditorViewModelsCollection
{
    public record RegisterAction(
        TextEditorViewModelKey TextEditorViewModelKey, 
        TextEditorModelKey TextEditorModelKey,
        ITextEditorService TextEditorService);

    public record DisposeAction(
        TextEditorViewModelKey TextEditorViewModelKey);
    
    public record SetViewModelWithAction(
        TextEditorViewModelKey TextEditorViewModelKey, 
        Func<TextEditorViewModel, TextEditorViewModel> WithFunc);
}