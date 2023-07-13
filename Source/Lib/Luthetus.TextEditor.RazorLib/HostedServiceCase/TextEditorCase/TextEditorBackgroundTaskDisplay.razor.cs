using Luthetus.Common.RazorLib.BackgroundTaskCase.BaseTypes;
using Microsoft.AspNetCore.Components;

namespace Luthetus.TextEditor.RazorLib.HostedServiceCase.TextEditorCase;

public partial class TextEditorBackgroundTaskDisplay : ComponentBase
{
    [Parameter, EditorRequired]
    public IBackgroundTask BackgroundTask { get; set; } = null!;
}