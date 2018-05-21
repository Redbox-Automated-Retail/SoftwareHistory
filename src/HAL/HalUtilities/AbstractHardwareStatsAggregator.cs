using System;
using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Core;

namespace HALUtilities;

internal abstract class AbstractHardwareStatsAggregator : IDisposable
{
	protected readonly KioskCorrectionInfo[] DataSet;

	protected readonly string[] Files;

	private bool Disposed;

	public void Dispose()
	{
		if (!Disposed)
		{
			Disposed = true;
			OnDispose(disposing: true);
		}
	}

	protected virtual void OnDispose(bool disposing)
	{
	}

	protected abstract void OnReadData();

	protected void Aggregate(List<StatsAggregatorFilter> filters)
	{
		foreach (StatsAggregatorFilter filter in filters)
		{
			filter.Apply(DataSet);
		}
	}

	protected AbstractHardwareStatsAggregator(string path)
	{
		if (!Directory.Exists(path))
		{
			throw new ArgumentException($"[{GetType().Name}] Path '{path}' doesn't exist");
		}
		Files = Directory.GetFiles(path, "*.txt");
		DataSet = new KioskCorrectionInfo[Files.Length];
	}

	internal static void Aggregate(string path, List<StatsAggregatorFilter> Filters)
	{
		Aggregate(new SyncHardwareStatsAggregator(path), Filters);
	}

	internal static void Aggregate(int workers, string path, List<StatsAggregatorFilter> Filters)
	{
		Aggregate(new AsyncHardwareStatsAggregator(workers, path), Filters);
	}

	private static void Aggregate(AbstractHardwareStatsAggregator agg, List<StatsAggregatorFilter> filters)
	{
		using ExecutionTimer executionTimer = new ExecutionTimer();
		using (agg)
		{
			agg.OnReadData();
			agg.Aggregate(filters);
		}
		Console.WriteLine("[{0}] Execution time = {1}ms", agg.GetType().Name, executionTimer.ElapsedMilliseconds);
	}
}
