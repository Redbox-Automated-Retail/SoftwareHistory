using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "KioskTest", Operand = "KIOSK-TEST", HideFromList = true)]
internal sealed class KioskTestJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		ServiceLocator.Instance.GetService<IControlSystem>();
		IDeck qlmDeck = ServiceLocator.Instance.GetService<IDecksService>().QlmDeck;
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "KioskTest", flushOnWrite: false, "KioskTestErrors");
		bool flag = base.Context.PopTop<bool>();
		int num = base.Context.PopTop<int>();
		int slot = base.Context.PopTop<int>();
		int num2 = base.Context.PopTop<int>();
		int slot2 = base.Context.PopTop<int>();
		int num3;
		int num4;
		if (!ControllerConfiguration.Instance.IsVMZMachine)
		{
			num3 = ((qlmDeck != null) ? 1 : 0);
			if (num3 != 0)
			{
				num4 = ((qlmDeck.Number == num) ? 1 : 0);
				goto IL_00a1;
			}
		}
		else
		{
			num3 = 0;
		}
		num4 = 0;
		goto IL_00a1;
		IL_00a1:
		bool flag2 = (byte)num4 != 0;
		bool flag3 = num3 != 0 && qlmDeck.Number == num2;
		if (flag2 && !EngageQlm())
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
		IControllerService service2 = ServiceLocator.Instance.GetService<IControllerService>();
		if (!service2.Get(base.AppLog).Success)
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
			if (flag3 && !EngageQlm())
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
			if (!service2.Put(scannedMatrix, base.AppLog).Success)
			{
				base.Context.CreateInfoResult("PutFailed", "Kiosk failed to put disc into slot");
				HandleMachineError();
				return;
			}
			service.Wait(1200);
			if (base.MotionService.MoveTo(num2, slot2, MoveMode.Get, base.AppLog) != ErrorCodes.Success)
			{
				base.Context.CreateInfoResult("MoveFailed", "Kiosk was unable to move to get position of destination slot");
				HandleMachineError();
				return;
			}
			if (flag2 && !flag3)
			{
				QlmStatus qlmStatus = base.ControlSystem.GetQlmStatus();
				if (QlmStatus.AuxNotResponsive == qlmStatus)
				{
					applicationLog.Write("The AUX board is not responsive.");
					HandleMachineError();
					return;
				}
				if (QlmStatus.Empty == qlmStatus)
				{
					applicationLog.Write("There is no QLM in the bay.", LogEntryType.Error);
					HandleMachineError();
					return;
				}
				applicationLog.Write("Disengage the QLM.");
				if (base.ControlSystem.DisengageQlm(home: true, applicationLog) != ErrorCodes.Success)
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
		IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
		QlmStatus qlmStatus = service.GetQlmStatus();
		if (QlmStatus.AuxNotResponsive == qlmStatus)
		{
			base.Context.AppLog.Write("QLM Status: the AUX board is not responsive.");
			AddError("AUX not responsive.");
			return false;
		}
		if (QlmStatus.Engaged == qlmStatus)
		{
			return true;
		}
		if (QlmStatus.Empty == qlmStatus)
		{
			base.Context.AppLog.Write("There is no QLM in the bay.", LogEntryType.Error);
			AddError("NO QLM IN BAY.");
			return false;
		}
		base.Context.AppLog.Write("Engaging the QLM.", LogEntryType.Info);
		if (service.EngageQlm(base.Context.AppLog) == ErrorCodes.Success)
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
