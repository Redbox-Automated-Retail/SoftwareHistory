using System.IO;
using System.Runtime.InteropServices;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.DirectExchange;

[ComVisible(true)]
public interface IServerServiceEntry
{
	Stream HandleMessage(Stream stream, HostInformation sender);
}
