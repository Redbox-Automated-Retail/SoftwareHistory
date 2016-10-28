using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "read-fraud-disc", Operand = "READ-FRAUD-DISK")]
internal sealed class ReadFraudDiskJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "Fraud", flushOnWrite: false, "Fraud");
		applicationLog.Write("Read Fraud Disc Start.");
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
		bool errored = true;
		using Scanner scanner = new Scanner();
		for (int i = 0; i < 2; i++)
		{
			ScanResult scanResult = scanner.Read();
			if (!scanResult.SnapOk)
			{
				break;
			}
			if (scanResult.ReadCode)
			{
				base.Context.CreateInfoResult("FraudItem", "This is the barcode to be marked as fraud.", scanResult.ScannedMatrix);
				errored = false;
				break;
			}
			base.ControlSystem.Center(CenterDiskMethod.DrumAndBack);
		}
		EndJob(errored);
	}

	internal ReadFraudDiskJob(ExecutionResult r, ExecutionContext c)
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
		base.AppLog.Write("Read fraud disk end.");
	}
}
