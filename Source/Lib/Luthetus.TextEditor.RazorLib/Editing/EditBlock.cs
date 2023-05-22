namespace Luthetus.TextEditor.RazorLib.Editing;

public record EditBlock(
    TextEditKind TextEditKind,
    string DisplayName,
    string ContentSnapshot,
    string? OtherTextEditKindIdentifier);