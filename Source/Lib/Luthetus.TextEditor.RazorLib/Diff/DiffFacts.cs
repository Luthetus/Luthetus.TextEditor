using System.Collections.Immutable;
using Luthetus.TextEditor.RazorLib.Decoration;
using Luthetus.TextEditor.RazorLib.Lexing;

namespace Luthetus.TextEditor.RazorLib.Diff;

public static class DiffFacts
{
    public const string CssClassString = "luth_te_diff-presentation";

    public static readonly TextEditorPresentationKey PresentationKey = TextEditorPresentationKey.NewTextEditorPresentationKey();
    public static readonly TextEditorPresentationModel EmptyPresentationModel = new(
        PresentationKey,
        0,
        CssClassString,
        new TextEditorDiffDecorationMapper(),
        ImmutableList<TextEditorTextSpan>.Empty);
}