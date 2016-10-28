using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

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
		IDumpbinService service = ServiceLocator.Instance.GetService<IDumpbinService>();
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
		if (!ControllerConfiguration.Instance.IsVMZMachine)
		{
			base.Context.CreateInfoResult("Misconfiguration", "The dump bin is not configured.");
			applicationLog.Write("The dumpbin is not configured.");
			AddError("The bin is not configured.");
			return;
		}
		if (service.IsFull())
		{
			base.Context.CreateInfoResult("DumpBinFull", "The dump bin is full.");
			applicationLog.Write("The dumpbin is full.");
			AddError("The dumpbin is full.");
			return;
		}
		using FraudValidator fraudValidator = new FraudValidator(base.Context, FraudScanJobMask.PutDiskInBin);
		fraudValidator.StartScan();
		IMotionControlService service2 = ServiceLocator.Instance.GetService<IMotionControlService>();
		using Scanner scanner = new Scanner();
		ErrorCodes errorCodes = service2.MoveTo(service.PutLocation, MoveMode.Put, applicationLog);
		if (errorCodes != ErrorCodes.Success)
		{
			base.Context.CreateInfoResult("MoveError", $"MOVE failed with error {errorCodes.ToString()}");
			AddError("The MOVE instruction failed.");
			return;
		}
		string text = "UNKNOWN";
		CenterDiskMethod[] returnReadMethods = m_returnReadMethods;
		foreach (CenterDiskMethod method in returnReadMethods)
		{
			PerfFunctions.Wait(1000);
			base.ControlSystem.Center(method);
			ScanResult scanResult = scanner.Read();
			if (!scanResult.SnapOk)
			{
				base.Context.CreateCameraCaptureErrorResult();
				break;
			}
			if (scanResult.ReadCode)
			{
				text = scanResult.ScannedMatrix;
				fraudValidator.ValidateAndPublish(text);
				break;
			}
		}
		IControllerService service3 = ServiceLocator.Instance.GetService<IControllerService>();
		IPutResult putResult = service3.Put(text, applicationLog);
		if (!putResult.Success)
		{
			base.Context.CreateInfoResult("PutError", $"PUT failed with error {putResult.ToString()}");
			AddError("The PUT instruction failed.");
			return;
		}
		base.Context.CreateInfoResult("PutToBinOK", "The disk was successfully filed", text);
		applicationLog.WriteFormatted("The barcode {0} was put into the dump bin.", text);
		service.DumpContents(base.AppLog);
	}

	internal PutDiskInDumpBinJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
