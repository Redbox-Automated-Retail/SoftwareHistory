using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "KioskTest", Operand = "KIOSK-TEST", HideFromList = true)]
internal sealed class KioskTestJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		IDeck deck = QLM.Deck;
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "KioskTest", flushOnWrite: false, "KioskTestErrors");
		bool flag = base.Context.PopTop<bool>();
		int num = base.Context.PopTop<int>();
		int slot = base.Context.PopTop<int>();
		int num2 = base.Context.PopTop<int>();
		int slot2 = base.Context.PopTop<int>();
		bool flag2 = !ControllerConfiguration.Instance.IsVMZMachine && deck != null;
		bool flag3 = flag2 && deck.Number == num;
		bool flag4 = flag2 && deck.Number == num2;
		if (flag3 && !EngageQlm())
		{
			return;
		}
		if (base.MotionService.MoveTo(num, slot, MoveMode.Get, base.Context.AppLog) != ErrorCodes.Success)
		{
			base.Context.CreateInfoResult("MoveError", "Failed to move to initial slot");
			HandleMachineError();
			return;
		}
		using Scanner scanner = new Scanner();
		IControllerService service = ServiceLocator.Instance.GetService<IControllerService>();
		IGetResult getResult = service.Get(base.AppLog);
		if (!getResult.Success)
		{
			base.Context.CreateInfoResult("GetError", "Failed to pull disc from slot");
			HandleMachineError();
			return;
		}
		base.ControlSystem.Center(CenterDiskMethod.VendDoorAndBack);
		ScanResult scanResult = scanner.Read();
		string scannedMatrix = scanResult.ScannedMatrix;
		if (!scanResult.SnapOk)
		{
			applicationLog.Write($"CAMERA CAPTURE returned an error status {scanResult.Diagnostic}", LogEntryType.Error);
		}
		if (!scanResult.ReadCode)
		{
			applicationLog.Write("The barcode in the picker could not be read.", LogEntryType.Error);
		}
		if (flag && !VendDisk(scannedMatrix))
		{
			HandleMachineError();
		}
		else
		{
			if (flag4 && !EngageQlm())
			{
				return;
			}
			if (base.MotionService.MoveTo(num2, slot2, MoveMode.Put, base.Context.AppLog) != ErrorCodes.Success)
			{
				base.Context.CreateInfoResult("MoveFailed", "Kiosk was unable to move to put position of destination slot");
				HandleMachineError();
				return;
			}
			base.ControlSystem.TrackCycle();
			IPutResult putResult = service.Put(scannedMatrix, base.AppLog);
			if (!putResult.Success)
			{
				base.Context.CreateInfoResult("PutFailed", "Kiosk failed to put disc into slot");
				HandleMachineError();
				return;
			}
			PerfFunctions.Wait(1200);
			if (base.MotionService.MoveTo(num2, slot2, MoveMode.Get, base.AppLog) != ErrorCodes.Success)
			{
				base.Context.CreateInfoResult("MoveFailed", "Kiosk was unable to move to get position of destination slot");
				HandleMachineError();
				return;
			}
			if (flag3 && !flag4)
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
				if (QLM.Disengage(home: true, applicationLog) != ErrorCodes.Success)
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
		if (QLM.Engage(home: true, base.Context.AppLog) == ErrorCodes.Success)
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
		if (base.MotionService.MoveVend(MoveMode.None, base.AppLog) != ErrorCodes.Success)
		{
			base.Context.CreateInfoResult("MoveVendError", "Failed to move to vend position");
			return false;
		}
		if (!base.ControlSystem.TrackOpen().Success)
		{
			appLog.Write("Track Open timed out.", LogEntryType.Error);
			base.Context.CreateInfoResult("TrackOpenTimeOut", "Track Open timedout.");
			return false;
		}
		if (!base.ControlSystem.TrackClose().Success)
		{
			appLog.Write("Track Close timed out.", LogEntryType.Error);
			base.Context.CreateInfoResult("TrackCloseTimeOut", "Track Close timedout.");
			return false;
		}
		if (!base.ControlSystem.VendDoorRent().Success)
		{
			appLog.Write("Vend door timed out trying to open vend door.", LogEntryType.Error);
			base.Context.CreateInfoResult("VendDoorTimeOut", "Vend door timed out trying to open vend door.");
			return false;
		}
		using (VendItemOperation vendItemOperation = new VendItemOperation())
		{
			VendItemResult vendItemResult = vendItemOperation.Execute();
			if (ErrorCodes.PickerEmpty == vendItemResult.Status)
			{
				appLog.Write($"The item {matrix} was taken.", LogEntryType.Error);
				return false;
			}
			if (ErrorCodes.PickerFull != vendItemResult.Status)
			{
				return false;
			}
		}
		using AcceptDiskOperation acceptDiskOperation = new AcceptDiskOperation();
		ErrorCodes errorCodes = acceptDiskOperation.Execute();
		if (ErrorCodes.PickerFull != errorCodes)
		{
			appLog.Write($"Take disk at door returned code {errorCodes.ToString().ToUpper()}", LogEntryType.Info);
			return false;
		}
		return true;
	}
}
