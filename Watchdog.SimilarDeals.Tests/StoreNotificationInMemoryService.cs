using Watchdog.Common.Abstractions;

namespace Watchdog.SimilarDeals.Tests;

public sealed class NotificationsStorage : List<INotification>;

internal class StoreNotificationInMemoryService(NotificationsStorage notifications) : INotificationService
{
	public ValueTask NotifyAsync<TNotification>(TNotification notification, CancellationToken ct) where TNotification : INotification
	{
		lock (notifications)
		{
			notifications.Add(notification);
		}

		return ValueTask.CompletedTask;
	}
}
