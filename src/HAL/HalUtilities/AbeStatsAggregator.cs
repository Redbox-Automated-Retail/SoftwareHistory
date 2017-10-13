using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Redbox.HAL.Core;

namespace HALUtilities;

internal sealed class AbeStatsAggregator : IDisposable
{
	private readonly AbeFirmwareInfo[] DataSet;

	private readonly Thread[] Workers;

	private readonly string[] Files;

	private readonly Redbox.HAL.Core.CountdownEvent Countdown;

	private int m_currentFile = -1;

	public void Dispose()
	{
		Countdown.Dispose();
	}

	internal static void Aggregate(string path, int workers)
	{
		using ExecutionTimer executionTimer = new ExecutionTimer();
		AbeStatsAggregator t = new AbeStatsAggregator(workers, path);
		try
		{
			t.ReadData();
			AbeFirmwareInfo[] array = Array.FindAll(t.DataSet, (AbeFirmwareInfo each) => !each.UpdatedFirmware);
			List<AbeFirmwareInfo> finalList = new List<AbeFirmwareInfo>();
			Array.ForEach(array, delegate(AbeFirmwareInfo each)
			{
				AbeFirmwareInfo[] array2 = Array.FindAll(t.DataSet, (AbeFirmwareInfo item) => each.Kiosk == item.Kiosk && item.UpdatedFirmware);
				if ((array2 == null || array2.Length == 0) && !finalList.Contains(each))
				{
					finalList.Add(each);
				}
			});
			Console.WriteLine("{0} kiosks without the update firmware.", finalList.Count);
			finalList.ForEach(delegate(AbeFirmwareInfo each)
			{
				Console.WriteLine(" {0}", each.Kiosk);
			});
		}
		finally
		{
			if (t != null)
			{
				((IDisposable)t).Dispose();
			}
		}
		Console.WriteLine("[Aggregator] Execution time = {0}ms", executionTimer.ElapsedMilliseconds);
	}

	private AbeStatsAggregator(string path)
	{
		if (!Directory.Exists(path))
		{
			throw new ArgumentException("[Aggregator] Path '{0}' doesn't exist", path);
		}
		Files = Directory.GetFiles(path, "*.txt");
		DataSet = new AbeFirmwareInfo[Files.Length];
	}

	private AbeStatsAggregator(int workers, string path)
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
				DataSet[num] = new AbeFirmwareInfo(Files[num]);
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
			DataSet[num] = new AbeFirmwareInfo(file);
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
