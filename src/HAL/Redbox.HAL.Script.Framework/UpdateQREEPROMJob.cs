using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "update-qr-eeprom", Operand = "UPDATE-QR-EEPROM")]
internal sealed class UpdateQREEPROMJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		if (!QuickReturnDevice.Instance.IsConfigured)
		{
			return;
		}
		if (!ExecutionEngine.Instance.OutsideRebootWindow())
		{
			AddError("The job is close to reboot.");
			return;
		}
		QuickReturnPoller.Instance.Shutdown();
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		service.Wait(5000);
		if (QuickReturnDevice.Instance.UpdateDeviceImages() == 0)
		{
			AddError("Unable to update device images; check service logs.");
			return;
		}
		service.Wait(5000);
		QuickReturnPoller.Instance.Start();
	}

	internal UpdateQREEPROMJob(ExecutionResult result, ExecutionContext context)
		: base(result, context)
	{
	}
}
