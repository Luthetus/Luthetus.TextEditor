using System.Collections.Immutable;
using Fluxor;
using Luthetus.TextEditor.RazorLib.Group;

namespace Luthetus.TextEditor.RazorLib.Store.Group;

/// <summary>
/// Keep the <see cref="TextEditorGroupsCollection"/> as a class
/// as to avoid record value comparisons when Fluxor checks
/// if the <see cref="FeatureStateAttribute"/> has been replaced.
/// </summary>
[FeatureState]
public partial class TextEditorGroupsCollection
{
    public TextEditorGroupsCollection()
    {
        GroupsList = ImmutableList<TextEditorGroup>.Empty; 
    }

    public ImmutableList<TextEditorGroup> GroupsList { get; init; }
}