﻿namespace Luthetus.TextEditor.RazorLib.Analysis.Css.Decoration;

public enum CssDecorationKind
{
    None,
    TagSelector,
    Comment,
    PropertyName,
    PropertyValue,
    UnexpectedToken,
    Identifier
}