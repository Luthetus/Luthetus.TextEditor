using Luthetus.TextEditor.RazorLib.Measurement;

namespace Luthetus.TextEditor.RazorLib.Virtualization;

public interface IVirtualizationResultWithoutTypeMask
{
    public VirtualizationBoundary LeftVirtualizationBoundary { get; init; }
    public VirtualizationBoundary RightVirtualizationBoundary { get; init; }
    public VirtualizationBoundary TopVirtualizationBoundary { get; init; }
    public VirtualizationBoundary BottomVirtualizationBoundary { get; init; }
    public ElementMeasurementsInPixels ElementMeasurementsInPixels { get; init; }
}