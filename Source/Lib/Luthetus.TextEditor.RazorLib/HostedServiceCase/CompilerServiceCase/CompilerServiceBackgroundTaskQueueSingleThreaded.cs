using Luthetus.Common.RazorLib.BackgroundTaskCase.BaseTypes;

namespace Luthetus.TextEditor.RazorLib.HostedServiceCase.CompilerServiceCase;

public class CompilerServiceBackgroundTaskQueueSingleThreaded : ICompilerServiceBackgroundTaskQueue
{
    public void QueueBackgroundWorkItem(
        IBackgroundTask backgroundTask)
    {
        _ = Task.Run(async () =>
        {
            await backgroundTask
                .InvokeWorkItem(CancellationToken.None);
        });
    }

    public Task<IBackgroundTask?> DequeueAsync(
        CancellationToken cancellationToken)
    {
        return Task.FromResult(default(IBackgroundTask?));
    }
}