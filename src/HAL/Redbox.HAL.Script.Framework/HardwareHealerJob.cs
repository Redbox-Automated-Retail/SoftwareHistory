using Redbox.HAL.Controller.Framework;
using Redbox.HAL.DataMatrix.Framework;

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
				if (ControlSystem.Instance.GetBoardVersion(ControlBoards.Serial) != "TIMEOUT" && ControlSystem.Instance.GetBoardVersion(ControlBoards.Aux) != "TIMEOUT" && ControlSystem.Instance.GetBoardVersion(ControlBoards.Picker) != "TIMEOUT")
				{
					flag = true;
					break;
				}
				ControlSystem.Instance.ClosePort(500);
				ControlSystem.Instance.Initialize();
			}
			if (!flag)
			{
				AddError("Failed to reset boards.");
			}
		}
		if (ControllerConfiguration.Instance.EnableArcusHealing)
		{
			MotionController controller = MotionController.GetController();
			if (ControllerConfiguration.Instance.EnableArcusKeepalive)
			{
				if (!controller.CommunicationOk())
				{
					base.Context.ServiceLog("The motion controller is not responsive.");
					AddError("Arcus COMM failure.");
				}
			}
			else if (!controller.CommunicationOk() && !ResetMotionController())
			{
				base.Context.ServiceLog("The motion controller is not responsive; driver reset returned FAILURE.");
				AddError("Can't reset controller.");
			}
		}
		if (ControllerConfiguration.Instance.EnableCameraHealing)
		{
			IScannerDevice configuredDevice = ScannerDeviceFactory.GetConfiguredDevice();
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
