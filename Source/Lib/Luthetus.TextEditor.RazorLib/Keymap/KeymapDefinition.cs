using System.Text.Json.Serialization;

namespace Luthetus.TextEditor.RazorLib.Keymap;

public record KeymapDefinition(
    KeymapKey KeymapKey,
    string DisplayName,
    [property: JsonIgnore] ITextEditorKeymap Keymap);