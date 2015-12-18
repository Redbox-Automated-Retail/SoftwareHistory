using System;

namespace Redbox.HAL.Core;

public interface IChannelResponse : IDisposable
{
	bool CommOk { get; }

	byte[] RawResponse { get; }

	string Error { get; }

	bool ReponseValid { get; }

	bool PortTimeout { get; }

	int GetIndex(byte b);
}
