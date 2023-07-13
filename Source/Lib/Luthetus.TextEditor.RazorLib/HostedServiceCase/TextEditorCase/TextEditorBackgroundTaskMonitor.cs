using Luthetus.Common.RazorLib.Store.NotificationCase;
using Luthetus.Common.RazorLib.Notification;
using Luthetus.Common.RazorLib.BackgroundTaskCase.BaseTypes;
using Luthetus.Common.RazorLib.ComponentRenderers;

namespace Luthetus.TextEditor.RazorLib.HostedServiceCase.TextEditorCase;

public class TextEditorBackgroundTaskMonitor : ITextEditorBackgroundTaskMonitor
{
    private readonly ILuthetusCommonComponentRenderers _luthetusCommonComponentRenderers;

    public TextEditorBackgroundTaskMonitor(
        ILuthetusCommonComponentRenderers luthetusCommonComponentRenderers)
    {
        _luthetusCommonComponentRenderers = luthetusCommonComponentRenderers;
    }

    public IBackgroundTask? ExecutingBackgroundTask { get; private set; }

    public event Action? ExecutingBackgroundTaskChanged;

    public void SetExecutingBackgroundTask(IBackgroundTask? backgroundTask)
    {
        ExecutingBackgroundTask = backgroundTask;
        ExecutingBackgroundTaskChanged?.Invoke();

        if (backgroundTask is not null &&
            backgroundTask.ShouldNotifyWhenStartingWorkItem &&
            backgroundTask.Dispatcher is not null)
        {
            var notificationRecord = new NotificationRecord(
                NotificationKey.NewNotificationKey(),
                $"Starting: {backgroundTask.Name}",
                typeof(TextEditorBackgroundTaskDisplay),
                new Dictionary<string, object?>
                {
                    {
                        nameof(TextEditorBackgroundTaskDisplay.BackgroundTask),
                        backgroundTask
                    }
                },
                null,
                null);

            backgroundTask.Dispatcher.Dispatch(
                new NotificationRecordsCollection.RegisterAction(
                    notificationRecord));
        }
    }
}