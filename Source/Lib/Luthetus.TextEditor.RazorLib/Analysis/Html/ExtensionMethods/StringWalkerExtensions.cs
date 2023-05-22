﻿using Luthetus.TextEditor.RazorLib.Analysis;
using Luthetus.TextEditor.RazorLib.Analysis.Html.InjectedLanguage;

namespace Luthetus.TextEditor.RazorLib.Analysis.Html.ExtensionMethods;

public static class StringWalkerExtensions
{
    public static bool CheckForInjectedLanguageCodeBlockTag(
        this StringWalker stringWalker,
        InjectedLanguageDefinition injectedLanguageDefinition)
    {
        var isMatch = stringWalker.CheckForSubstring(injectedLanguageDefinition.TransitionSubstring);
        var isEscaped = stringWalker.CheckForSubstring(injectedLanguageDefinition.TransitionSubstringEscaped);

        return isMatch && !isEscaped;
    }
}