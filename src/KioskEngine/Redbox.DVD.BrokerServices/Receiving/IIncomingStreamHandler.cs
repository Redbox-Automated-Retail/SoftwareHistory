using System;
using System.IO;
using Belikov.GenuineChannels.Connection;
using Belikov.GenuineChannels.GenuineHttp;
using Belikov.GenuineChannels.Messaging;
using Belikov.GenuineChannels.Security;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.Receiving;

internal interface IIncomingStreamHandler
{
	bool HandleMessage(Stream stream, HostInformation remote, GenuineConnectionType genuineConnectionType, string connectionName, int dbgConnectionId, bool useThisThread, IMessageRegistrator iMessageRegistrator, SecuritySession connectionLevelSecuritySession, HttpServerRequestResult httpServerRequestResult);

	void HandleMessage_AfterCLSS(Message message);

	void HandleMessage_Final(Message message);

	void DispatchException(HostInformation hostInformation, Exception exception);

	void DispatchException(Message sourceMessage, Exception exception);

	void RegisterResponseProcessor(int replyId, IResponseProcessor iResponseProcessor);

	bool IsHandlerAlive(int replyId);

	void RegisterTransportUser(string name, ITransportUser transportUser);
}
