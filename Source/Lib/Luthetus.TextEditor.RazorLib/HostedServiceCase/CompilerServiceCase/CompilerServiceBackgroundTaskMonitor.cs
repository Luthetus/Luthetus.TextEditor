using Luthetus.Common.RazorLib.Store.NotificationCase;
using Luthetus.Common.RazorLib.Notification;
using Luthetus.Common.RazorLib.BackgroundTaskCase.BaseTypes;
using Luthetus.Common.RazorLib.ComponentRenderers;
using Luthetus.Ide.ClassLib.ComponentRenderers.Types;

namespace Luthetus.TextEditor.RazorLib.HostedServiceCase.CompilerServiceCase;

public class CompilerServiceBackgroundTaskMonitor : ICompilerServiceBackgroundTaskMonitor
{
    private readonly ILuthetusCommonComponentRenderers _luthetusCommonComponentRenderers;

    public CompilerServiceBackgroundTaskMonitor(
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
            backgroundTask.Dispatcher is not null &&
            _luthetusCommonComponentRenderers.CompilerServiceBackgroundTaskDisplayRendererType is not null)
        {
            var notificationRecord = new NotificationRecord(
                NotificationKey.NewNotificationKey(),
                $"Starting: {backgroundTask.Name}",
                _luthetusCommonComponentRenderers.CompilerServiceBackgroundTaskDisplayRendererType,
                new Dictionary<string, object?>
                {
                    {
                        nameof(ICompilerServiceBackgroundTaskDisplayRendererType.BackgroundTask),
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