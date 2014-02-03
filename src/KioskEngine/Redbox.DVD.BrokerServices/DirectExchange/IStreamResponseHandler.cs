using System;
using System.IO;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.DirectExchange;

internal interface IStreamResponseHandler
{
	void HandleResponse(Stream response, HostInformation remoteHost, object tag);

	void HandleException(Exception exception, HostInformation remoteHost, object tag);
}
