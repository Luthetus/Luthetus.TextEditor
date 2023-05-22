using System.Collections.Immutable;
using Luthetus.TextEditor.RazorLib.Analysis.Json.SyntaxEnums;
using Luthetus.TextEditor.RazorLib.Lexing;

namespace Luthetus.TextEditor.RazorLib.Analysis.Json.SyntaxObjects;

public class JsonPropertyKeySyntax : IJsonSyntax
{
    public JsonPropertyKeySyntax(
        TextEditorTextSpan textEditorTextSpan,
        ImmutableArray<IJsonSyntax> childJsonSyntaxes)
    {
        ChildJsonSyntaxes = childJsonSyntaxes;
        TextEditorTextSpan = textEditorTextSpan;
    }

    public TextEditorTextSpan TextEditorTextSpan { get; }
    public ImmutableArray<IJsonSyntax> ChildJsonSyntaxes { get; }

    public JsonSyntaxKind JsonSyntaxKind => JsonSyntaxKind.PropertyKey;
}