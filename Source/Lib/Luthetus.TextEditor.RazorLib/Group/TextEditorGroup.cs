using System.Collections.Immutable;
using Luthetus.Common.RazorLib.Misc;
using Luthetus.TextEditor.RazorLib.ViewModel;

namespace Luthetus.TextEditor.RazorLib.Group;

/// <summary>Store the state of none or many tabs, and which tab is the active one. Each tab represents a <see cref="TextEditorViewModel"/>.</summary>
public record TextEditorGroup(
    TextEditorGroupKey GroupKey,
    TextEditorViewModelKey ActiveViewModelKey,
    ImmutableList<TextEditorViewModelKey> ViewModelKeys)
{
    public RenderStateKey RenderStateKey { get; init; } =
        RenderStateKey.NewRenderStateKey();
}