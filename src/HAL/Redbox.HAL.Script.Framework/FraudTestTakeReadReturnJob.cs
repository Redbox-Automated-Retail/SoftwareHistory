using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "fraud-test-take-read-return", Operand = "FRAUD-TEST-TAKE-READ-RETURN")]
internal sealed class FraudTestTakeReadReturnJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "Fraud", flushOnWrite: false, "Fraud-take-read-return");
		applicationLog.Write("Take/Test/Return Start.");
		IDiskValidatorService service = ServiceLocator.Instance.GetService<IDiskValidatorService>();
		using DiskValidatorHelper diskValidatorHelper = new DiskValidatorHelper(service, JobsWithScan.Return);
		if (CorrectiveMoveVend(MoveMode.Get) != ErrorCodes.Success)
		{
			EndJob(errored: true);
			return;
		}
		if (!base.ControlSystem.VendDoorRent().Success)
		{
			EndJob(errored: true);
			return;
		}
		using (AcceptDiskOperation acceptDiskOperation = new AcceptDiskOperation())
		{
			ErrorCodes errorCodes = acceptDiskOperation.Execute();
			if (ErrorCodes.PickerFull != errorCodes)
			{
				applicationLog.Write($"GRIPPER ACCEPT-DISK-AT-DOOR returned error status {errorCodes.ToString()}", LogEntryType.Error);
				EndJob(errored: true);
				return;
			}
		}
		if (!base.ControlSystem.VendDoorClose().Success)
		{
			applicationLog.Write("Disk is in picker; cannot close the vend door.", LogEntryType.Error);
			EndJob(errored: true);
			return;
		}
		base.ControlSystem.TrackCycle();
		using Scanner scanner = new Scanner();
		for (int i = 0; i < 2; i++)
		{
			ScanResult scanResult = scanner.Read();
			if (!scanResult.SnapOk)
			{
				EndJob(errored: true);
				return;
			}
			if (!scanResult.ReadCode)
			{
				if (i != 0)
				{
					break;
				}
				base.ControlSystem.Center(CenterDiskMethod.DrumAndBack);
				continue;
			}
			IFraudScanResult scanResult2 = diskValidatorHelper.Validate(scanResult.ScannedMatrix);
			diskValidatorHelper.NotifyContext(scanResult2, scanResult.ScannedMatrix, base.Context, leaveResult: true);
			EndJob(errored: false);
			return;
		}
		EndJob(errored: true);
	}

	internal FraudTestTakeReadReturnJob(ExecutionResult r, ExecutionContext c)
		: base(r, c)
	{
	}

	private void EndJob(bool errored)
	{
		if (errored)
		{
			base.Context.CreateInfoResult("HardwareError", "A hardware error occurred.", "UNKNOWN");
		}
		if (PickerBoard.IsFull())
		{
			using VendItemOperation vendItemOperation = new VendItemOperation();
			VendItemResult vendItemResult = vendItemOperation.Execute();
			if (ErrorCodes.PickerEmpty != vendItemResult.Status)
			{
				base.AppLog.Write($"GRIPPER VEND-DISC-AT-DOOR returned error status {vendItemResult.ToString()}", LogEntryType.Error);
			}
		}
		base.ControlSystem.VendDoorClose();
	}
}
