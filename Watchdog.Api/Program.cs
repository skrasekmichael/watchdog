using FastEndpoints;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Watchdog.Infrastructure;
using Watchdog.SimilarDeals.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddFastEndpoints();
builder.Services.AddInfrastructure();
builder.Services.AddSimilarDealsModule();

var app = builder.Build();

var defaultCulture = new CultureInfo("en-US");
var localizationOptions = new RequestLocalizationOptions
{
	DefaultRequestCulture = new RequestCulture(defaultCulture),
	SupportedCultures = [defaultCulture],
	SupportedUICultures = [defaultCulture]
};

app.UseRequestLocalization(localizationOptions);

app.UseHttpsRedirection();
app.UseFastEndpoints();

app.Run();

public partial class Program;
