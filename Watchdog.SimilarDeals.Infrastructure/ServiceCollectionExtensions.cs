using Microsoft.Extensions.DependencyInjection;
using Watchdog.Infrastructure;
using Watchdog.SimilarDeals.Abstractions;

namespace Watchdog.SimilarDeals.Infrastructure;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddSimilarDealsModule(this IServiceCollection services)
	{
		services.AddAppOptions<SimilarDealsOptions>();

		services.AddSingleton<IDealsRepository, InfluxDbDealsRepository>();

		services
			.AddSingleton<SimilarDealsService>()
			.AddSingleton<SimilarDealsMapper>();

		return services;
	}
}
