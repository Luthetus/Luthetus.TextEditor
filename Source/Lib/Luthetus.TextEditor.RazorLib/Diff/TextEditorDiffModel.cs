using Luthetus.Common.RazorLib.Misc;
using Luthetus.TextEditor.RazorLib.ViewModel;

namespace Luthetus.TextEditor.RazorLib.Diff;

public record TextEditorDiffModel(
    TextEditorDiffKey DiffKey,
    TextEditorViewModelKey BeforeViewModelKey,
    TextEditorViewModelKey AfterViewModelKey)
{
    public RenderStateKey RenderStateKey { get; init; } = RenderStateKey.NewRenderStateKey();
}