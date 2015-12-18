using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[Poller(Name = "HardwareHealer")]
internal class HardwareHealerPoller : ScriptPoller
{
	protected override string ThreadName => "Hardware Healer";

	protected override string JobName => "hardware-healer";

	protected override string JobLabel => "Hardware healer process";

	protected override ProgramPriority Priority => ProgramPriority.Highest;

	protected override int PollSleep => 1800000;

	protected override bool IsConfigured => ControllerConfiguration.Instance.EnableHardwareHealing;

	protected override bool CoreExecute()
	{
		ExecutionContext executionContext = ScheduleAndBlockOnJob();
		executionContext.Trash();
		return true;
	}

	internal override void OnEngineModeChange(EngineModes newMode)
	{
		switch (newMode)
		{
		case EngineModes.Normal:
			Start();
			break;
		case EngineModes.Maintenance:
		case EngineModes.Diagnostic:
		case EngineModes.Merchandizing:
			Shutdown();
			break;
		}
	}

	private HardwareHealerPoller()
	{
	}
}
