﻿using Luthetus.TextEditor.RazorLib.Decoration;

namespace Luthetus.TextEditor.RazorLib.Analysis.Css.Decoration;

public class TextEditorCssDecorationMapper : IDecorationMapper
{
    public string Map(byte decorationByte)
    {
        var decoration = (CssDecorationKind)decorationByte;

        return decoration switch
        {
            CssDecorationKind.None => string.Empty,
            CssDecorationKind.Identifier => "bte_css-identifier",
            CssDecorationKind.PropertyName => "bte_css-property-name",
            CssDecorationKind.PropertyValue => "bte_css-property-value",
            CssDecorationKind.Comment => "bte_comment",
            _ => string.Empty,
        };
    }
}
