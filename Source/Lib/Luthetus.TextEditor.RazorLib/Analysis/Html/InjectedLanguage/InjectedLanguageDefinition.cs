using Luthetus.TextEditor.RazorLib.Analysis.Html.SyntaxObjects;

namespace Luthetus.TextEditor.RazorLib.Analysis.Html.InjectedLanguage;

public class InjectedLanguageDefinition
{
    public InjectedLanguageDefinition(
        string transitionSubstring,
        string transitionSubstringEscaped,
        Func<StringWalker, TextEditorHtmlDiagnosticBag, InjectedLanguageDefinition, List<IHtmlSyntaxNode>> parseInjectedLanguageFunc,
        Func<StringWalker, TextEditorHtmlDiagnosticBag, InjectedLanguageDefinition, AttributeNameNode>? parseAttributeName,
        Func<StringWalker, TextEditorHtmlDiagnosticBag, InjectedLanguageDefinition, AttributeValueNode>? parseAttributeValue)
    {
        TransitionSubstring = transitionSubstring;
        TransitionSubstringEscaped = transitionSubstringEscaped;
        ParseInjectedLanguageFunc = parseInjectedLanguageFunc;
        ParseAttributeName = parseAttributeName;
        ParseAttributeValue = parseAttributeValue;
    }

    /// <summary>Upon finding this substring when peeking by <see cref="TransitionSubstring"/>.Length the injected language Lexer will be invoked.</summary>
    public string TransitionSubstring { get; set; }
    /// <summary> If <see cref="TransitionSubstring"/> is found then a peek is done to ensure the upcoming text is not equal to <see cref="TransitionSubstringEscaped"/>. <br/><br/> Should both <see cref="TransitionSubstring"/> and <see cref="TransitionSubstringEscaped"/> be found, then the injected language Lexer will NOT be invoked.</summary>
    public string TransitionSubstringEscaped { get; set; }

    public Func<StringWalker, TextEditorHtmlDiagnosticBag, InjectedLanguageDefinition, List<IHtmlSyntaxNode>>
        ParseInjectedLanguageFunc
    { get; }

    public Func<StringWalker, TextEditorHtmlDiagnosticBag, InjectedLanguageDefinition, AttributeNameNode>?
        ParseAttributeName
    { get; }
    
    public Func<StringWalker, TextEditorHtmlDiagnosticBag, InjectedLanguageDefinition, AttributeValueNode>?
        ParseAttributeValue
    { get; }
}
