using InfluxDB.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Watchdog.Common.Abstractions;
using Watchdog.Infrastructure.Options;
using Watchdog.Infrastructure.Services;

namespace Watchdog.Infrastructure;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddAppOptions<TOptions>(this IServiceCollection services)
		where TOptions : class, IAppOptions
	{
		services.AddOptions<TOptions>()
			.BindConfiguration(TOptions.SectionName)
			.ValidateDataAnnotations()
			.ValidateOnStart();

		return services;
	}

	public static IServiceCollection AddInfrastructure(this IServiceCollection services)
	{
		services
			.AddAppOptions<CosmosDbAppOptions>()
			.AddAppOptions<InfluxDbAppOptions>();

		services.AddSingleton<INotificationService, LogNotificationService>();

		services.AddSingleton(serviceProvider =>
		{
			var options = serviceProvider.GetRequiredService<IOptions<InfluxDbAppOptions>>().Value;
			return new InfluxDBClient(options.Url, options.Token);
		});

		return services;
	}
}
