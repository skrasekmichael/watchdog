using InfluxDB.Client;
using InfluxDB.Client.Writes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Watchdog.Infrastructure.Options;
using Watchdog.SimilarDeals.Abstractions;
using Watchdog.SimilarDeals.Models;

namespace Watchdog.SimilarDeals.Infrastructure;

internal sealed class InfluxDbDealsRepository(
	IOptions<InfluxDbAppOptions> dbOptions,
	IOptions<SimilarDealsOptions> options,
	ILogger<InfluxDbDealsRepository> logger,
	InfluxDBClient client): IDealsRepository
{
	private readonly SimilarDealsOptions options = options.Value;
	private readonly InfluxDbAppOptions dbOptions = dbOptions.Value;
	private readonly ILogger<InfluxDbDealsRepository> logger = logger;
	private readonly InfluxDBClient client = client;

	private const string MEASUREMENT = "similar-deals";

	public async Task AddDealAsync(Deal deal, CancellationToken ct)
	{
		logger.LogInformation("Storing deal {deal}", deal);

		var pointData = PointData.Measurement("similar-deals")
			.Tag("currency", deal.Currency)
			.Tag("dealId", deal.Id.ToString())
			.Field("server", deal.Server)
			.Field("localId", deal.LocalId)
			.Field("account", deal.Account)
			.Field("volumeToBalanceRatio", deal.VolumeToBalanceRatio)
			.Timestamp(deal.TimestampUtc, InfluxDB.Client.Api.Domain.WritePrecision.Ns);

		var writer = client.GetWriteApiAsync();
		await writer.WritePointAsync(pointData, dbOptions.Bucket, dbOptions.Organization, ct);
	}

	public async Task<List<Deal>> GetSimilarDealsAsync(Deal deal, CancellationToken ct)
	{
		var ratio = (deal.VolumeToBalanceRatio * options.VolumeToBalanceRatio).ToString("f15");
		var query = $"""
			from(bucket: "{dbOptions.Bucket}")
				|> range(start: {deal.TimestampUtc.Add(-options.OpenTimeDelta):yyyy-MM-ddTHH:mm:ssZ}, stop: {deal.TimestampUtc.Add(options.OpenTimeDelta):yyyy-MM-ddTHH:mm:ssZ})
				|> filter(fn: (r) => r._measurement == "{MEASUREMENT}" and r.currency == "{deal.Currency}")
				|> pivot(rowKey: ["dealId"], columnKey: ["_field"], valueColumn: "_value")
				|> filter(fn: (r) => r.volumeToBalanceRatio - {deal.VolumeToBalanceRatio:f15} >= -{ratio} and r.volumeToBalanceRatio - {deal.VolumeToBalanceRatio:f15} <= {ratio})
			""";

		var reader = client.GetQueryApi();
		var tables = await reader.QueryAsync(query, dbOptions.Organization, ct);

		logger.LogInformation("Query: \n{query}\n returned {count} tables", query, tables.Count);

		var similarDeals = new List<Deal>(tables.Count);
		foreach (var table in tables)
		{
			foreach (var record in table.Records)
			{
				var id = new Guid((string)record.GetValueByKey("dealId"));
				if (id == deal.Id)
				{
					continue;
				}

				similarDeals.Add(new Deal
				{
					Id = id,
					Server = (string)record.GetValueByKey("server"),
					LocalId = (string)record.GetValueByKey("localId"),
					Account = (string)record.GetValueByKey("account"),
					Currency = (string)record.GetValueByKey("currency"),
					VolumeToBalanceRatio = (double)record.GetValueByKey("volumeToBalanceRatio"),
					TimestampUtc = record.GetTimeInDateTime() ?? throw new NullReferenceException()
				});
			}
		}

		return similarDeals;
	}
}
