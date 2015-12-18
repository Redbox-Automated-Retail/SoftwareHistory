using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "push-in-dvd", Operand = "PUSH-IN-DVD")]
internal sealed class PushInDVDJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		try
		{
			_ = ControllerService.Instance;
			PickerBoard.StartRollerIn();
			PerfFunctions.Wait(500);
			PickerBoard.StopRoller();
			if (Track.Open())
			{
				HandleMachineError("TRACK OPEN TIMEOUT");
				return;
			}
			if (Gripper.SetFinger(GripperFingerState.Closed))
			{
				HandleMachineError("GRIPPER CLOSE TIMEOUT");
				return;
			}
			int pushTime = ControllerConfiguration.Instance.PushTime;
			Gripper.PushToSensor(pushTime);
			if (Gripper.SetFinger(GripperFingerState.Rent))
			{
				HandleMachineError("GRIPPER RENT TIMEOUT");
				return;
			}
			if (Gripper.RetractArm())
			{
				HandleMachineError("GRIPPER RETRACT TIMEOUT");
				return;
			}
			if (Gripper.SetFinger(GripperFingerState.Closed))
			{
				HandleMachineError("GRIPPER CLOSE TIMEOUT");
				return;
			}
			Gripper.PushToSensor(pushTime);
			if (Gripper.SetFinger(GripperFingerState.Rent))
			{
				HandleMachineError("GRIPPER RENT TIMEOUT");
			}
			else if (Gripper.RetractArm())
			{
				HandleMachineError("GRIPPER RETRACT TIMEOUT");
			}
		}
		finally
		{
			Track.Close();
			Gripper.RetractArm();
		}
	}

	internal PushInDVDJob(ExecutionResult r, ExecutionContext c)
		: base(r, c)
	{
	}

	private void HandleMachineError(string error)
	{
		base.Context.SetSymbolValue("ERROR-STATE", error);
		base.Context.CreateInfoResult("MachineError", "There was an error pushing the DVD in.");
	}
}
