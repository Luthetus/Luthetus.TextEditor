using Luthetus.Common.RazorLib.BackgroundTaskCase.BaseTypes;

namespace Luthetus.TextEditor.RazorLib.HostedServiceCase;

public class TextEditorBackgroundTaskQueueSingleThreaded : ITextEditorBackgroundTaskQueue
{
    public void QueueBackgroundWorkItem(
        IBackgroundTask backgroundTask)
    {
        backgroundTask
            .InvokeWorkItem(CancellationToken.None)
            .Wait();
    }

    public Task<IBackgroundTask?> DequeueAsync(
        CancellationToken cancellationToken)
    {
        return Task.FromResult(default(IBackgroundTask?));
    }
}