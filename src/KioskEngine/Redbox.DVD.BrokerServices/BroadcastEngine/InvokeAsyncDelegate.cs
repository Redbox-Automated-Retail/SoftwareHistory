using System.Runtime.Remoting.Messaging;

namespace Belikov.GenuineChannels.BroadcastEngine;

internal delegate IMessage InvokeAsyncDelegate(IMessage msg);
