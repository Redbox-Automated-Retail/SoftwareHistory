using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal class ReadFraudDiskJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "Fraud", flushOnWrite: false, "Fraud");
		applicationLog.Write("Read Fraud Disc Start.");
		if (ControllerService.Instance.MoveToVend(MoveMode.Get, applicationLog) != MotionResult.Success)
		{
			EndJob(errored: true);
			return;
		}
		if (base.VendDoor.Rent())
		{
			EndJob(errored: true);
			return;
		}
		using (new CameraHelper(500))
		{
			PickerStatus pickerStatus = ControllerService.Instance.TakeDiskAtDoor();
			if (pickerStatus != PickerStatus.DiscInPicker)
			{
				applicationLog.Write($"GRIPPER ACCEPT-DISK-AT-DOOR returned error status {pickerStatus.ToString()}", LogEntryType.Error);
				EndJob(errored: true);
				return;
			}
			if (base.VendDoor.Close())
			{
				applicationLog.Write("Disk is in picker; cannot close the vend door.", LogEntryType.Error);
				EndJob(errored: true);
				return;
			}
			ReadDiscStatus readDiscStatus = ReadDiskRaw(out var barcode);
			if (readDiscStatus != ReadDiscStatus.Success)
			{
				applicationLog.Write($"GRIPPER READ-DISC returned error status {readDiscStatus.ToString()}", LogEntryType.Error);
				EndJob(errored: true);
			}
			else if (barcode == "UNKNOWN" || barcode == "redbox")
			{
				EndJob(errored: true);
			}
			else
			{
				ResultHelper.Instance.CreateInfoResult("FraudItem", "This is the barcode to be marked as fraud.", barcode);
				EndJob(errored: false);
			}
		}
	}

	private ReadDiscStatus ReadDiskRaw(out string barcode)
	{
		ReadDiskResult readDiskResult = DataMatrixHelper.Instance.RawReadDiscInPicker(CenterDiskMethod.None);
		barcode = readDiskResult.Matrix;
		if (readDiskResult.ReadStatus != ReadDiscStatus.Success)
		{
			return readDiskResult.ReadStatus;
		}
		if (!readDiskResult.IsDiskUnknown())
		{
			return readDiskResult.ReadStatus;
		}
		readDiskResult = DataMatrixHelper.Instance.RawReadDiscInPicker(CenterDiskMethod.DrumAndBack);
		barcode = readDiskResult.Matrix;
		return readDiskResult.ReadStatus;
	}

	private void EndJob(bool errored)
	{
		if (errored)
		{
			ResultHelper.Instance.CreateInfoResult("HardwareError", "A hardware error occurred.", "UNKNOWN");
		}
		if (base.PickerBoard.IsFull())
		{
			VendItemResult vendItemResult = ControllerService.Instance.VendItem();
			if (vendItemResult.PickerStatus != PickerStatus.Empty)
			{
				base.AppLog.Write($"GRIPPER VEND-DISC-AT-DOOR returned error status {vendItemResult.ToString()}", LogEntryType.Error);
			}
		}
		base.VendDoor.Close();
		base.AppLog.Write("Read fraud disk end.");
	}
}
