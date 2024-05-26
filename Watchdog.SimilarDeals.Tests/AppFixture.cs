using InfluxDB.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.InfluxDb;
using Watchdog.Common.Abstractions;

namespace Watchdog.SimilarDeals.Tests;

public sealed class AppFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
	private readonly InfluxDbContainer dbContainer = new InfluxDbBuilder()
		.WithImage("influxdb:2.7.6")
		.WithAdminToken("access_token")
		.WithUsername("user")
		.WithPassword("password")
		.WithOrganization("organization")
		.WithBucket("watchdog")
		//configuring infinite retention policy so that the testing data can be from past
		.WithRetention("0")
		.WithCleanUp(true)
		.Build();

	public async Task InitializeAsync()
	{
		await dbContainer.StartAsync();
	}

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.ConfigureServices(services =>
		{
			services.RemoveAll<INotificationService>();
			services.AddSingleton<NotificationsStorage>();
			services.AddSingleton<INotificationService, StoreNotificationInMemoryService>();

			services.RemoveAll<InfluxDBClient>();
			services.AddSingleton(new InfluxDBClient(dbContainer.GetAddress(), "access_token"));
		});
	}

	async Task IAsyncLifetime.DisposeAsync()
	{
		await dbContainer.StopAsync();
		await dbContainer.DisposeAsync();
	}
}
