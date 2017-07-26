using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "fraud-test-sell-relay")]
internal sealed class FraudTestSellRelayJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		bool flag = base.Context.PopTop<bool>();
		LogHelper.Instance.Log(" ** read sensors = {0}", flag);
		int num = base.Context.PopTop<int>();
		LogHelper.Instance.Log(" ** initial pause = {0}", num);
		int num2 = base.Context.PopTop<int>();
		LogHelper.Instance.Log(" ** iterations = {0}", num2);
		int num3 = base.Context.PopTop<int>();
		LogHelper.Instance.Log(" ** pause time = {0}", num3);
		if (flag)
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
		}
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		service.Wait(num);
		for (int i = 0; i < num2; i++)
		{
			if (i % 50 == 0 && !Post())
			{
				AddError("POST failure");
				break;
			}
			base.ControlSystem.Center(CenterDiskMethod.VendDoorAndBack);
			base.ControlSystem.SetSensors(on: true);
			service.Wait(50);
			IControlResponse controlResponse = base.ControlSystem.SetFinger(GripperFingerState.Rent);
			if (!controlResponse.Success)
			{
				AddError($"GripperSell returned error {controlResponse.Diagnostic}");
				break;
			}
			service.Wait(num3);
			base.ControlSystem.SetSensors(on: false);
			service.Wait(50);
			controlResponse = base.ControlSystem.SetFinger(GripperFingerState.Rent);
			if (!controlResponse.Success)
			{
				AddError($"GripperRent returned error {controlResponse.Diagnostic}");
				break;
			}
			service.Wait(num3);
			controlResponse = base.ControlSystem.SetFinger(GripperFingerState.Closed);
			if (!controlResponse.Success)
			{
				AddError($"GripperClose returned error {controlResponse.Diagnostic}");
				break;
			}
			service.Wait(num3);
			base.ControlSystem.TrackCycle();
			using FraudValidator fraudValidator = new FraudValidator(base.Context, startScan: true);
			using Scanner scanner = new Scanner();
			ScanResult scanResult = scanner.Read();
			if (!scanResult.SnapOk)
			{
				base.Context.CreateCameraCaptureErrorResult();
				continue;
			}
			if (!scanResult.ReadCode)
			{
				base.Context.CreateInfoResult("NoBarcodeRead", "The disk has no barcode.");
				continue;
			}
			IFraudScanResult fraudScanResult = fraudValidator.MultipassValidate(scanResult.ScannedMatrix);
			fraudValidator.Publish(fraudScanResult, scanResult.ScannedMatrix);
			if (fraudScanResult.ScanError != ErrorCodes.Success || !fraudScanResult.Valid)
			{
				AddError($"Validate returned error {fraudScanResult.ScanError} valid = {fraudScanResult.Valid}");
				break;
			}
		}
	}

	internal FraudTestSellRelayJob(ExecutionResult r, ExecutionContext c)
		: base(r, c)
	{
	}

	private bool Post()
	{
		IFormattedLogFactoryService service = ServiceLocator.Instance.GetService<IFormattedLogFactoryService>();
		IDiskValidatorService service2 = ServiceLocator.Instance.GetService<IDiskValidatorService>();
		bool result = true;
		ErrorCodes errorCodes = service2.FraudSensor.Reset(service.NilLog);
		if (errorCodes != ErrorCodes.Success)
		{
			base.Context.Send("POST failed.");
			base.Context.CreateInfoResult("POSTFailure", $"POST failed with error = {errorCodes}.");
			AddError("POST failure.");
			result = false;
		}
		else
		{
			base.Context.Send("POST end.");
			base.Context.CreateInfoResult("POSTSuccess", "The POST succeeded.");
		}
		return result;
	}
}
