using System.ComponentModel.DataAnnotations;

namespace Watchdog.Infrastructure.Options;

public sealed class InfluxDbAppOptions : IAppOptions
{
	public static string SectionName => "InfluxDB";

	[Required]
	public required string Url { get; init; }

	[Required]
	public required string Token { get; init; }

	[Required]
	public required string Organization { get; init; }

	[Required]
	public required string Bucket { get; init; }
}
