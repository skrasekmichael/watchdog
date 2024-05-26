using System.ComponentModel.DataAnnotations;
using Watchdog.Infrastructure.Options;

namespace Watchdog.SimilarDeals.Infrastructure;

internal sealed class SimilarDealsOptions : IAppOptions
{
	public static string SectionName => "SimilarDeals";

	[Required]
	public required double VolumeToBalanceRatio { get; init; }

	[Required]
	public required TimeSpan OpenTimeDelta { get; init; }
}
