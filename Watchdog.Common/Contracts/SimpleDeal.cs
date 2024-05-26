using System.Text.Json.Serialization;
using Watchdog.Common.Abstractions;

namespace Watchdog.Common.Contracts;

public sealed class SimpleDeal : IApiContract
{
	public required int Number { get; init; }
	public required int Balance { get; init; }

	[JsonConverter(typeof(JsonStringEnumConverter))]
	public required OperationType Operation { get; init; }
	public required string Currency { get; init; }
	public required double Lot { get; init; }
	public required DateTime TimestampUtc { get; init; }
}
