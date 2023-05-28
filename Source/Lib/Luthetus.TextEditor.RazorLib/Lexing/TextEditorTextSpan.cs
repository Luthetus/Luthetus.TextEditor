namespace Luthetus.TextEditor.RazorLib.Lexing;

public record TextEditorTextSpan(
    int StartingIndexInclusive,
    int EndingIndexExclusive,
    byte DecorationByte,
    ResourceUri ResourceUri,
    string SourceText)
{
#if DEBUG
    /// <summary>This expression bound property is useful because it will evaluate <see cref="GetText"/> immediately upon inspecting the object instance in the debugger.</summary>
    public string Text => GetText();
#endif

    public string GetText()
    {
        return SourceText.Substring(
            StartingIndexInclusive,
            EndingIndexExclusive - StartingIndexInclusive);
    }
}
