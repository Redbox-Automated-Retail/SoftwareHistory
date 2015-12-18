using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;
using Redbox.HAL.DataMatrix.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "put-disk-in-picker-in-bin", Operand = "PUT-DISK-IN-PICKER-IN-BIN")]
internal sealed class PutDiskInDumpBinJob : NativeJobAdapter
{
	private readonly CenterDiskMethod[] m_returnReadMethods = new CenterDiskMethod[2]
	{
		CenterDiskMethod.VendDoorAndBack,
		CenterDiskMethod.DrumAndBack
	};

	protected override void ExecuteInner()
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			base.Context.CreateInfoResult("PickerEmpty", "The picker is empty.");
			AddError("The picker is empty.");
			return;
		}
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "VMZ", flushOnWrite: true, "Tester-Put-In-Bin");
		if (!PickerBoard.IsFull())
		{
			base.Context.CreateInfoResult("PickerEmpty", "The picker is empty.");
			applicationLog.Write("The picker is empty.");
			AddError("The picker is empty.");
			return;
		}
		if (!DumpBin.Instance.Active)
		{
			base.Context.CreateInfoResult("Misconfiguration", "The dump bin is not configured.");
			applicationLog.Write("The dumpbin is not configured.");
			AddError("The bin is not configured.");
			return;
		}
		if (DumpBin.Instance.IsFull())
		{
			base.Context.CreateInfoResult("DumpBinFull", "The dump bin is full.");
			applicationLog.Write("The dumpbin is full.");
			AddError("The dumpbin is full.");
			return;
		}
		using Scanner scanner = new Scanner();
		Location location = DumpBin.Instance.MoveableLocation();
		MotionResult motionResult = ControllerService.Instance.MoveTo(location, MoveMode.Put, applicationLog);
		if (motionResult != MotionResult.Success)
		{
			base.Context.CreateInfoResult("MoveError", $"MOVE failed with error {motionResult.ToString()}");
			AddError("The MOVE instruction failed.");
			return;
		}
		string text = "UNKNOWN";
		CenterDiskMethod[] returnReadMethods = m_returnReadMethods;
		foreach (CenterDiskMethod method in returnReadMethods)
		{
			PerfFunctions.Wait(1000);
			ControllerService.Instance.CenterItem(method);
			ScanResult scanResult = scanner.Read();
			if (!scanResult.SnapOk)
			{
				base.Context.CreateCameraCaptureErrorResult();
				break;
			}
			if (scanResult.ReadCode)
			{
				text = scanResult.ScannedMatrix;
				break;
			}
		}
		PutResult putResult = ControllerService.Instance.Put(text, applicationLog);
		if (!putResult.Success)
		{
			base.Context.CreateInfoResult("PutError", $"PUT failed with error {putResult.ToString()}");
			AddError("The PUT instruction failed.");
			return;
		}
		base.Context.CreateInfoResult("PutToBinOK", "The disk was successfully filed", text);
		applicationLog.WriteFormatted("The barcode {0} was put into the dump bin.", text);
		DumpBin.Instance.DumpContents(base.AppLog);
	}

	internal PutDiskInDumpBinJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
