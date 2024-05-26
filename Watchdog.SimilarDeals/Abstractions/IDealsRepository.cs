using Watchdog.SimilarDeals.Models;

namespace Watchdog.SimilarDeals.Abstractions;

public interface IDealsRepository
{
	public Task AddDealAsync(Deal deal, CancellationToken ct);
	public Task<List<Deal>> GetSimilarDealsAsync(Deal deal, CancellationToken ct);
}
