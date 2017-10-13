using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Redbox.HAL.Core;

namespace HALUtilities;

internal sealed class HardwareStatsAggregator : IDisposable
{
	private readonly KioskCorrectionInfo[] DataSet;

	private readonly Thread[] Workers;

	private readonly string[] Files;

	private readonly Redbox.HAL.Core.CountdownEvent Countdown;

	private int m_currentFile = -1;

	public void Dispose()
	{
		Countdown.Dispose();
	}

	internal static void Aggregate(string path, int workers, List<StatsAggregatorFilter> Filters)
	{
		using ExecutionTimer executionTimer = new ExecutionTimer();
		using (HardwareStatsAggregator hardwareStatsAggregator = new HardwareStatsAggregator(workers, path))
		{
			hardwareStatsAggregator.ReadData();
			hardwareStatsAggregator.Aggregate(Filters);
		}
		Console.WriteLine("[Aggregator] Execution time = {0}ms", executionTimer.ElapsedMilliseconds);
	}

	private HardwareStatsAggregator(string path)
	{
		if (!Directory.Exists(path))
		{
			throw new ArgumentException("[Aggregator] Path '{0}' doesn't exist", path);
		}
		Files = Directory.GetFiles(path, "*.txt");
		DataSet = new KioskCorrectionInfo[Files.Length];
	}

	private HardwareStatsAggregator(int workers, string path)
		: this(path)
	{
		if (workers <= 0)
		{
			throw new ArgumentException("Need at least one worker");
		}
		Workers = new Thread[workers];
		for (int i = 0; i < workers; i++)
		{
			Workers[i] = new Thread(Consume);
		}
		Countdown = new Redbox.HAL.Core.CountdownEvent(workers);
	}

	private void Aggregate(List<StatsAggregatorFilter> filters)
	{
		foreach (StatsAggregatorFilter filter in filters)
		{
			filter.Apply(DataSet);
		}
	}

	private void ReadData()
	{
		if (Workers != null)
		{
			Array.ForEach(Workers, delegate(Thread thread)
			{
				thread.Start();
			});
			Countdown.Wait();
		}
		else
		{
			for (int num = 0; num < Files.Length; num++)
			{
				DataSet[num] = new KioskCorrectionInfo(Files[num]);
			}
		}
	}

	private void Consume()
	{
		while (true)
		{
			int num = Interlocked.Increment(ref m_currentFile);
			if (num >= Files.Length)
			{
				break;
			}
			string file = Files[num];
			DataSet[num] = new KioskCorrectionInfo(file);
		}
		Countdown.Signal();
	}

	private void Shutdown(bool waitForWorkers)
	{
		if (waitForWorkers)
		{
			Array.ForEach(Workers, delegate(Thread j)
			{
				j.Join();
			});
		}
	}
}
