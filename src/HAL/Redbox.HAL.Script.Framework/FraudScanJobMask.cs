using System;

namespace Redbox.HAL.Script.Framework;

[Flags]
internal enum FraudScanJobMask
{
	Sync = 1,
	Unload = 2,
	ReturnUnknown = 4,
	PutDiskInBin = 8,
	CheckPicker = 0x10,
	SyncLocations = 0x20
}
