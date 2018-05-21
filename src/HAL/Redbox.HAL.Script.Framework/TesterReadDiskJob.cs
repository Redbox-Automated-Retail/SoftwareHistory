using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "read-barcode", Operand = "READ-BARCODE")]
internal sealed class TesterReadDiskJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		if (!PickerBoard.IsFull())
		{
			base.Context.CreateInfoResult("NoDisk", "The picker is empty.");
			return;
		}
		using Scanner scanner = new Scanner(CenterDiskMethod.VendDoorAndBack, base.ControlSystem);
		ScanResult scanResult = scanner.Read();
		if (!scanResult.SnapOk)
		{
			base.Context.CreateCameraCaptureErrorResult();
			PushResults(default(TimeSpan), 0, "UNKNOWN");
			return;
		}
		PushResults(scanResult.ExecutionTime, scanResult.ReadCount, scanResult.ScannedMatrix);
		if (base.InventoryService.IsBarcodeDuplicate(scanResult.ScannedMatrix, out var _))
		{
			base.Context.CreateInfoResult("DuplicateDetected", "The read ID is a duplicate.", scanResult.ScannedMatrix);
		}
	}

	internal TesterReadDiskJob(ExecutionResult r, ExecutionContext c)
		: base(r, c)
	{
	}

	private void PushResults(TimeSpan ts, int readCodes, string id)
	{
		string message = $"{ts.Seconds}.{ts.Milliseconds} s";
		base.Context.CreateInfoResult("ReadTime", message);
		base.Context.CreateInfoResult("NumBarcodes", readCodes.ToString());
		base.Context.CreateResult("Read-ID", "Id read.", null, null, id, null, null);
	}
}
