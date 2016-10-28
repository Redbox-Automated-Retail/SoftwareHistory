using System;
using System.IO;
using System.Runtime.InteropServices;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.DirectExchange;

[ComVisible(true)]
public interface IStreamResponseHandler
{
	void HandleResponse(Stream response, HostInformation remoteHost, object tag);

	void HandleException(Exception exception, HostInformation remoteHost, object tag);
}
