namespace Luthetus.TextEditor.RazorLib.Lexing;

public record TextEditorTextSpan(
    int StartingIndexInclusive,
    int EndingIndexExclusive,
    byte DecorationByte,
    ResourceUri ResourceUri,
    string SourceText)
{
    public string GetText()
    {
        return SourceText.Substring(
            StartingIndexInclusive,
            EndingIndexExclusive - StartingIndexInclusive);
    }
}
