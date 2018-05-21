using System;
using System.Threading;
using Redbox.HAL.Core;

namespace HALUtilities;

internal sealed class AsyncHardwareStatsAggregator : AbstractHardwareStatsAggregator
{
	private readonly Thread[] Workers;

	private readonly Redbox.HAL.Core.CountdownEvent Countdown;

	private int m_currentFile = -1;

	protected override void OnDispose(bool disposing)
	{
		Countdown.Dispose();
	}

	protected override void OnReadData()
	{
		Array.ForEach(Workers, delegate(Thread thread)
		{
			thread.Start();
		});
		Countdown.Wait();
	}

	internal AsyncHardwareStatsAggregator(int workers, string path)
		: base(path)
	{
		if (workers <= 0)
		{
			throw new ArgumentException("workers must be greater than zero");
		}
		Workers = new Thread[workers];
		for (int i = 0; i < workers; i++)
		{
			Workers[i] = new Thread(Consume);
		}
		Countdown = new Redbox.HAL.Core.CountdownEvent(workers);
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
}
