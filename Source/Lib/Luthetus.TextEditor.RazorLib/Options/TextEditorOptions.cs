using Luthetus.Common.RazorLib.Misc;
using Luthetus.Common.RazorLib.Options;
using Luthetus.TextEditor.RazorLib.Keymap;

namespace Luthetus.TextEditor.RazorLib.Options;

public record TextEditorOptions(
    CommonOptions? CommonOptions,
    bool? ShowWhitespace,
    bool? ShowNewlines,
    int? TextEditorHeightInPixels,
    double? CursorWidthInPixels,
    KeymapDefinition? KeymapDefinition,
    bool UseMonospaceOptimizations)
{
    public RenderStateKey RenderStateKey { get; init; } = RenderStateKey.NewRenderStateKey();
}