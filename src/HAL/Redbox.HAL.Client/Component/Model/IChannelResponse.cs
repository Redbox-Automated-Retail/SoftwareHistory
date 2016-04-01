using System;

namespace Redbox.HAL.Component.Model;

internal interface IChannelResponse : IDisposable
{
	bool CommOk { get; }

	ErrorCodes Error { get; }

	byte[] RawResponse { get; }

	bool ReponseValid { get; }

	int GetIndex(byte b);
}
