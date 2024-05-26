using Watchdog.Common.Contracts;
using Watchdog.SimilarDeals.Models;

namespace Watchdog.SimilarDeals;

public sealed class SimilarDealsMapper
{
	public Deal ToModel(SimpleDeal simpleDeal)
	{
		return new Deal()
		{
			Id = Guid.NewGuid(), //globally unique deal id
			Server = "Server",
			Account = "Account",
			LocalId = $"#{simpleDeal.Number}", //deal id unique in server (data source)
			Currency = simpleDeal.Currency,
			VolumeToBalanceRatio = CalculateVolumeToBalanceRatio(simpleDeal.Balance, simpleDeal.Lot),
			TimestampUtc = simpleDeal.TimestampUtc,
		};
	}

	private static double CalculateVolumeToBalanceRatio(int balance, double lot) => lot / balance;
}
