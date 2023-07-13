using Luthetus.TextEditor.RazorLib.Lexing;
using Luthetus.TextEditor.RazorLib.Model;
using System.Collections.Immutable;

namespace Luthetus.TextEditor.RazorLib.CompilerServiceCase.JsonCase;

public class JsonResource
{
    public JsonResource(
        TextEditorModelKey modelKey,
        ResourceUri resourceUri,
        TextEditorJsonCompilerService textEditorJsonCompilerService)
    {
        ModelKey = modelKey;
        ResourceUri = resourceUri;
        TextEditorJsonCompilerService = textEditorJsonCompilerService;
    }

    public TextEditorModelKey ModelKey { get; }
    public ResourceUri ResourceUri { get; }
    public TextEditorJsonCompilerService TextEditorJsonCompilerService { get; }
    public ImmutableArray<TextEditorTextSpan>? SyntacticTextSpans { get; internal set; }
}
