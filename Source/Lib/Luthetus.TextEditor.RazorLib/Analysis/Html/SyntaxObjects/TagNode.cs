using System.Collections.Immutable;
using Luthetus.TextEditor.RazorLib.Analysis.GenericLexer.Decoration;
using Luthetus.TextEditor.RazorLib.Analysis.Html.SyntaxEnums;
using Luthetus.TextEditor.RazorLib.Lexing;

namespace Luthetus.TextEditor.RazorLib.Analysis.Html.SyntaxObjects;

public class TagNode : IHtmlSyntaxNode
{
    public TagNode(
        TagNameNode? openTagNameSyntax,
        TagNameNode? closeTagNameSyntax,
        ImmutableArray<AttributeNode> attributeSyntaxes,
        ImmutableArray<IHtmlSyntax> childHtmlSyntaxes,
        HtmlSyntaxKind htmlSyntaxKind,
        bool hasSpecialHtmlCharacter = false)
    {
        Children = childHtmlSyntaxes;
        HtmlSyntaxKind = htmlSyntaxKind;
        HasSpecialHtmlCharacter = hasSpecialHtmlCharacter;
        AttributeSyntaxes = attributeSyntaxes;
        OpenTagNameSyntax = openTagNameSyntax;
        CloseTagNameSyntax = closeTagNameSyntax;
    }

    public TagNameNode? OpenTagNameSyntax { get; }
    public TagNameNode? CloseTagNameSyntax { get; }
    public ImmutableArray<AttributeNode> AttributeSyntaxes { get; }
    public bool HasSpecialHtmlCharacter { get; }

    public ImmutableArray<IHtmlSyntax> Children { get; }
    public HtmlSyntaxKind HtmlSyntaxKind { get; }

    public TextEditorTextSpan TextEditorTextSpan => new(
        0,
        0,
        (byte)GenericDecorationKind.None,
        new ResourceUri(string.Empty),
        string.Empty);
}
