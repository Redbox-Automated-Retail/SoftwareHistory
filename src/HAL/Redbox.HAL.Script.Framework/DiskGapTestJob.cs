using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal sealed class DiskGapTestJob : NativeJobAdapter
{
	private int WaitTime => RegistryOptions.Instance.GetSecretValue("SOFTWARE\\Redbox\\HAL", "ReadPause", 10);

	private bool Clear => RegistryOptions.Instance.GetSecretValue("SOFTWARE\\Redbox\\HAL", "Clear", def: false);

	protected override void ExecuteInner()
	{
		if (Clear)
		{
			bool flag = ControllerService.Instance.ClearDiskFromPicker(RollerDirection.None);
			LogHelper.Instance.Log("WaitToClear returned {0}", flag);
			PickerBoard.Instance.LogPickerSensorState();
			PickerBoard.Instance.StopRoller();
			return;
		}
		_ = WaitTime;
		PickerBoard.Instance.StartRollerIn();
		for (int i = 0; i < 150; i++)
		{
			PickerBoard.Instance.ReadPickerSensors();
			PerfFunctions.SpinWait(10);
		}
	}
}
