using Microsoft.AspNetCore.Components;
using Luthetus.Common.RazorLib.JavaScriptObjects;

namespace Luthetus.TextEditor.RazorLib.ViewModel.InternalComponents;

public partial class TooltipDisplay : ComponentBase
{
    [Parameter, EditorRequired]
    public (string message, RelativeCoordinates relativeCoordinates)? MouseStoppedEventMostRecentResult { get; set; }
}