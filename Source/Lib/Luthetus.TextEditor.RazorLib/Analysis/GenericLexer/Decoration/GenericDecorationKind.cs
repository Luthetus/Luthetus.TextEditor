﻿namespace Luthetus.TextEditor.RazorLib.Analysis.GenericLexer.Decoration;

public enum GenericDecorationKind
{
    None,
    Keyword,
    KeywordControl,
    CommentSingleLine,
    CommentMultiLine,
    Error,
    StringLiteral,
    Variable,
    Function,
    PreprocessorDirective,
    DeliminationExtended,
    Type,
    Property,
}