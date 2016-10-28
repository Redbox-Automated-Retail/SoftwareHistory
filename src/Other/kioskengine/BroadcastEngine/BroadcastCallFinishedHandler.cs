using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;

namespace Belikov.GenuineChannels.BroadcastEngine;

[ComVisible(true)]
public delegate void BroadcastCallFinishedHandler(Dispatcher dispatcher, IMessage message, ResultCollector resultCollector);
