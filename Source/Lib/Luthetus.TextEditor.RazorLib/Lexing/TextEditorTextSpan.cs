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
    [Obsolete("This property is only meant for when running in 'DEBUG' mode and viewing the debugger. One should invoke the method: GetText() instead.")]
    public string Text => GetText();
#endif

    public string GetText()
    {
        return SourceText.Substring(
            StartingIndexInclusive,
            EndingIndexExclusive - StartingIndexInclusive);
    }
}
