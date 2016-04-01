using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "fraud-test-disk-in-picker", Operand = "FRAUD-TEST-DISK-IN-PICKER")]
internal sealed class FraudTestDiskInPickerJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		IPickerSensorReadResult pickerSensorReadResult = base.ControlSystem.ReadImmediateSensors();
		if (pickerSensorReadResult.Error != ErrorCodes.Success)
		{
			base.Context.CreateInfoResult("HardwareError", string.Format("Picker sensor read returned {0", pickerSensorReadResult.Error.ToString()));
			AddError("HardwareError");
			return;
		}
		if (!pickerSensorReadResult.IsFull)
		{
			base.Context.CreateInfoResult("PickerEmpty", "The picker is empty.");
			AddError("PickerEmpty");
			return;
		}
		IDiskValidatorService service = ServiceLocator.Instance.GetService<IDiskValidatorService>();
		using DiskValidatorHelper diskValidatorHelper = new DiskValidatorHelper(service, JobsWithScan.Return);
		PerfFunctions.Wait(500);
		using Scanner scanner = new Scanner();
		base.ControlSystem.Center(CenterDiskMethod.VendDoorAndBack);
		ScanResult scanResult = scanner.Read();
		if (!scanResult.SnapOk)
		{
			base.Context.CreateCameraCaptureErrorResult();
			return;
		}
		if (!scanResult.ReadCode)
		{
			base.Context.CreateInfoResult("NoBarcodeRead", "The disk has no barcode.");
			return;
		}
		IFraudScanResult scanResult2 = diskValidatorHelper.Validate(scanResult.ScannedMatrix);
		diskValidatorHelper.NotifyContext(scanResult2, scanResult.ScannedMatrix, base.Context, leaveResult: true);
	}

	internal FraudTestDiskInPickerJob(ExecutionResult r, ExecutionContext c)
		: base(r, c)
	{
	}
}
