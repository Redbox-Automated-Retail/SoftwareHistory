using System.IO;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.DirectExchange;

internal interface IServerServiceEntry
{
	Stream HandleMessage(Stream stream, HostInformation sender);
}
