namespace Watchdog.Common.Abstractions;

public interface INotification
{
	public Guid Id { get; init; }
	public DateTime TimestampUtc { get; init; }
}
