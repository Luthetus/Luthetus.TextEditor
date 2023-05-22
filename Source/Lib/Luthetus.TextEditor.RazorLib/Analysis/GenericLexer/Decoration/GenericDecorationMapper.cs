﻿using Luthetus.TextEditor.RazorLib.Decoration;

namespace Luthetus.TextEditor.RazorLib.Analysis.GenericLexer.Decoration;

public class GenericDecorationMapper : IDecorationMapper
{
    public string Map(byte decorationByte)
    {
        var decoration = (GenericDecorationKind)decorationByte;

        return decoration switch
        {
            GenericDecorationKind.None => string.Empty,
            GenericDecorationKind.Keyword => "bte_keyword",
            GenericDecorationKind.KeywordControl => "bte_keyword-control",
            GenericDecorationKind.StringLiteral => "bte_string-literal",
            GenericDecorationKind.CommentSingleLine => "bte_comment",
            GenericDecorationKind.CommentMultiLine => "bte_comment",
            GenericDecorationKind.Function => "bte_method",
            GenericDecorationKind.PreprocessorDirective => "bte_keyword",
            GenericDecorationKind.DeliminationExtended => "bte_string-literal",
            _ => string.Empty,
        };
    }
}
