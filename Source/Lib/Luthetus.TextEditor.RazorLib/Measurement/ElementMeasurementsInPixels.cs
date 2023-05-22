namespace Luthetus.TextEditor.RazorLib.Measurement;

public record ElementMeasurementsInPixels(
    double ScrollLeft,
    double ScrollTop,
    double ScrollWidth,
    double ScrollHeight,
    double MarginScrollHeight,
    double Width,
    double Height,
    CancellationToken MeasurementsExpiredCancellationToken);
