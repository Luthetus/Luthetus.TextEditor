using Luthetus.TextEditor.RazorLib.Model;
using Luthetus.TextEditor.RazorLib.Options;

namespace Luthetus.TextEditor.RazorLib.ViewModel;

public record TextEditorRenderBatch(
    TextEditorModel? Model,
    TextEditorViewModel? ViewModel,
    TextEditorOptions? Options);
