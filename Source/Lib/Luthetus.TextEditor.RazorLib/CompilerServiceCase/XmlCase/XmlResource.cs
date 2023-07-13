using Luthetus.TextEditor.RazorLib.Lexing;
using Luthetus.TextEditor.RazorLib.Model;
using System.Collections.Immutable;

namespace Luthetus.TextEditor.RazorLib.CompilerServiceCase.XmlCase;

public class XmlResource
{
    public XmlResource(
        TextEditorModelKey modelKey,
        ResourceUri resourceUri,
        TextEditorXmlCompilerService textEditorXmlCompilerService)
    {
        ModelKey = modelKey;
        ResourceUri = resourceUri;
        TextEditorXmlCompilerService = textEditorXmlCompilerService;
    }

    public TextEditorModelKey ModelKey { get; }
    public ResourceUri ResourceUri { get; }
    public TextEditorXmlCompilerService TextEditorXmlCompilerService { get; }
    public ImmutableArray<TextEditorTextSpan>? SyntacticTextSpans { get; internal set; }
}
