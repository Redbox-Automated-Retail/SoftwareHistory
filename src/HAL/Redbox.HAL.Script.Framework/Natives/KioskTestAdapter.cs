using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework.Natives;

internal class KioskTestAdapter : IDisposable
{
	protected readonly IControlSystem ControlSystem;

	protected readonly IMotionControlService MotionService;

	protected readonly ExecutionContext Context;

	protected readonly ExecutionResult Result;

	private readonly ApplicationLog AppLog;

	public void Dispose()
	{
	}

	protected virtual ErrorCodes OnUnitComplete()
	{
		return ErrorCodes.Success;
	}

	protected virtual bool OnVendDisk(string matrix)
	{
		using (VendItemOperation vendItemOperation = new VendItemOperation(2))
		{
			VendItemResult vendItemResult = vendItemOperation.Execute();
			if (ErrorCodes.PickerEmpty == vendItemResult.Status)
			{
				Context.AppLog.Write($"The item {matrix} was taken.", LogEntryType.Error);
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
			Context.AppLog.Write($"Take disk at door returned code {errorCodes.ToString().ToUpper()}", LogEntryType.Info);
			return false;
		}
		return true;
	}

	internal void Run()
	{
		bool flag = Context.PopTop<bool>();
		int deck = Context.PopTop<int>();
		int slot = Context.PopTop<int>();
		int deck2 = Context.PopTop<int>();
		int slot2 = Context.PopTop<int>();
		if (MotionService.MoveTo(deck, slot, MoveMode.Get, Context.AppLog) != ErrorCodes.Success)
		{
			Context.CreateInfoResult("MoveError", "Failed to move to initial slot");
			HandleMachineError();
			return;
		}
		using Scanner scanner = new Scanner();
		IControllerService service = ServiceLocator.Instance.GetService<IControllerService>();
		if (!service.Get(AppLog).Success)
		{
			Context.CreateInfoResult("GetError", "Failed to pull disc from slot");
			HandleMachineError();
			return;
		}
		ControlSystem.Center(CenterDiskMethod.VendDoorAndBack);
		ScanResult scanResult = scanner.Read();
		string scannedMatrix = scanResult.ScannedMatrix;
		if (!scanResult.SnapOk)
		{
			AppLog.Write($"CAMERA CAPTURE returned an error status {scanResult.Diagnostic}", LogEntryType.Error);
		}
		if (!scanResult.ReadCode)
		{
			AppLog.Write("The barcode in the picker could not be read.", LogEntryType.Error);
		}
		if (flag && !VendDisk(scannedMatrix))
		{
			HandleMachineError();
			return;
		}
		if (MotionService.MoveTo(deck2, slot2, MoveMode.Put, Context.AppLog) != ErrorCodes.Success)
		{
			Context.CreateInfoResult("MoveFailed", "Kiosk was unable to move to put position of destination slot");
			HandleMachineError();
			return;
		}
		ControlSystem.TrackCycle();
		if (!service.Put(scannedMatrix, AppLog).Success)
		{
			Context.CreateInfoResult("PutFailed", "Kiosk failed to put disc into slot");
			HandleMachineError();
			return;
		}
		ErrorCodes errorCodes = OnUnitComplete();
		if (errorCodes != ErrorCodes.Success)
		{
			Context.CreateInfoResult("PostUnitFailed", $"Post unit test failed with status {errorCodes}");
			HandleMachineError();
		}
		else
		{
			Context.CreateInfoResult("Success", "SUCCESS");
		}
	}

	internal KioskTestAdapter(ExecutionResult r, ExecutionContext c)
	{
		Context = c;
		Result = r;
		ControlSystem = ServiceLocator.Instance.GetService<IControlSystem>();
		MotionService = ServiceLocator.Instance.GetService<IMotionControlService>();
		AppLog = ApplicationLog.ConfigureLog(Context, logAppends: true, "KioskTest", flushOnWrite: false, "KioskTest");
	}

	private bool VendDisk(string matrix)
	{
		ApplicationLog appLog = Context.AppLog;
		appLog.Write("Performing a Vend Test.");
		if (MotionService.MoveVend(MoveMode.None, AppLog) != ErrorCodes.Success)
		{
			Context.CreateInfoResult("MoveVendError", "Failed to move to vend position");
			return false;
		}
		if (!ControlSystem.TrackOpen().Success)
		{
			appLog.Write("Track Open timed out.", LogEntryType.Error);
			Context.CreateInfoResult("TrackOpenTimeOut", "Track Open timedout.");
			return false;
		}
		if (!ControlSystem.TrackClose().Success)
		{
			appLog.Write("Track Close timed out.", LogEntryType.Error);
			Context.CreateInfoResult("TrackCloseTimeOut", "Track Close timedout.");
			return false;
		}
		if (!ControlSystem.VendDoorRent().Success)
		{
			appLog.Write("Vend door timed out trying to open vend door.", LogEntryType.Error);
			Context.CreateInfoResult("VendDoorTimeOut", "Vend door timed out trying to open vend door.");
			return false;
		}
		return OnVendDisk(matrix);
	}

	private void HandleMachineError()
	{
		Context.AppLog.Write("An unrecoverable error occurred in KioskTest.", LogEntryType.Error);
		Context.CreateInfoResult("MachineError", "An unrecoverable error occurred in KioskTest");
		AddError("MACHINE ERROR");
	}

	private void AddError(string errMsg)
	{
		Result.Errors.Add(Error.NewError("E999", "Execution context error.", errMsg));
	}
}
