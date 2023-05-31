using Luthetus.TextEditor.RazorLib.Analysis.Html.InjectedLanguage;
using Luthetus.TextEditor.RazorLib.Analysis.Razor.SyntaxActors;

namespace Luthetus.TextEditor.RazorLib.Analysis.Razor.Facts;

public static class RazorInjectedLanguageFacts
{
    public static readonly InjectedLanguageDefinition RazorInjectedLanguageDefinition = new(
            RazorFacts.TRANSITION_SUBSTRING,
            RazorFacts.TRANSITION_SUBSTRING_ESCAPED,
            RazorSyntaxTree
                .ParseInjectedLanguageFragment);
}