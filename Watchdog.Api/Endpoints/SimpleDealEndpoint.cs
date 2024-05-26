using FastEndpoints;
using Watchdog.Common.Contracts;
using Watchdog.SimilarDeals;

namespace Watchdog.Api.Endpoints;

public class SimpleDealEndpoint(
	SimilarDealsService similarDealsService,
	SimilarDealsMapper similarDealsMapper) : Endpoint<SimpleDeal>
{
	private readonly SimilarDealsService similarDealsService = similarDealsService;
	private readonly SimilarDealsMapper similarDealsMapper = similarDealsMapper;

	public override void Configure()
	{
		Post("/deals/simple-deal");
		AllowAnonymous();
	}

	public override async Task HandleAsync(SimpleDeal request, CancellationToken ct)
	{
		await similarDealsService.AnalyzeAsync(similarDealsMapper.ToModel(request), ct);

		//additional analysis of incoming data from this data source

		await SendOkAsync(ct);
	}
}
