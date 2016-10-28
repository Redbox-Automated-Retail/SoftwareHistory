using System.Runtime.InteropServices;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.DirectExchange;

[ComVisible(true)]
public delegate void StreamResponseEventHandler(object response, HostInformation remoteHost, object tag);
