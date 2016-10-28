using System.Collections.Generic;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "get-barcodes-in-bin", Operand = "GET-BARCODES-IN-BIN")]
internal sealed class GetBarcodesInBinJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		IDumpbinService service = ServiceLocator.Instance.GetService<IDumpbinService>();
		IList<IDumpBinInventoryItem> barcodesInBin = service.GetBarcodesInBin();
		using (new DisposeableList<IDumpBinInventoryItem>(barcodesInBin))
		{
			foreach (IDumpBinInventoryItem item in barcodesInBin)
			{
				base.Context.CreateInfoResult("DumpBinInventoryItem", "The item is in the dump bin.", item.ID);
			}
		}
	}

	internal GetBarcodesInBinJob(ExecutionResult result, ExecutionContext context)
		: base(result, context)
	{
	}
}
