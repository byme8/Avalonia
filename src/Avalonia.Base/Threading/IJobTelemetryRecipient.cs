namespace Avalonia.Threading;

public interface IJobTelemetryRecipient
{
    void OnFrameStart();

    void OnFrameEnd(DispatcherPriority priority);
}
