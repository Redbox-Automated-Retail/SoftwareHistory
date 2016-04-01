using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "fraud-sensor-stress-test", Operand = "FRAUD-SENSOR-STRESS-TEST", HideFromList = true)]
internal sealed class FraudSensorStressJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: false, "Fraud", flushOnWrite: true, null);
		IPickerSensorReadResult pickerSensorReadResult = base.ControlSystem.ReadSensors();
		if (!pickerSensorReadResult.IsFull)
		{
			applicationLog.Write("There is no disk in the picker.");
			base.Context.CreateInfoResult("PickerEmpty", "This test needs a disk in the picker.");
			AddError("No disk!");
			return;
		}
		int top = GetTop<int>();
		int num = 0;
		int num2 = 0;
		int top2 = GetTop<int>();
		int top3 = GetTop<int>();
		applicationLog.Write($"Args: iter = {top} readPause = {top2}ms iterPause = {top3}ms");
		IDiskValidatorService service = ServiceLocator.Instance.GetService<IDiskValidatorService>();
		_ = service.FraudSensor;
		base.ControlSystem.ToggleRingLight(on: true, 100);
		for (int i = 1; i <= top; i++)
		{
			using (DiskValidatorHelper diskValidatorHelper = new DiskValidatorHelper(service, JobsWithScan.Return))
			{
				using (VendItemOperation vendItemOperation = new VendItemOperation(1))
				{
					VendItemResult vendItemResult = vendItemOperation.Execute();
					if (!vendItemResult.Presented)
					{
						base.Context.AppLog.Write("The disk was not presented to the user from the vend door.");
						AddError("Job error.");
						return;
					}
					if (vendItemResult.Status == ErrorCodes.PickerEmpty)
					{
						base.Context.AppLog.Write("The disk is no longer present in the picker. This is not good.");
						AddError("Job error.");
						return;
					}
				}
				using (AcceptDiskOperation acceptDiskOperation = new AcceptDiskOperation())
				{
					ErrorCodes errorCodes = acceptDiskOperation.Execute();
					if (ErrorCodes.PickerFull != errorCodes)
					{
						base.Context.AppLog.Write("There was a problem taking the disk in.");
						AddError("Job error.");
						return;
					}
				}
				Thread.Sleep(top2);
				IFraudScanResult fraudScanResult = diskValidatorHelper.Validate();
				base.Context.AppLog.WriteFormatted("Fraud scan returned {0} at iteration {1}", fraudScanResult.Valid ? "OK" : "NO MARKERS", i);
				if (fraudScanResult.Valid)
				{
					num2++;
					base.Context.Send("Read OK");
				}
				else
				{
					num++;
					base.Context.Send("Read Failure");
				}
			}
			Thread.Sleep(top3);
		}
		base.ControlSystem.ToggleRingLight(on: false, 100);
		base.Context.CreateInfoResult("FailedReads", num.ToString());
		base.Context.CreateInfoResult("SuccessReads", num2.ToString());
		if (num > 0)
		{
			AddError("MARKER READ FAILURES");
		}
	}

	internal FraudSensorStressJob(ExecutionResult result, ExecutionContext context)
		: base(result, context)
	{
	}
}
