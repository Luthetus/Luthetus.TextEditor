using Luthetus.TextEditor.RazorLib.Diff;
using Luthetus.TextEditor.RazorLib.Decoration;
using Luthetus.TextEditor.RazorLib.Lexing;
using System.Collections.Immutable;

namespace Luthetus.TextEditor.RazorLib.Semantics;

public class SemanticFacts
{
    public const string CssClassString = "bte_semantic-presentation";

    public static readonly TextEditorPresentationKey PresentationKey = TextEditorPresentationKey.NewTextEditorPresentationKey();
    public static readonly TextEditorPresentationModel EmptyPresentationModel = new(
        PresentationKey,
        0,
        CssClassString,
        new TextEditorSemanticDecorationMapper(),
        ImmutableList<TextEditorTextSpan>.Empty);
}
