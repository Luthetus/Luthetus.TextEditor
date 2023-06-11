using Luthetus.Common.RazorLib.Reactive;

namespace Luthetus.TextEditor.RazorLib.ViewModel;

public class BatchScrollEvents
{
    public IThrottle ThrottleMutateScrollHorizontalPositionByPixels { get; } = new Throttle(IThrottle.DefaultThrottleTimeSpan);
    public IThrottle ThrottleMutateScrollVerticalPositionByPixels { get; } = new Throttle(IThrottle.DefaultThrottleTimeSpan);
    public IThrottle ThrottleSetScrollPosition { get; } = new Throttle(IThrottle.DefaultThrottleTimeSpan);

    public double MutateScrollHorizontalPositionByPixels { get; set; }
    public double MutateScrollVerticalPositionByPixels { get; set; }
}