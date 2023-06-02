namespace Luthetus.TextEditor.RazorLib.Semantics;

public class TextEditorSymbolDefinition
{
    public TextEditorSymbolDefinition(
        string resourceUri,
        int positionIndex)
    {
        ResourceUri = resourceUri;
        PositionIndex = positionIndex;
    }

    public string ResourceUri { get; }
    public int PositionIndex { get; }
}