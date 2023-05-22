namespace Luthetus.TextEditor.RazorLib.Editing;

public enum TextEditKind
{
    None,
    InitialState,
    Other,
    Insertion,
    Deletion,
    ForcePersistEditBlock
}