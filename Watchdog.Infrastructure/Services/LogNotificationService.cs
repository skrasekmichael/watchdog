using Microsoft.Extensions.Logging;
using Watchdog.Common.Abstractions;

namespace Watchdog.Infrastructure.Services;

internal sealed class LogNotificationService(ILogger<LogNotificationService> logger) : INotificationService
{
	private readonly ILogger<LogNotificationService> logger = logger;

	public ValueTask NotifyAsync<TNotification>(TNotification notification, CancellationToken ct)
		where TNotification : INotification
	{
		logger.LogWarning("Notify {notification} at {timestamp}", notification, notification.TimestampUtc);

		return ValueTask.CompletedTask;
	}
}
