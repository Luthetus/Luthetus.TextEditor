using Luthetus.TextEditor.RazorLib.Lexing;

namespace Luthetus.TextEditor.RazorLib.Semantics;

public class TextEditorSymbolDefinition
{
    public TextEditorSymbolDefinition(
        ResourceUri resourceUri,
        int positionIndex)
    {
        ResourceUri = resourceUri;
        PositionIndex = positionIndex;
    }

    public ResourceUri ResourceUri { get; }
    public int PositionIndex { get; }
}