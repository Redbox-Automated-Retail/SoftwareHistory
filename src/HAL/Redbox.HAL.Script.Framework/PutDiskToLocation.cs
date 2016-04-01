using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "put-disk-to-location", Operand = "PUT-DISK-TO-LOCATION")]
internal sealed class PutDiskToLocation : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		int top = GetTop<int>();
		int top2 = GetTop<int>();
		using Scanner scanner = new Scanner();
		ErrorCodes errorCodes = base.MotionService.MoveTo(top2, top, MoveMode.Put, base.AppLog);
		if (errorCodes != ErrorCodes.Success)
		{
			base.Context.CreateInfoResult("JobError", $"MOVE DECK={top2} SLOT={top} MODE=PUT {errorCodes.ToString().ToUpper()}");
			return;
		}
		base.ControlSystem.TrackCycle();
		ScanResult scanResult = scanner.Read();
		IPutResult putResult = ControllerService.Instance.Put(scanResult.ScannedMatrix, base.AppLog);
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
