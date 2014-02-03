namespace Belikov.GenuineChannels.GenuineHttp;

internal enum HttpPacketType
{
	Usual,
	Listening,
	Establishing_ResetConnection,
	Establishing,
	DoubleRequests,
	RequestRepeated,
	Desynchronization,
	SenderResponse,
	ListenerTimedOut,
	ClosedManually,
	SenderError,
	Unkown
}
