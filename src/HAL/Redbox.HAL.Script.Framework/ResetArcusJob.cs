using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal sealed class ResetArcusJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		MotionController controller = MotionController.GetController();
		if (!controller.ResetDeviceDriver())
		{
			base.Context.SetSymbolValue("MSTESTER-SYMBOL-RESET-FAIL", "RESET of the controller failed.");
			AddError("Reset failure.");
		}
		else if (!controller.Restart(base.AppLog))
		{
			base.Context.SetSymbolValue("MSTESTER-SYMBOL-INIT-FAIL", "Failed to initialize the controller.");
			AddError("Controller initialize failure.");
		}
		else
		{
			base.Context.SetSymbolValue("MSTESTER-SYMBOL-SUCCESS", "RESET OK.");
		}
	}
}
