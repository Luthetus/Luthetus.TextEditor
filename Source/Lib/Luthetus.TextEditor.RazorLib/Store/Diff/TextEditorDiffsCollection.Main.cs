using System.Collections.Immutable;
using Fluxor;
using Luthetus.TextEditor.RazorLib.Diff;

namespace Luthetus.TextEditor.RazorLib.Store.Diff;

/// <summary>
/// Keep the <see cref="TextEditorDiffsCollection"/> as a class
/// as to avoid record value comparisons when Fluxor checks
/// if the <see cref="FeatureStateAttribute"/> has been replaced.
/// </summary>
[FeatureState]
public partial class TextEditorDiffsCollection
{
    public TextEditorDiffsCollection()
    {
        DiffModelsList = ImmutableList<TextEditorDiffModel>.Empty; 
    }

    public ImmutableList<TextEditorDiffModel> DiffModelsList { get; init; }
}