using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;

namespace Belikov.GenuineChannels.BroadcastEngine;

[ComVisible(true)]
public delegate IMessage InvokeAsyncDelegate(IMessage msg);
