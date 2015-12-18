using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;
using Redbox.HAL.DataMatrix.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "read-fraud-disc", Operand = "READ-FRAUD-DISK")]
internal sealed class ReadFraudDiskJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "Fraud", flushOnWrite: false, "Fraud");
		applicationLog.Write("Read Fraud Disc Start.");
		if (CorrectiveMoveVend(MoveMode.Get) != MotionResult.Success)
		{
			EndJob(errored: true);
			return;
		}
		if (VendDoor.Rent())
		{
			EndJob(errored: true);
			return;
		}
		PickerStatus pickerStatus = ControllerService.Instance.TakeDiskAtDoor();
		if (pickerStatus != PickerStatus.DiscInPicker)
		{
			applicationLog.Write($"GRIPPER ACCEPT-DISK-AT-DOOR returned error status {pickerStatus.ToString()}", LogEntryType.Error);
			EndJob(errored: true);
			return;
		}
		if (VendDoor.Close())
		{
			applicationLog.Write("Disk is in picker; cannot close the vend door.", LogEntryType.Error);
			EndJob(errored: true);
			return;
		}
		using Scanner scanner = new Scanner();
		for (int i = 0; i < 2; i++)
		{
			RawReadResult rawReadResult = scanner.RawRead();
			if (!rawReadResult.SnapOk)
			{
				EndJob(errored: true);
				return;
			}
			if (rawReadResult.ReadCodes[0] == "UNKNOWN")
			{
				ControllerService.Instance.CenterItem(CenterDiskMethod.DrumAndBack);
				continue;
			}
			base.Context.CreateInfoResult("FraudItem", "This is the barcode to be marked as fraud.", rawReadResult.ReadCodes[0]);
			EndJob(errored: false);
			return;
		}
		EndJob(errored: true);
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
			VendItemResult vendItemResult = ControllerService.Instance.VendItem();
			if (vendItemResult.PickerStatus != PickerStatus.Empty)
			{
				base.AppLog.Write($"GRIPPER VEND-DISC-AT-DOOR returned error status {vendItemResult.ToString()}", LogEntryType.Error);
			}
		}
		VendDoor.Close();
		base.AppLog.Write("Read fraud disk end.");
	}
}
