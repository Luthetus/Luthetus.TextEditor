using System.Collections.Immutable;
using Luthetus.TextEditor.RazorLib.Analysis.GenericLexer.Decoration;
using Luthetus.TextEditor.RazorLib.Analysis.Html.SyntaxEnums;
using Luthetus.TextEditor.RazorLib.Lexing;

namespace Luthetus.TextEditor.RazorLib.Analysis.Html.SyntaxObjects;

public class AttributeNode : IHtmlSyntaxNode
{
    public AttributeNode(
        AttributeNameNode attributeNameSyntax,
        AttributeValueNode attributeValueSyntax)
    {
        AttributeNameSyntax = attributeNameSyntax;
        AttributeValueSyntax = attributeValueSyntax;

        Children = new IHtmlSyntax[]
        {
            AttributeNameSyntax,
            AttributeValueSyntax,
        }.ToImmutableArray();
    }

    public AttributeNameNode AttributeNameSyntax { get; }
    public AttributeValueNode AttributeValueSyntax { get; }

    public ImmutableArray<IHtmlSyntax> Children { get; }

    public TextEditorTextSpan TextEditorTextSpan => new(
        AttributeNameSyntax.TextEditorTextSpan.StartingIndexInclusive,
        AttributeValueSyntax.TextEditorTextSpan.EndingIndexExclusive,
        (byte)GenericDecorationKind.None,
        AttributeNameSyntax.TextEditorTextSpan.ResourceUri,
        AttributeNameSyntax.TextEditorTextSpan.SourceText);
 
    public HtmlSyntaxKind HtmlSyntaxKind => HtmlSyntaxKind.AttributeNode;
}