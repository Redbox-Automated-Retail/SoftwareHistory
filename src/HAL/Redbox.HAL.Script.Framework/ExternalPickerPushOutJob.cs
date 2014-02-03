using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal sealed class ExternalPickerPushOutJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		try
		{
			base.PickerBoard.RollerToPosition(RollerPosition.Position4, 3000);
			ControllerService.Instance.GripperPushOut();
			ControllerService.Instance.ClearDiskFromPicker(RollerDirection.None);
		}
		finally
		{
			base.PickerBoard.StopRoller();
		}
	}

	private bool Cleared(PickerSensorReadResult res)
	{
		if (res.ReadError)
		{
			return false;
		}
		if (!res.Sensors[1] && !res.Sensors[2] && !res.Sensors[3])
		{
			return !res.Sensors[4];
		}
		return false;
	}
}
