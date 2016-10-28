using System.Runtime.InteropServices;

namespace Belikov.GenuineChannels.Logbook;

[ComVisible(true)]
public enum BinaryRecordVersion
{
	GeneralRecord = 256,
	ImplementationWarning,
	MessageRecord,
	SecuritySessionParameters,
	HostInformationInfo,
	ConnectionParametersRecord,
	TransportContentRecord,
	TransportBroadcastEngineRecord,
	VersionRecord
}
