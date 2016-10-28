using System.Runtime.InteropServices;

namespace Belikov.GenuineChannels.DotNetRemotingLayer;

[ComVisible(true)]
public enum GenuineEventType
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
