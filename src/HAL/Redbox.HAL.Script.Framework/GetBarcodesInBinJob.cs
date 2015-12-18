using System.Collections.Generic;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "get-barcodes-in-bin", Operand = "GET-BARCODES-IN-BIN")]
internal sealed class GetBarcodesInBinJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		List<DumpBinEntry> contents = DumpBin.Instance.GetContents();
		foreach (DumpBinEntry item in contents)
		{
			base.Context.CreateInfoResult("DumpBinInventoryItem", "The item is in the dump bin.", item.ID);
		}
	}

	internal GetBarcodesInBinJob(ExecutionResult result, ExecutionContext context)
		: base(result, context)
	{
	}
}
