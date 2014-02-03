namespace Belikov.GenuineChannels.DotNetRemotingLayer;

internal enum GenuineEventType
{
	GeneralConnectionIndeterminate,
	GeneralConnectionEstablished,
	GeneralServerRestartDetected,
	GeneralNewSessionDetected,
	GeneralConnectionReestablishing,
	GeneralConnectionClosed,
	GeneralListenerStarted,
	GeneralListenerShutDown,
	GeneralListenerFailure,
	GTcpConnectionAccepted,
	GHttpConnectionAccepted,
	GUdpSocketException,
	BroadcastUnknownCourtReceived,
	UnknownServerService,
	SecuritySessionWasNotFound,
	SecuritySessionFailed,
	HostResourcesReleased
}
