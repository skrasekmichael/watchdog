using Watchdog.Common.Abstractions;

namespace Watchdog.SimilarDeals.Models;

public sealed record Deal : IAnalysisData
{
	public required Guid Id { get; init; }
	public required string Server { get; init; }
	public required string LocalId { get; init; }
	public required string Account { get; init; }
	public required string Currency { get; init; }
	public required double VolumeToBalanceRatio { get; init; }
	public required DateTime TimestampUtc { get; init; }
}
