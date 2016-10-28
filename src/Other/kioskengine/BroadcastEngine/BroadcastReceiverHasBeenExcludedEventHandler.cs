using System;
using System.Runtime.InteropServices;

namespace Belikov.GenuineChannels.BroadcastEngine;

[ComVisible(true)]
public delegate void BroadcastReceiverHasBeenExcludedEventHandler(MarshalByRefObject marshalByRefObject, ReceiverInfo receiverInfo);
