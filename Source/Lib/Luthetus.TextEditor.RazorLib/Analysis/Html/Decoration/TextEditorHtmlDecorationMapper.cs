﻿using Luthetus.TextEditor.RazorLib.Decoration;

namespace Luthetus.TextEditor.RazorLib.Analysis.Html.Decoration;

public class TextEditorHtmlDecorationMapper : IDecorationMapper
{
    public string Map(byte decorationByte)
    {
        var decoration = (HtmlDecorationKind)decorationByte;

        return decoration switch
        {
            HtmlDecorationKind.None => string.Empty,
            HtmlDecorationKind.AttributeName => "bte_attribute-name",
            HtmlDecorationKind.AttributeValue => "bte_attribute-value",
            HtmlDecorationKind.Comment => "bte_comment",
            HtmlDecorationKind.CustomTagName => "bte_custom-tag-name",
            HtmlDecorationKind.EntityReference => "bte_entity-reference",
            HtmlDecorationKind.HtmlCode => "bte_html-code",
            HtmlDecorationKind.InjectedLanguageFragment => "bte_injected-language-fragment",
            HtmlDecorationKind.TagName => "bte_tag-name",
            HtmlDecorationKind.Tag => "bte_tag",
            HtmlDecorationKind.Error => "bte_error",
            HtmlDecorationKind.InjectedLanguageCodeBlock => "bte_injected-language-code-block",
            HtmlDecorationKind.InjectedLanguageCodeBlockTag => "bte_injected-language-code-block-tag",
            HtmlDecorationKind.InjectedLanguageKeyword => "bte_keyword",
            HtmlDecorationKind.InjectedLanguageTagHelperAttribute => "bte_injected-language-tag-helper-attribute",
            HtmlDecorationKind.InjectedLanguageTagHelperElement => "bte_injected-language-tag-helper-element",
            HtmlDecorationKind.InjectedLanguageMethod => "bte_method",
            HtmlDecorationKind.InjectedLanguageVariable => "bte_parameter",
            HtmlDecorationKind.InjectedLanguageType => "bte_type",
            HtmlDecorationKind.InjectedLanguageStringLiteral => "bte_string-literal",
            _ => string.Empty,
        };
    }
}
