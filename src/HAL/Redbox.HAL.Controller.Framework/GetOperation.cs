using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

internal sealed class GetOperation : AbstractOperation<IGetResult>
{
	private IFormattedLog Log;

	private readonly ILocation Location;

	private readonly IGetObserver Observer;

	private readonly IDeck Deck;

	private readonly int Slot;

	private readonly GetResult Result;

	public override IGetResult Execute()
	{
		if (Validate())
		{
			FetchDisk();
		}
		return Result;
	}

	protected override void OnDispose()
	{
		Log = null;
	}

	internal GetOperation(ILocation location, IGetObserver observer, IFormattedLog log)
	{
		Observer = ((observer == null) ? DefaultGetObserver.Instance : observer);
		Log = log;
		IDecksService service = ServiceLocator.Instance.GetService<IDecksService>();
		Deck = service.GetFrom(location);
		Slot = location.Slot;
		Location = location;
		Result = new GetResult(Location);
	}

	private void FetchDisk()
	{
		if (Location.IsWide)
		{
			SettleDiskInSlot();
		}
		if (!Controller.TrackOpen().Success)
		{
			Result.Update(ErrorCodes.TrackOpenTimeout);
			return;
		}
		Controller.StartRollerOut();
		ErrorCodes errorCodes = PullFrom(Location, ControllerConfiguration.Instance.NumberOfPulls);
		if (errorCodes != ErrorCodes.Success)
		{
			Controller.StopRoller();
			Controller.TrackClose();
			Result.Update(errorCodes);
			return;
		}
		if (!Controller.TrackClose().Success)
		{
			Controller.StopRoller();
			PushIntoSlot();
			Result.Update(ErrorCodes.TrackCloseTimeout);
			return;
		}
		IPickerSensorReadResult pickerSensorReadResult = Controller.ReadSensors();
		if (pickerSensorReadResult.Sensors[0] && !pickerSensorReadResult.Sensors[1])
		{
			errorCodes = PullFrom(Location, 1);
		}
		PerfFunctions.SpinWait(300);
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		if (Controller.RollerToPosition(RollerPosition.Position4, 6000, logSensors: false).Success)
		{
			service.Reset(Location);
			return;
		}
		if (PickerBoard.IsFull())
		{
			OnStuck();
			return;
		}
		LogHelper.Instance.Log("GET: no disk in picker after pull.", LogEntryType.Info);
		Controller.LogPickerSensorState();
		Result.Update(ErrorCodes.SlotEmpty);
		if (Observer.OnEmpty(Result))
		{
			service.Reset(Location);
		}
	}

	private void OnStuck()
	{
		LogHelper.Instance.Log("Disk did not make it to sensor 4.", LogEntryType.Error);
		Controller.LogPickerSensorState(LogEntryType.Error);
		Controller.StartRollerIn();
		try
		{
			bool flag = ClearDiskFromPicker();
			PushIntoSlot();
			LogHelper.Instance.Log(LogEntryType.Error, "GET: couldn't get disc, ClearDiscFromPicker returned {0}", flag ? "TIMEOUT" : "SUCCESS");
			Controller.LogPickerSensorState(LogEntryType.Error);
			Result.Update(PickerBoard.IsFull() ? ErrorCodes.PickerObstructed : ErrorCodes.ItemStuck);
		}
		finally
		{
			Controller.StopRoller();
		}
		Observer.OnStuck(Result);
	}

	private bool Validate()
	{
		if (Controller.TrackState != TrackState.Closed && !Controller.TrackClose().Success)
		{
			Result.Update(ErrorCodes.TrackCloseTimeout);
			return false;
		}
		IPickerSensorReadResult pickerSensorReadResult = Controller.ReadSensors();
		if (pickerSensorReadResult.IsFull)
		{
			LogHelper.Instance.Log($">> ControllerService.Get: {ErrorCodes.PickerFull.ToString()}", LogEntryType.Info);
			Result.Update(ErrorCodes.PickerFull);
			return false;
		}
		return true;
	}
}
