namespace Belikov.GenuineChannels.Logbook;

internal enum BinaryRecordVersion
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
