using System.ComponentModel.DataAnnotations;

namespace Watchdog.Infrastructure.Options;

public sealed class CosmosDbAppOptions : IAppOptions
{
	public static string SectionName => "CosmosDB";

	[Required]
	public required string Endpoint { get; init; }

	[Required]
	public required string Key { get; init; }

	[Required]
	public required string DbName { get; init; }
}
