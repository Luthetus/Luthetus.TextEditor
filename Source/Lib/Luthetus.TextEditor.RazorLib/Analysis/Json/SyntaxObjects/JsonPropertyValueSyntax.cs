﻿using System.Collections.Immutable;
using Luthetus.TextEditor.RazorLib.Analysis.Json.SyntaxEnums;
using Luthetus.TextEditor.RazorLib.Lexing;

namespace Luthetus.TextEditor.RazorLib.Analysis.Json.SyntaxObjects;

public class JsonPropertyValueSyntax : IJsonSyntax
{
    public JsonPropertyValueSyntax(
        TextEditorTextSpan textEditorTextSpan,
        IJsonSyntax underlyingJsonSyntax)
    {
        UnderlyingJsonSyntax = underlyingJsonSyntax;
        TextEditorTextSpan = textEditorTextSpan;
    }

    public static JsonPropertyValueSyntax GetInvalidJsonPropertyValueSyntax()
    {
        return new JsonPropertyValueSyntax(
            new TextEditorTextSpan(
                0,
                0,
                default,
                new ResourceUri(string.Empty),
                string.Empty),
            null);
    }

    public IJsonSyntax UnderlyingJsonSyntax { get; }
    public TextEditorTextSpan TextEditorTextSpan { get; }
    public ImmutableArray<IJsonSyntax> ChildJsonSyntaxes => new IJsonSyntax[]
    {
        UnderlyingJsonSyntax
    }.ToImmutableArray();

    public JsonSyntaxKind JsonSyntaxKind => JsonSyntaxKind.PropertyValue;
}