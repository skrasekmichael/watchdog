namespace Watchdog.Common.Abstractions;

public interface IAnalysisService<TData> where TData : IAnalysisData
{
	public Task AnalyzeAsync(TData data, CancellationToken ct);
}
