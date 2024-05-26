namespace Watchdog.Common.Abstractions;

public interface INotificationService
{
	public ValueTask NotifyAsync<TNotification>(TNotification notification, CancellationToken ct) where TNotification : INotification;
}
