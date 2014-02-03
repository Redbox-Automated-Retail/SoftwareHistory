using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal class PushInDVDJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		try
		{
			_ = ControllerService.Instance;
			PickerBoard.Instance.StartRollerIn();
			PerfFunctions.Wait(500);
			PickerBoard.Instance.StopRoller();
			if (base.Track.Open())
			{
				HandleMachineError("TRACK OPEN TIMEOUT");
				return;
			}
			if (base.Gripper.SetFinger(GripperFingerState.Closed))
			{
				HandleMachineError("GRIPPER CLOSE TIMEOUT");
				return;
			}
			int pushTime = ControllerConfiguration.Instance.PushTime;
			base.Gripper.PushToSensor(pushTime);
			if (base.Gripper.SetFinger(GripperFingerState.Rent))
			{
				HandleMachineError("GRIPPER RENT TIMEOUT");
				return;
			}
			if (base.Gripper.RetractArm())
			{
				HandleMachineError("GRIPPER RETRACT TIMEOUT");
				return;
			}
			if (base.Gripper.SetFinger(GripperFingerState.Closed))
			{
				HandleMachineError("GRIPPER CLOSE TIMEOUT");
				return;
			}
			base.Gripper.PushToSensor(pushTime);
			if (base.Gripper.SetFinger(GripperFingerState.Rent))
			{
				HandleMachineError("GRIPPER RENT TIMEOUT");
			}
			else if (base.Gripper.RetractArm())
			{
				HandleMachineError("GRIPPER RETRACT TIMEOUT");
			}
		}
		finally
		{
			base.Track.Close();
			base.Gripper.RetractArm();
		}
	}

	private void HandleMachineError(string error)
	{
		base.Context.SetSymbolValue("ERROR-STATE", error);
		ResultHelper.Instance.CreateInfoResult("MachineError", "There was an error pushing the DVD in.");
	}
}
