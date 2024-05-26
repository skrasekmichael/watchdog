using Watchdog.Common.Abstractions;
using Watchdog.SimilarDeals.Models;

namespace Watchdog.SimilarDeals.Contracts;

public sealed record SimilarDealsDetected : INotification
{
	public Guid Id { get; init; }
	public required Deal Deal { get; init; }
	public required List<Deal> SimilarDeals { get; init; }
	public required DateTime TimestampUtc { get; init; }

	public override string ToString()
	{
		return $$"""
			{
				SimilarDeals: [
					{{string.Join(",\n\t\t", SimilarDeals.Select(x => x.ToString()))}}
				],
				TimestampUtc: {{TimestampUtc}}
			}
			""";
	}
}
