using System;
using System.Runtime.InteropServices;

namespace Belikov.GenuineChannels.BroadcastEngine;

[ComVisible(true)]
public class Court
{
	public MarshalByRefObject Receiver;

	public MarshalByRefObject Sender;

	public bool HasEverReceived = false;
}
