using System.Collections.ObjectModel;

namespace Redbox.HAL.DataStorage;

internal sealed class CountersStore : StoreDescriptor
{
	private ReadOnlyCollection<string> ReadonlyTableList;

	internal override string Store => "HALCounters.vdb3";

	internal override ReadOnlyCollection<string> Tables => ReadonlyTableList;

	internal CountersStore()
	{
		ReadonlyTableList = new ReadOnlyCollection<string>(new string[5] { "BarcodesInBin", "CameraData", "SecretOptions_v2", "Counters", "HardwareCorrection" });
	}
}
