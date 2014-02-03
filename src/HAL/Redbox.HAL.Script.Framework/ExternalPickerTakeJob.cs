using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal class ExternalPickerTakeJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		try
		{
			Gripper instance = Gripper.Instance;
			PickerBoard.Instance.StartRollerIn();
			Track.Instance.Open();
			instance.SetFinger(GripperFingerState.Open);
			instance.TimedExtend(350);
			instance.SetFinger(GripperFingerState.Closed);
			instance.RetractArm();
			instance.SetFinger(GripperFingerState.Rent);
			instance.TimedExtend(350);
			instance.SetFinger(GripperFingerState.Closed);
			instance.RetractArm();
			Track.Instance.Close();
			PickerBoard.Instance.RollerToPosition(RollerPosition.Position2, 4000);
		}
		finally
		{
			PickerBoard.Instance.StopRoller();
		}
	}
}
