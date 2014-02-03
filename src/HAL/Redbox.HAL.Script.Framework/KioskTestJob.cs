using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal class KioskTestJob : NativeJobAdapter
{
	private QLM QLM => QLM.Instance;

	protected override void ExecuteInner()
	{
		Deck deck = QLM.Deck;
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "KioskTest", flushOnWrite: false, "KioskTestErrors");
		bool top = GetTop<bool>();
		int top2 = GetTop<int>();
		int top3 = GetTop<int>();
		int top4 = GetTop<int>();
		int top5 = GetTop<int>();
		bool flag = ControllerConfiguration.Instance.MerchandizingMode == MerchandizingMode.QLM && deck != null && deck.Number == top2;
		bool flag2 = ControllerConfiguration.Instance.MerchandizingMode == MerchandizingMode.QLM && deck != null && deck.Number == top4;
		if (flag && !EngageQlm())
		{
			return;
		}
		if (MoveTo(top2, top3, MoveMode.Get) != MotionResult.Success)
		{
			ResultHelper.Instance.CreateInfoResult("MoveError", "Failed to move to initial slot");
			HandleMachineError();
			return;
		}
		using (new CameraHelper(500))
		{
			GetResult getResult = ControllerService.Instance.Get(testOnEmpty: false, base.AppLog);
			if (!getResult.Success)
			{
				ResultHelper.Instance.CreateInfoResult("GetError", "Failed to pull disc from slot");
				HandleMachineError();
				return;
			}
			ReadDiskResult readDiskResult = DataMatrixHelper.Instance.ReadDiscInPicker(CenterDiskMethod.VendDoorAndBack);
			string matrix = readDiskResult.Matrix;
			if (readDiskResult.ReadStatus != ReadDiscStatus.Success)
			{
				applicationLog.Write($"CAMERA CAPTURE returned an error status {readDiskResult.ToString().ToUpper()}", LogEntryType.Error);
			}
			if (readDiskResult.IsDiskUnknown())
			{
				applicationLog.Write("The barcode in the picker could not be read.", LogEntryType.Error);
			}
			if (top && !VendDisk(matrix))
			{
				HandleMachineError();
			}
			else
			{
				if (flag2 && !EngageQlm())
				{
					return;
				}
				if (MoveTo(top4, top5, MoveMode.Put) != MotionResult.Success)
				{
					ResultHelper.Instance.CreateInfoResult("MoveFailed", "Kiosk was unable to move to put position of destination slot");
					HandleMachineError();
					return;
				}
				PutResult putResult = ControllerService.Instance.Put(matrix, base.AppLog);
				if (!putResult.Success)
				{
					ResultHelper.Instance.CreateInfoResult("PutFailed", "Kiosk failed to put disc into slot");
					HandleMachineError();
					return;
				}
				PerfFunctions.Wait(1200);
				if (MoveTo(top4, top5, MoveMode.Get) != MotionResult.Success)
				{
					ResultHelper.Instance.CreateInfoResult("MoveFailed", "Kiosk was unable to move to get position of destination slot");
					HandleMachineError();
					return;
				}
				if (ControllerConfiguration.Instance.MerchandizingMode == MerchandizingMode.QLM)
				{
					QlmStatus status = QLM.GetStatus();
					if (QlmStatus.AuxNotResponsive == status)
					{
						applicationLog.Write("The AUX board is not responsive.");
						HandleMachineError();
						return;
					}
					if (QlmStatus.Empty == status)
					{
						applicationLog.Write("There is no QLM in the bay.", LogEntryType.Error);
						HandleMachineError();
						return;
					}
				}
				if (flag && !flag2)
				{
					applicationLog.Write("Disengage the QLM.");
					QlmMotionResult qlmMotionResult = QLM.Disengage(home: true, applicationLog);
					if (QlmMotionResult.Success != qlmMotionResult)
					{
						ResultHelper.Instance.CreateInfoResult("QlmDeckDisengageFailure", "The QLM disengage failed.");
						AddError("The QLM Case didn't disengage.");
						return;
					}
					ResultHelper.Instance.CreateInfoResult("QlmDeckDisengageSuccess", "The QLM Disengaged successfully.");
				}
				ResultHelper.Instance.CreateInfoResult("Success", "SUCCESS");
			}
		}
	}

	private void HandleMachineError()
	{
		base.Context.AppLog.Write("An unrecoverable error occurred in KioskTest.", LogEntryType.Error);
		ResultHelper.Instance.CreateInfoResult("MachineError", "An unrecoverable error occurred in KioskTest");
		AddError("MACHINE ERROR");
	}

	private bool EngageQlm()
	{
		QlmStatus status = QLM.GetStatus();
		if (QlmStatus.AuxNotResponsive == status)
		{
			base.Context.AppLog.Write("QLM Status: the AUX board is not responsive.");
			AddError("AUX not responsive.");
			return false;
		}
		if (QlmStatus.Engaged == status)
		{
			return true;
		}
		if (QlmStatus.Empty == status)
		{
			base.Context.AppLog.Write("There is no QLM in the bay.", LogEntryType.Error);
			AddError("NO QLM IN BAY.");
			return false;
		}
		base.Context.AppLog.Write("Engaging the QLM.", LogEntryType.Info);
		QlmMotionResult qlmMotionResult = QLM.Engage(home: true, base.Context.AppLog);
		if (QlmMotionResult.Success == qlmMotionResult)
		{
			return true;
		}
		ResultHelper.Instance.CreateInfoResult("MachineError", "QLM Engage timeout.");
		AddError("QLM Engage Timeout");
		return false;
	}

	private bool VendDisk(string matrix)
	{
		ApplicationLog appLog = base.Context.AppLog;
		appLog.Write("Performing a Vend Test.");
		if (MoveVend(MoveMode.None) != MotionResult.Success)
		{
			ResultHelper.Instance.CreateInfoResult("MoveVendError", "Failed to move to vend position");
			return false;
		}
		if (base.Track.Open())
		{
			appLog.Write("Track Open timed out.", LogEntryType.Error);
			ResultHelper.Instance.CreateInfoResult("TrackOpenTimeOut", "Track Open timedout.");
			return false;
		}
		if (base.Track.Close())
		{
			appLog.Write("Track Close timed out.", LogEntryType.Error);
			ResultHelper.Instance.CreateInfoResult("TrackCloseTimeOut", "Track Close timedout.");
			return false;
		}
		if (base.VendDoor.Rent())
		{
			appLog.Write("Vend door timed out trying to open vend door.", LogEntryType.Error);
			ResultHelper.Instance.CreateInfoResult("VendDoorTimeOut", "Vend door timed out trying to open vend door.");
			return false;
		}
		VendItemResult vendItemResult = ControllerService.Instance.VendItem();
		if (PickerStatus.Empty == vendItemResult.PickerStatus)
		{
			appLog.Write($"The item {matrix} was taken.", LogEntryType.Error);
			return false;
		}
		if (vendItemResult.PickerStatus == PickerStatus.HardwareError || PickerStatus.Obstructed == vendItemResult.PickerStatus)
		{
			return false;
		}
		PickerStatus pickerStatus = ControllerService.Instance.TakeDiskAtDoor();
		if (PickerStatus.DiscInPicker != pickerStatus)
		{
			appLog.Write($"Take disk at door returned code {pickerStatus.ToString().ToUpper()}", LogEntryType.Info);
			return false;
		}
		return true;
	}
}
