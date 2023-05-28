using Luthetus.TextEditor.RazorLib.Decoration;

namespace Luthetus.TextEditor.RazorLib.Analysis.GenericLexer.Decoration;

public class GenericDecorationMapper : IDecorationMapper
{
    public string Map(byte decorationByte)
    {
        var decoration = (GenericDecorationKind)decorationByte;

        return decoration switch
        {
            GenericDecorationKind.None => string.Empty,
            GenericDecorationKind.Keyword => "luth_te_keyword",
            GenericDecorationKind.KeywordControl => "luth_te_keyword-control",
            GenericDecorationKind.StringLiteral => "luth_te_string-literal",
            GenericDecorationKind.CommentSingleLine => "luth_te_comment",
            GenericDecorationKind.CommentMultiLine => "luth_te_comment",
            GenericDecorationKind.Function => "luth_te_method",
            GenericDecorationKind.PreprocessorDirective => "luth_te_keyword",
            GenericDecorationKind.DeliminationExtended => "luth_te_string-literal",
            GenericDecorationKind.Type => "luth_te_type",
            _ => string.Empty,
        };
    }
}
