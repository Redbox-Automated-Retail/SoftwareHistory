using System;

namespace Belikov.GenuineChannels.BroadcastEngine;

internal class Court
{
	public MarshalByRefObject Receiver;

	public MarshalByRefObject Sender;

	public bool HasEverReceived = false;
}
