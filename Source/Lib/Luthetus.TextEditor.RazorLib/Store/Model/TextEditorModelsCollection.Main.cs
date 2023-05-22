using System.Collections.Immutable;
using Luthetus.TextEditor.RazorLib.Model;
using Fluxor;

namespace Luthetus.TextEditor.RazorLib.Store.Model;

/// <summary>
/// Keep the <see cref="TextEditorModelsCollection"/> as a class
/// as to avoid record value comparisons when Fluxor checks
/// if the <see cref="FeatureStateAttribute"/> has been replaced.
/// </summary>
[FeatureState]
public partial class TextEditorModelsCollection
{
    public TextEditorModelsCollection()
    {
        TextEditorList = ImmutableList<TextEditorModel>.Empty;
    }

    public ImmutableList<TextEditorModel> TextEditorList { get; init; }
}