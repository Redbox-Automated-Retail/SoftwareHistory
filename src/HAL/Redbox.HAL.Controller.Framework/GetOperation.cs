using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

internal sealed class GetOperation : IDisposable
{
	private bool Disposed;

	private IFormattedLog Log;

	private readonly IGetObserver Observer;

	private readonly Deck Deck;

	private readonly int Slot;

	private readonly GetResult Result = new GetResult();

	public void Dispose()
	{
		if (!Disposed)
		{
			Disposed = true;
			Log = null;
		}
	}

	internal GetResult Execute()
	{
		if (Validate())
		{
			FetchDisk();
		}
		return Result;
	}

	internal GetOperation(IGetObserver observer, IFormattedLog log)
	{
		Observer = ((observer == null) ? DefaultGetObserver.Instance : observer);
		Log = log;
		Deck = DecksManager.Instance.GetDeck(ControllerService.Instance.Location.Deck);
		Slot = ControllerService.Instance.Location.Slot;
	}

	private void FetchDisk()
	{
		Deck.Fill(Result, Slot);
		if (Deck.IsSlotSellThru(Slot) && ControllerConfiguration.Instance.SettleDiscInWideSlot)
		{
			Gripper.SettleDiskInSlot();
		}
		if (Track.OpenIfClosed())
		{
			Result.SetHardwareError(PickerHardwareError.TrackOpenTimeout);
			return;
		}
		PickerBoard.StartRollerOut();
		PickerHardwareError pickerHardwareError = Gripper.PullFromLocation(ControllerConfiguration.Instance.NumberOfPulls);
		if (pickerHardwareError != PickerHardwareError.Success)
		{
			PickerBoard.StopRoller();
			Track.Close();
			Result.SetHardwareError(pickerHardwareError);
			return;
		}
		if (Track.Close())
		{
			PickerBoard.StopRoller();
			Gripper.PushIntoSlot();
			Result.SetHardwareError(PickerHardwareError.TrackCloseTimeout);
			return;
		}
		bool[] array = PickerBoard.ReadAsFlags();
		if (array[0] && !array[1])
		{
			pickerHardwareError = Gripper.PullFromLocation(1);
		}
		PerfFunctions.SpinWait(300);
		if (!PickerBoard.RollerToPosition(RollerPosition.Position4, 6000, logError: false))
		{
			Deck.ResetLocation(Slot);
			return;
		}
		if (PickerBoard.IsFull())
		{
			OnStuck();
			return;
		}
		LogHelper.Instance.Log("GET: no disk in picker after pull.", LogEntryType.Info);
		PickerBoard.LogPickerSensorState(LogEntryType.Info);
		Result.UpdateCode(GetResultCode.SlotEmpty);
		if (Observer.OnEmpty(Result, Deck, Slot))
		{
			Deck.ResetLocation(Slot);
		}
	}

	private void OnStuck()
	{
		LogHelper.Instance.Log("Disk did not make it to sensor 4.", LogEntryType.Error);
		PickerBoard.LogPickerSensorState(LogEntryType.Error);
		PickerBoard.StartRollerIn();
		try
		{
			bool flag = PickerBoard.ClearDiskFromPicker();
			Gripper.PushIntoSlot();
			LogHelper.Instance.Log(string.Format("GET: couldn't get disc, ClearDiscFromPicker returned {0}", flag ? "TIMEOUT" : "SUCCESS"), LogEntryType.Error);
			PickerBoard.LogPickerSensorState(LogEntryType.Error);
			Result.UpdateCode(PickerBoard.IsFull() ? GetResultCode.PickerObstructed : GetResultCode.ItemStuck);
		}
		finally
		{
			PickerBoard.StopRoller();
		}
		Observer.OnStuck(Result, Deck, Slot);
	}

	private bool Validate()
	{
		if (Track.CloseIfOpen())
		{
			Result.SetHardwareError(PickerHardwareError.TrackCloseTimeout);
			return false;
		}
		PickerSensorReadResult pickerSensorReadResult = PickerSensorBar.ReadSensors();
		if (pickerSensorReadResult.IsFull)
		{
			LogHelper.Instance.Log($">> ControllerService.Get: {GetResultCode.PickerFull.ToString()}", LogEntryType.Info);
			Result.UpdateCode(GetResultCode.PickerFull);
			return false;
		}
		return true;
	}
}
