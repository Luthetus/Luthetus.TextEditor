using System.Collections.Immutable;
using Luthetus.TextEditor.RazorLib.Analysis.Json.SyntaxEnums;
using Luthetus.TextEditor.RazorLib.Lexing;

namespace Luthetus.TextEditor.RazorLib.Analysis.Json.SyntaxObjects;

public class JsonObjectSyntax : IJsonSyntax
{
    public JsonObjectSyntax(
        TextEditorTextSpan textEditorTextSpan,
        ImmutableArray<JsonPropertySyntax> jsonPropertySyntaxes)
    {
        TextEditorTextSpan = textEditorTextSpan;

        // To avoid re-evaluating the Select() for casting as (IJsonSyntax)
        // every time the ChildJsonSyntaxes getter is accessed
        // this is being done here initially on construction once.
        JsonPropertySyntaxes = jsonPropertySyntaxes
            .Select(x => (IJsonSyntax)x)
            .ToImmutableArray();
    }

    public TextEditorTextSpan TextEditorTextSpan { get; }
    public ImmutableArray<IJsonSyntax> JsonPropertySyntaxes { get; }
    public ImmutableArray<IJsonSyntax> ChildJsonSyntaxes => JsonPropertySyntaxes;

    public JsonSyntaxKind JsonSyntaxKind => JsonSyntaxKind.Object;
}