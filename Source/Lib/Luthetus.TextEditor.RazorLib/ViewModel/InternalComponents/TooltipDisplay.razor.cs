using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Luthetus.Common.RazorLib.CustomEvents;
using Luthetus.Common.RazorLib.Icons;
using Luthetus.Common.RazorLib.Icons.Codicon;
using Luthetus.Common.RazorLib.JavaScriptObjects;

namespace Luthetus.TextEditor.RazorLib.ViewModel.InternalComponents;

public partial class TooltipDisplay : ComponentBase
{
    [Parameter, EditorRequired]
    public (string message, RelativeCoordinates relativeCoordinates)? MouseStoppedEventMostRecentResult { get; set; }
}