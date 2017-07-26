using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "hardware-healer", Operand = "HARDWARE-HEALER")]
internal class HardwareHealerJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		ApplicationLog.ConfigureLog(base.Context, logAppends: true, "Service", flushOnWrite: false, null);
		if (ControllerConfiguration.Instance.EnableLeiweHealing)
		{
			bool flag = false;
			for (int i = 0; i < 2; i++)
			{
				if (base.ControlSystem.GetBoardVersion(ControlBoards.Serial) != "TIMEOUT" && base.ControlSystem.GetBoardVersion(ControlBoards.Aux) != "TIMEOUT" && base.ControlSystem.GetBoardVersion(ControlBoards.Picker) != "TIMEOUT")
				{
					flag = true;
					break;
				}
				base.ControlSystem.Shutdown();
				Thread.Sleep(500);
				base.ControlSystem.Initialize();
			}
			if (!flag)
			{
				AddError("Failed to reset boards.");
			}
		}
		if (ControllerConfiguration.Instance.EnableArcusHealing && !base.MotionService.CommunicationOk())
		{
			if (!base.MotionService.Reset(base.Context.AppLog, quick: false))
			{
				AddError("Can't reset controller.");
			}
			else
			{
				base.MotionService.MoveVend(MoveMode.Get, base.Context.AppLog);
			}
		}
		if (ControllerConfiguration.Instance.EnableCameraHealing)
		{
			IScannerDevice configuredDevice = ServiceLocator.Instance.GetService<IScannerDeviceService>().GetConfiguredDevice();
			if (!configuredDevice.Snap().SnapOk)
			{
				bool flag2 = configuredDevice.Restart();
				base.Context.ServiceLog("Camera snap returned failure; restart device returned {0}", flag2.ToString());
			}
		}
	}

	internal HardwareHealerJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
