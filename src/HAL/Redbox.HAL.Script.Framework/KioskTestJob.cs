using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;
using Redbox.HAL.DataMatrix.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "KioskTest", Operand = "KIOSK-TEST", HideFromList = true)]
internal sealed class KioskTestJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		Deck deck = QLM.Deck;
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "KioskTest", flushOnWrite: false, "KioskTestErrors");
		bool top = GetTop<bool>();
		int top2 = GetTop<int>();
		int top3 = GetTop<int>();
		int top4 = GetTop<int>();
		int top5 = GetTop<int>();
		bool flag = !ControllerConfiguration.Instance.IsVMZMachine && deck != null;
		bool flag2 = flag && deck.Number == top2;
		bool flag3 = flag && deck.Number == top4;
		if (flag2 && !EngageQlm())
		{
			return;
		}
		if (ControllerService.Instance.MoveTo(top2, top3, MoveMode.Get, base.Context.AppLog) != MotionResult.Success)
		{
			base.Context.CreateInfoResult("MoveError", "Failed to move to initial slot");
			HandleMachineError();
			return;
		}
		using Scanner scanner = new Scanner();
		GetResult getResult = ControllerService.Instance.Get(base.AppLog);
		if (!getResult.Success)
		{
			base.Context.CreateInfoResult("GetError", "Failed to pull disc from slot");
			HandleMachineError();
			return;
		}
		ControllerService.Instance.CenterItem(CenterDiskMethod.VendDoorAndBack);
		ScanResult scanResult = scanner.Read();
		string scannedMatrix = scanResult.ScannedMatrix;
		if (!scanResult.SnapOk)
		{
			applicationLog.Write($"CAMERA CAPTURE returned an error status {scanner.Diagnostic(scanResult)}", LogEntryType.Error);
		}
		if (!scanResult.ReadCode)
		{
			applicationLog.Write("The barcode in the picker could not be read.", LogEntryType.Error);
		}
		if (top && !VendDisk(scannedMatrix))
		{
			HandleMachineError();
		}
		else
		{
			if (flag3 && !EngageQlm())
			{
				return;
			}
			if (ControllerService.Instance.MoveTo(top4, top5, MoveMode.Put, base.Context.AppLog) != MotionResult.Success)
			{
				base.Context.CreateInfoResult("MoveFailed", "Kiosk was unable to move to put position of destination slot");
				HandleMachineError();
				return;
			}
			PutResult putResult = ControllerService.Instance.Put(scannedMatrix, base.AppLog);
			if (!putResult.Success)
			{
				base.Context.CreateInfoResult("PutFailed", "Kiosk failed to put disc into slot");
				HandleMachineError();
				return;
			}
			PerfFunctions.Wait(1200);
			if (ControllerService.Instance.MoveTo(top4, top5, MoveMode.Get, base.AppLog) != MotionResult.Success)
			{
				base.Context.CreateInfoResult("MoveFailed", "Kiosk was unable to move to get position of destination slot");
				HandleMachineError();
				return;
			}
			if (flag2 && !flag3)
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
				applicationLog.Write("Disengage the QLM.");
				QlmMotionResult qlmMotionResult = QLM.Disengage(home: true, applicationLog);
				if (QlmMotionResult.Success != qlmMotionResult)
				{
					base.Context.CreateInfoResult("QlmDeckDisengageFailure", "The QLM disengage failed.");
					AddError("The QLM Case didn't disengage.");
					return;
				}
				base.Context.CreateInfoResult("QlmDeckDisengageSuccess", "The QLM Disengaged successfully.");
			}
			base.Context.CreateInfoResult("Success", "SUCCESS");
		}
	}

	internal KioskTestJob(ExecutionResult r, ExecutionContext c)
		: base(r, c)
	{
	}

	private void HandleMachineError()
	{
		base.Context.AppLog.Write("An unrecoverable error occurred in KioskTest.", LogEntryType.Error);
		base.Context.CreateInfoResult("MachineError", "An unrecoverable error occurred in KioskTest");
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
		base.Context.CreateInfoResult("MachineError", "QLM Engage timeout.");
		AddError("QLM Engage Timeout");
		return false;
	}

	private bool VendDisk(string matrix)
	{
		ApplicationLog appLog = base.Context.AppLog;
		appLog.Write("Performing a Vend Test.");
		if (ControllerService.Instance.MoveToVend(MoveMode.None, base.AppLog) != MotionResult.Success)
		{
			base.Context.CreateInfoResult("MoveVendError", "Failed to move to vend position");
			return false;
		}
		if (Track.Open())
		{
			appLog.Write("Track Open timed out.", LogEntryType.Error);
			base.Context.CreateInfoResult("TrackOpenTimeOut", "Track Open timedout.");
			return false;
		}
		if (Track.Close())
		{
			appLog.Write("Track Close timed out.", LogEntryType.Error);
			base.Context.CreateInfoResult("TrackCloseTimeOut", "Track Close timedout.");
			return false;
		}
		if (VendDoor.Rent())
		{
			appLog.Write("Vend door timed out trying to open vend door.", LogEntryType.Error);
			base.Context.CreateInfoResult("VendDoorTimeOut", "Vend door timed out trying to open vend door.");
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
