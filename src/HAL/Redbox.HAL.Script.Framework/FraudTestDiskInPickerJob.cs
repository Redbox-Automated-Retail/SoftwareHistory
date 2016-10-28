using Redbox.HAL.Component.Model;

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
		using FraudValidator fraudValidator = new FraudValidator(base.Context, startScan: true);
		base.ControlSystem.Center(CenterDiskMethod.VendDoorAndBack);
		using Scanner scanner = new Scanner();
		ScanResult scanResult = scanner.Read();
		if (!scanResult.SnapOk)
		{
			base.Context.CreateCameraCaptureErrorResult();
		}
		else if (!scanResult.ReadCode)
		{
			base.Context.CreateInfoResult("NoBarcodeRead", "The disk has no barcode.");
		}
		else
		{
			fraudValidator.ValidateAndPublish(scanResult.ScannedMatrix);
		}
	}

	internal FraudTestDiskInPickerJob(ExecutionResult r, ExecutionContext c)
		: base(r, c)
	{
	}
}
