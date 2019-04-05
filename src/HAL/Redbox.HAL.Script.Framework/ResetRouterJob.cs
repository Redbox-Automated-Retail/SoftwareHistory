using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "power-cycle-router")]
internal sealed class ResetRouterJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		IPowerCycleDeviceService service = ServiceLocator.Instance.GetService<IPowerCycleDeviceService>();
		IPowerCycleDevice powerCycleDevice = service.Get(PowerCycleDevices.Router);
		if (!powerCycleDevice.Configured)
		{
			AddError("Router relay is not configured.");
			return;
		}
		ErrorCodes errorCodes = powerCycleDevice.CyclePower();
		LogHelper.Instance.Log("[ResetRouterJob] CyclePower returned {0}", errorCodes);
		IHardwareCorrectionStatisticService service2 = ServiceLocator.Instance.GetService<IHardwareCorrectionStatisticService>();
		service2.Insert(HardwareCorrectionStatistic.RouterRecycle, base.Context, ErrorCodes.Success == errorCodes);
		if (errorCodes != ErrorCodes.Success)
		{
			AddError("Power cycle failed.");
			IControlSystemService service3 = ServiceLocator.Instance.GetService<IControlSystemService>();
			service3.Restart();
		}
	}

	internal ResetRouterJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
