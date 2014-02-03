using System;
using Belikov.GenuineChannels.Messaging;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.Receiving;

internal interface IResponseProcessor : IDisposable
{
	HostInformation Remote { get; }

	bool IsShortInProcessing { get; }

	Message Message { get; }

	void ProcessRespose(Message message);

	void DispatchException(object exceptionAsObject);

	bool IsExpired(int now);
}
