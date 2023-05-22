using System.Collections.Immutable;
using Luthetus.TextEditor.RazorLib.Analysis.Html;
using Luthetus.TextEditor.RazorLib.Analysis.Html.SyntaxEnums;

namespace Luthetus.TextEditor.RazorLib.Analysis.Html.SyntaxObjects;

public class AttributeSyntax : IHtmlSyntax
{
    public AttributeSyntax(
        AttributeNameSyntax attributeNameSyntax,
        AttributeValueSyntax attributeValueSyntax)
    {
        AttributeNameSyntax = attributeNameSyntax;
        AttributeValueSyntax = attributeValueSyntax;
    }

    public AttributeNameSyntax AttributeNameSyntax { get; }
    public AttributeValueSyntax AttributeValueSyntax { get; }

    public HtmlSyntaxKind HtmlSyntaxKind => HtmlSyntaxKind.Attribute;
    public ImmutableArray<IHtmlSyntax> ChildHtmlSyntaxes { get; } = ImmutableArray<IHtmlSyntax>.Empty;
}