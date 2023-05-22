﻿using System.Collections.Immutable;
using Luthetus.TextEditor.RazorLib.Analysis.Html;
using Luthetus.TextEditor.RazorLib.Analysis.Html.SyntaxEnums;

namespace Luthetus.TextEditor.RazorLib.Analysis.Html.SyntaxObjects;

public class TagSyntax : IHtmlSyntax
{
    public TagSyntax(
        TagNameSyntax? openTagNameSyntax,
        TagNameSyntax? closeTagNameSyntax,
        ImmutableArray<AttributeSyntax> attributeSyntaxes,
        ImmutableArray<IHtmlSyntax> childHtmlSyntaxes,
        TagKind tagKind,
        bool hasSpecialHtmlCharacter = false)
    {
        ChildHtmlSyntaxes = childHtmlSyntaxes;
        HasSpecialHtmlCharacter = hasSpecialHtmlCharacter;
        AttributeSyntaxes = attributeSyntaxes;
        OpenTagNameSyntax = openTagNameSyntax;
        CloseTagNameSyntax = closeTagNameSyntax;
        TagKind = tagKind;
    }

    public TagNameSyntax? OpenTagNameSyntax { get; }
    public TagNameSyntax? CloseTagNameSyntax { get; }
    public ImmutableArray<AttributeSyntax> AttributeSyntaxes { get; }
    public TagKind TagKind { get; }
    public bool HasSpecialHtmlCharacter { get; }

    public virtual ImmutableArray<IHtmlSyntax> ChildHtmlSyntaxes { get; }
    public virtual HtmlSyntaxKind HtmlSyntaxKind => HtmlSyntaxKind.Tag;

    public class TagSyntaxBuilder
    {
        public TagNameSyntax? OpenTagNameSyntax { get; set; }
        public TagNameSyntax? CloseTagNameSyntax { get; set; }
        public List<AttributeSyntax> AttributeSyntaxes { get; set; } = new();
        public List<IHtmlSyntax> ChildHtmlSyntaxes { get; set; } = new();
        public TagKind TagKind { get; set; }
        public bool HasSpecialHtmlCharacter { get; set; }

        public TagSyntax Build()
        {
            return new TagSyntax(
                OpenTagNameSyntax,
                CloseTagNameSyntax,
                AttributeSyntaxes.ToImmutableArray(),
                ChildHtmlSyntaxes.ToImmutableArray(),
                TagKind,
                HasSpecialHtmlCharacter);
        }
    }
}