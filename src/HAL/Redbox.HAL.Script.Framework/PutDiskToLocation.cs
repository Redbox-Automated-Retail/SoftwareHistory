using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "put-disk-to-location", Operand = "PUT-DISK-TO-LOCATION")]
internal sealed class PutDiskToLocation : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		int num = base.Context.PopTop<int>();
		int num2 = base.Context.PopTop<int>();
		using Scanner scanner = new Scanner();
		ErrorCodes errorCodes = base.MotionService.MoveTo(num2, num, MoveMode.Put, base.AppLog);
		if (errorCodes != ErrorCodes.Success)
		{
			base.Context.CreateInfoResult("JobError", $"MOVE DECK={num2} SLOT={num} MODE=PUT {errorCodes.ToString().ToUpper()}");
			return;
		}
		base.ControlSystem.TrackCycle();
		ScanResult scanResult = scanner.Read();
		IControllerService service = ServiceLocator.Instance.GetService<IControllerService>();
		IPutResult putResult = service.Put(scanResult.ScannedMatrix, base.AppLog);
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
