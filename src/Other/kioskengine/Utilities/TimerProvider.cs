using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;

namespace Belikov.GenuineChannels.Utilities;

[ComVisible(true)]
public class TimerProvider
{
	public const int CallFrequencyInSeconds = 10;

	private static Timer _timer;

	private static ArrayList _clients;

	static TimerProvider()
	{
		_clients = new ArrayList(5);
		_timer = new Timer(TimerCallback, null, TimeSpan.FromSeconds(10.0), TimeSpan.FromSeconds(10.0));
	}

	public static void TimerCallback(object state)
	{
		lock (_clients)
		{
			int num = 0;
			while (num < _clients.Count)
			{
				ITimerConsumer timerConsumer = null;
				if (_clients[num] is WeakReference weakReference)
				{
					timerConsumer = weakReference.Target as ITimerConsumer;
				}
				if (timerConsumer == null)
				{
					_clients.RemoveAt(num);
					continue;
				}
				try
				{
					timerConsumer.TimerCallback();
				}
				catch (Exception)
				{
				}
				num++;
			}
		}
	}

	public static void Attach(ITimerConsumer iTimerConsumer)
	{
		lock (_clients)
		{
			_clients.Add(new WeakReference(iTimerConsumer));
		}
	}

	public static void Detach(ITimerConsumer iTimerConsumer)
	{
		lock (_clients)
		{
			_clients.Remove(new WeakReference(iTimerConsumer));
		}
	}
}
