using Watchdog.SimilarDeals.Contracts;
using Watchdog.SimilarDeals.Models;
using Microsoft.Extensions.Logging;
using Watchdog.Common.Abstractions;
using Watchdog.SimilarDeals.Abstractions;

namespace Watchdog.SimilarDeals;

public sealed class SimilarDealsService(
	ILogger<SimilarDealsService> logger,
	IDealsRepository dealsRepository,
	INotificationService notificationService) : IAnalysisService<Deal>
{
	private readonly ILogger<SimilarDealsService> logger = logger;
	private readonly IDealsRepository dealsRepository = dealsRepository;
	private readonly INotificationService notificationService = notificationService;

	public Task AnalyzeAsync(Deal deal, CancellationToken ct)
	{
		//run analysis asynchronously and don't wait for result for faster API response
		AnalyzeDealAsynchronously(deal, ct);
		return dealsRepository.AddDealAsync(deal, ct);
	}

	private async void AnalyzeDealAsynchronously(Deal deal, CancellationToken ct)
	{
		List<Deal> similarDeals;

		try
		{
			similarDeals = await dealsRepository.GetSimilarDealsAsync(deal, ct);
			if (similarDeals.Count == 0)
			{
				return;
			}
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failed to search for similar deals of deal {deal}.", deal);
			return;
		}

		var notification = new SimilarDealsDetected
		{
			Id = Guid.NewGuid(),
			Deal = deal,
			SimilarDeals = similarDeals,
			TimestampUtc = DateTime.UtcNow,
		};

		try
		{
			await notificationService.NotifyAsync(notification, ct);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failed to send notification {notification}", notification);
			return;
		}
	}
}
