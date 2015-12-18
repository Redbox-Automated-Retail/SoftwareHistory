using Redbox.HAL.Controller.Framework;
using Redbox.HAL.DataMatrix.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "put-disk-to-location", Operand = "PUT-DISK-TO-LOCATION")]
internal sealed class PutDiskToLocation : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		int top = GetTop<int>();
		int top2 = GetTop<int>();
		using Scanner scanner = new Scanner();
		MotionResult motionResult = ControllerService.Instance.MoveTo(top2, top, MoveMode.Put, base.AppLog);
		if (motionResult != MotionResult.Success)
		{
			base.Context.CreateInfoResult("JobError", $"MOVE DECK={top2} SLOT={top} MODE=PUT {motionResult.ToString().ToUpper()}");
			return;
		}
		ScanResult scanResult = scanner.Read();
		PutResult putResult = ControllerService.Instance.Put(scanResult.ScannedMatrix, base.AppLog);
		if (putResult.Success)
		{
			base.Context.CreateInfoResult("JobSuccess", $"PUT SUCCESS (barcode = {scanResult.ScannedMatrix})");
		}
		else
		{
			base.Context.CreateInfoResult("JobError", $"PUT {putResult.ToString()}");
		}
	}

	internal PutDiskToLocation(ExecutionResult r, ExecutionContext c)
		: base(r, c)
	{
	}
}
