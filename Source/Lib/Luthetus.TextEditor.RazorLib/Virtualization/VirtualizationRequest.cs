namespace Luthetus.TextEditor.RazorLib.Virtualization;

public record VirtualizationRequest(
    VirtualizationScrollPosition ScrollPosition,
    CancellationToken CancellationToken);