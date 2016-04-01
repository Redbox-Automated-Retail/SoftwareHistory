using System;

namespace Redbox.HAL.Controller.Framework;

[Flags]
public enum JobsWithScan
{
	Return = 1,
	FieldInsertFraudCheck = 2,
	Sync = 4,
	Vend = 8,
	Unload = 0x10,
	ReturnUnknown = 0x20,
	PutDiskInBin = 0x40,
	CheckPicker = 0x80,
	ReadFraudDisk = 0x100
}
