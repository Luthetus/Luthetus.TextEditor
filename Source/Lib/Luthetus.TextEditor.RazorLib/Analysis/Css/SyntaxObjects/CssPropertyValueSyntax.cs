﻿using System.Collections.Immutable;
using Luthetus.TextEditor.RazorLib.Analysis.Css;
using Luthetus.TextEditor.RazorLib.Analysis.Css.SyntaxEnums;
using Luthetus.TextEditor.RazorLib.Lexing;

namespace Luthetus.TextEditor.RazorLib.Analysis.Css.SyntaxObjects;

public class CssPropertyValueSyntax : ICssSyntax
{
    public CssPropertyValueSyntax(
        TextEditorTextSpan textEditorTextSpan,
        ImmutableArray<ICssSyntax> childCssSyntaxes)
    {
        ChildCssSyntaxes = childCssSyntaxes;
        TextEditorTextSpan = textEditorTextSpan;
    }

    public TextEditorTextSpan TextEditorTextSpan { get; }
    public ImmutableArray<ICssSyntax> ChildCssSyntaxes { get; }

    public CssSyntaxKind CssSyntaxKind => CssSyntaxKind.PropertyValue;
}