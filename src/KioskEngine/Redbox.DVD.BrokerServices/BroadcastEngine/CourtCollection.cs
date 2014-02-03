using System;
using System.Collections;

namespace Belikov.GenuineChannels.BroadcastEngine;

internal class CourtCollection
{
	private static Hashtable _courts = new Hashtable();

	private CourtCollection()
	{
	}

	public static void Attach(string courtName, MarshalByRefObject receiver, MarshalByRefObject sender)
	{
		if (courtName == null || courtName.Length <= 0)
		{
			throw new ArgumentException("CourtName can not be null or contains an empty string.", "courtName", null);
		}
		if (receiver == null)
		{
			throw new ArgumentNullException("receiver");
		}
		lock (_courts)
		{
			Court court = null;
			if (!_courts.ContainsKey(courtName))
			{
				court = new Court();
				_courts[courtName] = court;
			}
			else
			{
				court = (Court)_courts[courtName];
			}
			court.Receiver = receiver;
			if (sender != null)
			{
				court.Sender = sender;
			}
		}
	}

	public static void Detach(string courtName)
	{
		if (courtName == null || courtName.Length <= 0)
		{
			throw new ArgumentException("CourtName must not be empty.", "courtName", null);
		}
		lock (_courts)
		{
			_courts.Remove(courtName);
		}
	}

	internal static void Find(string courtName, out MarshalByRefObject receiver, out MarshalByRefObject sender)
	{
		receiver = null;
		sender = null;
		lock (_courts)
		{
			if (_courts[courtName] is Court court)
			{
				court.HasEverReceived = true;
				receiver = court.Receiver;
				sender = court.Sender;
			}
		}
	}
}
