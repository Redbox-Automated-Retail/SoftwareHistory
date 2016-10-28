using System;
using System.Collections;
using System.Runtime.InteropServices;
using Belikov.GenuineChannels.Utilities;

namespace Belikov.GenuineChannels.BroadcastEngine;

[ComVisible(true)]
public class UniqueCallTracer : ITimerConsumer
{
	public static UniqueCallTracer Instance = new UniqueCallTracer();

	private static TimeSpan _timeSpanToRememberCall = TimeSpan.FromSeconds(180.0);

	private static int _timeSpanToRememberCallInMilliseconds = 180000;

	private static object _timeSpanToRememberCallLock = new object();

	private Hashtable _guids = new Hashtable();

	public static TimeSpan TimeSpanToRememberCall
	{
		get
		{
			lock (_timeSpanToRememberCallLock)
			{
				return _timeSpanToRememberCall;
			}
		}
		set
		{
			lock (_timeSpanToRememberCallLock)
			{
				_timeSpanToRememberCall = value;
				_timeSpanToRememberCallInMilliseconds = GenuineUtility.ConvertToMilliseconds(value);
			}
		}
	}

	private UniqueCallTracer()
	{
		TimerProvider.Attach(this);
	}

	public bool WasGuidRegistered(string guid)
	{
		lock (this)
		{
			if (_guids.Contains(guid))
			{
				return true;
			}
			_guids[guid] = GenuineUtility.GetTimeout(_timeSpanToRememberCallInMilliseconds);
			return false;
		}
	}

	public void TimerCallback()
	{
		lock (this)
		{
			int tickCount = GenuineUtility.TickCount;
			ArrayList arrayList = new ArrayList();
			foreach (object guid in _guids)
			{
				DictionaryEntry dictionaryEntry = (DictionaryEntry)guid;
				if (GenuineUtility.IsTimeoutExpired((int)dictionaryEntry.Value, tickCount))
				{
					arrayList.Add(dictionaryEntry.Key);
				}
			}
			foreach (object item in arrayList)
			{
				_guids.Remove(item);
			}
		}
	}
}
