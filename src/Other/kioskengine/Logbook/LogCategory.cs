using System.Runtime.InteropServices;

namespace Belikov.GenuineChannels.Logbook;

[ComVisible(true)]
public enum LogCategory
{
	Connection,
	ChannelEvent,
	MessageProcessing,
	ImplementationWarning,
	Security,
	BroadcastEngine,
	Transport,
	DXM,
	HostInformation,
	AcceptingConnection,
	Debugging,
	Version,
	LowLevelTransport,
	StatisticCounters,
	TotalCategories
}
