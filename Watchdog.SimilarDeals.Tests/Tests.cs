using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using Watchdog.Common.Contracts;
using Watchdog.SimilarDeals.Contracts;

namespace Watchdog.SimilarDeals.Tests;

public sealed class Tests(AppFixture app) : IClassFixture<AppFixture>
{
	private readonly HttpClient client = app.CreateClient();

	private readonly NotificationsStorage notifications = app.Services.GetRequiredService<NotificationsStorage>();

	[Fact]
	public async Task Watchdog_Should_NotifyAboutTwoSimilarDeals()
	{
		//arrange
		notifications.Clear();

		var deal1 = new SimpleDeal()
		{
			Number = 1,
			Balance = 10000,
			Operation = OperationType.Buy,
			Currency = "EURUSD",
			Lot = 1.0,
			TimestampUtc = new DateTime(2024, 05, 12, 14, 43, 12)
		};

		var deal2 = new SimpleDeal()
		{
			Number = 2,
			Balance = 10000,
			Operation = OperationType.Sell,
			Currency = "GBPUSD",
			Lot = 0.2,
			TimestampUtc = new DateTime(2024, 05, 12, 14, 43, 23)
		};

		var deal3 = new SimpleDeal()
		{
			Number = 3,
			Balance = 10000,
			Operation = OperationType.Sell,
			Currency = "GBPUSD",
			Lot = 0.21,
			TimestampUtc = new DateTime(2024, 05, 12, 14, 43, 24)
		};

		//act
		var res1 = await client.PostAsJsonAsync("/deals/simple-deal", deal1);
		var res3 = await client.PostAsJsonAsync("/deals/simple-deal", deal2);
		var res2 = await client.PostAsJsonAsync("/deals/simple-deal", deal3);

		//arrange
		res1.Should().Be200Ok();
		res2.Should().Be200Ok();
		res3.Should().Be200Ok();

		await Task.Delay(500); //wait for the analysis to be completed

		var similarDealsNotifications = notifications.OfType<SimilarDealsDetected>().ToList();
		similarDealsNotifications.Should().ContainSingle();

		var notification = similarDealsNotifications[0];
		notification.SimilarDeals.Should().ContainSingle();
		notification.Deal.LocalId.Should().Be("#3", "deal #3 triggered notification");
		notification.SimilarDeals[0].LocalId.Should().Be("#2", "deal #2 is similar");
	}

	[Fact]
	public async Task Watchdog_Should_NotifyAboutThreeSimilarDeals()
	{
		//arrange
		notifications.Clear();

		var deal1 = new SimpleDeal()
		{
			Number = 1,
			Balance = 10000,
			Operation = OperationType.Buy,
			Currency = "EURUSD",
			Lot = 1.0,
			TimestampUtc = new DateTime(2024, 04, 12, 14, 43, 12)
		};

		var deal2 = new SimpleDeal()
		{
			Number = 2,
			Balance = 10000,
			Operation = OperationType.Sell,
			Currency = "GBPUSD",
			Lot = 0.2,
			TimestampUtc = new DateTime(2024, 04, 12, 14, 43, 23)
		};

		var deal3 = new SimpleDeal()
		{
			Number = 3,
			Balance = 1000,
			Operation = OperationType.Sell,
			Currency = "GBPUSD",
			Lot = 1.2,
			TimestampUtc = new DateTime(2024, 04, 12, 14, 43, 23)
		};

		var deal4 = new SimpleDeal()
		{
			Number = 4,
			Balance = 10000,
			Operation = OperationType.Sell,
			Currency = "GBPUSD",
			Lot = 0.21,
			TimestampUtc = new DateTime(2024, 04, 12, 14, 43, 24)
		};

		var deal5 = new SimpleDeal()
		{
			Number = 5,
			Balance = 20000,
			Operation = OperationType.Sell,
			Currency = "GBPUSD",
			Lot = 0.4,
			TimestampUtc = new DateTime(2024, 04, 12, 14, 43, 24)
		};

		//act
		var responses = new List<HttpResponseMessage>
		{
			await client.PostAsJsonAsync("/deals/simple-deal", deal1),
			await client.PostAsJsonAsync("/deals/simple-deal", deal2),
			await client.PostAsJsonAsync("/deals/simple-deal", deal3),
			await client.PostAsJsonAsync("/deals/simple-deal", deal4),
			await client.PostAsJsonAsync("/deals/simple-deal", deal5),
		};

		//arrange
		responses.ForEach(r => r.Should().Be200Ok());

		await Task.Delay(500); //wait for the analysis to be completed

		var similarDealsNotifications = notifications.OfType<SimilarDealsDetected>().ToList();
		similarDealsNotifications.Should().HaveCount(2);

		var notification1 = similarDealsNotifications[0];
		notification1.SimilarDeals.Should().ContainSingle();
		notification1.Deal.LocalId.Should().Be("#4", "deal #4 is notification triggering deal");
		notification1.SimilarDeals[0].LocalId.Should().Be("#2", "deal #2 is similar");

		var notification2 = similarDealsNotifications[1];
		notification2.SimilarDeals.Should().HaveCount(2);
		notification2.Deal.LocalId.Should().Be("#5", "deal #5 is notification triggering deal");
		notification2.SimilarDeals.Should().Contain(x => x.LocalId == "#2", "deal #2 is similar");
		notification2.SimilarDeals.Should().Contain(x => x.LocalId == "#4", "deal #4 is similar");
	}
}
