using System.Collections.Generic;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal class GetBarcodesInBinJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		List<DumpBinEntry> contents = DumpBin.Instance.GetContents();
		foreach (DumpBinEntry item in contents)
		{
			ResultHelper.Instance.CreateInfoResult("DumpBinInventoryItem", "The item is in the dump bin.", item.ID);
		}
	}
}
