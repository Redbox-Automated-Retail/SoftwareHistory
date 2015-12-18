using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework.Natives;

[NativeJob(ProgramName = "ms-pull-in-dvd", Operand = "MS-PULL-IN-DVD")]
internal sealed class MSPullInDVDJob : NativeJobAdapter
{
	private string ErrorCode = "SUCCESS";

	protected override void ExecuteInner()
	{
		PickerSensorReadResult pickerSensorReadResult = PickerBoard.ReadSensors();
		if (!pickerSensorReadResult.IsFull)
		{
			base.Context.CreateInfoResult("InvalidJobUse", "Picker is clear.");
			AddError("pull-in-dvd failed.");
			return;
		}
		try
		{
			ControllerLocation location = ControllerService.Instance.Location;
			GripperFingerState finger = GripperFingerState.Rent;
			if (location.IsValid)
			{
				Deck deck = DecksManager.Instance.GetDeck(location.Deck);
				if (deck.IsSlotSellThru(location.Slot))
				{
					finger = GripperFingerState.Open;
				}
			}
			if (Track.Open())
			{
				ErrorCode = "TRACK-OPEN-TIMEOUT";
				HandleMachineError();
				return;
			}
			PickerBoard.StartRollerOut();
			for (int i = 0; i < ControllerConfiguration.Instance.NumberOfPulls; i++)
			{
				if (Gripper.SetFinger(finger))
				{
					ErrorCode = "GRIPPER FINGER TIMEOUT";
					HandleMachineError();
					return;
				}
				if (Gripper.ExtendArmToSensor())
				{
					ErrorCode = "GRIPPER EXTEND TIMEOUT";
					HandleMachineError();
					return;
				}
				if (Gripper.SetFinger(GripperFingerState.Closed))
				{
					ErrorCode = "GRIPPER FINGER TIMEOUT";
					HandleMachineError();
					return;
				}
				if (Gripper.RetractArm())
				{
					ErrorCode = "GRIPPER RETRACT TIMEOUT";
					HandleMachineError();
					return;
				}
			}
			Gripper.SetFinger(GripperFingerState.Open);
			if (Track.Close())
			{
				ErrorCode = "TRACK CLOSE TIMEOUT";
				HandleMachineError();
			}
			else if (PickerBoard.RollerToPosition(RollerPosition.Position4, 3000))
			{
				ErrorCode = "ROLLER POS4 TIMEOUT";
				HandleMachineError();
			}
		}
		finally
		{
			Track.Close();
			PickerBoard.StopRoller();
			Gripper.SetFinger(GripperFingerState.Rent);
			Gripper.RetractArm();
		}
	}

	internal MSPullInDVDJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}

	private void HandleMachineError()
	{
		base.Context.CreateInfoResult("MachineError", ErrorCode);
		AddError("pull-in-dvd failed.");
	}
}
