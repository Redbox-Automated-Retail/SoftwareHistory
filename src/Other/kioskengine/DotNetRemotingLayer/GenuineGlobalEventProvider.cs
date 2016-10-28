using System;
using System.Runtime.InteropServices;

namespace Belikov.GenuineChannels.DotNetRemotingLayer;

[ComVisible(true)]
public class GenuineGlobalEventProvider
{
	private static object _genuineChannelsGlobalEventObject = new object();

	public static event GenuineChannelsGlobalEventHandler GenuineChannelsGlobalEvent
	{
		add
		{
			lock (_genuineChannelsGlobalEventObject)
			{
				_genuineChannelsGlobalEvent = (GenuineChannelsGlobalEventHandler)Delegate.Combine(_genuineChannelsGlobalEvent, value);
			}
		}
		remove
		{
			lock (_genuineChannelsGlobalEventObject)
			{
				_genuineChannelsGlobalEvent = (GenuineChannelsGlobalEventHandler)Delegate.Remove(_genuineChannelsGlobalEvent, value);
			}
		}
	}

	private static event GenuineChannelsGlobalEventHandler _genuineChannelsGlobalEvent;

	public static void FireGlobalEvent(object sender, GenuineEventArgs e)
	{
		GenuineChannelsGlobalEventHandler genuineChannelsGlobalEventHandler = null;
		lock (_genuineChannelsGlobalEventObject)
		{
			if (_genuineChannelsGlobalEvent == null)
			{
				return;
			}
			genuineChannelsGlobalEventHandler = (GenuineChannelsGlobalEventHandler)_genuineChannelsGlobalEvent.Clone();
		}
		genuineChannelsGlobalEventHandler(sender, e);
	}
}
