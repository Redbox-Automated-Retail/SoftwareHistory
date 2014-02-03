using System.Runtime.Remoting.Messaging;

namespace Belikov.GenuineChannels.BroadcastEngine;

internal delegate void BroadcastCallFinishedHandler(Dispatcher dispatcher, IMessage message, ResultCollector resultCollector);
