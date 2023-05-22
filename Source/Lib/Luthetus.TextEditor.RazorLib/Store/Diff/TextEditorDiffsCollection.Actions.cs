using Luthetus.TextEditor.RazorLib.Diff;
using Luthetus.TextEditor.RazorLib.ViewModel;

namespace Luthetus.TextEditor.RazorLib.Store.Diff;

public partial class TextEditorDiffsCollection
{
    public record RegisterAction(
        TextEditorDiffKey DiffKey,
        TextEditorViewModelKey BeforeViewModelKey,
        TextEditorViewModelKey AfterViewModelKey);
    
    public record DisposeAction(
        TextEditorDiffKey DiffKey);
}