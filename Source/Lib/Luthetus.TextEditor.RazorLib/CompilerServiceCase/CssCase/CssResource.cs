using Luthetus.TextEditor.RazorLib.Lexing;
using Luthetus.TextEditor.RazorLib.Model;
using System.Collections.Immutable;

namespace Luthetus.TextEditor.RazorLib.CompilerServiceCase.CssCase;

public class CssResource
{
    public CssResource(
        TextEditorModelKey modelKey,
        ResourceUri resourceUri,
        TextEditorCssCompilerService textEditorCssCompilerService)
    {
        ModelKey = modelKey;
        ResourceUri = resourceUri;
        TextEditorCssCompilerService = textEditorCssCompilerService;
    }

    public TextEditorModelKey ModelKey { get; }
    public ResourceUri ResourceUri { get; }
    public TextEditorCssCompilerService TextEditorCssCompilerService { get; }
    public ImmutableArray<TextEditorTextSpan>? SyntacticTextSpans { get; internal set; }
}
