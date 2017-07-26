using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[Poller(Name = "HardwareHealer")]
internal class HardwareHealerPoller : ScriptPoller, IConfigurationObserver
{
	private bool m_configured;

	private int m_pauseTime = 900000;

	protected override string ThreadName => "Hardware Healer";

	protected override string JobName => "hardware-healer";

	protected override string JobLabel => "Hardware healer process";

	protected override ProgramPriority Priority => ProgramPriority.Highest;

	protected override int PollSleep => m_pauseTime;

	protected override bool IsConfigured => m_configured;

	public void NotifyConfigurationLoaded()
	{
		LogHelper.Instance.Log("[HardwareHealer] Configuration loaded.");
		m_configured = ControllerConfiguration.Instance.EnableHardwareHealing;
		m_pauseTime = ControllerConfiguration.Instance.HardwareHealerPauseTime;
	}

	public void NotifyConfigurationChangeStart()
	{
		LogHelper.Instance.Log("[HardwareHealer] Configuration change start.");
		Shutdown();
	}

	public void NotifyConfigurationChangeEnd()
	{
		LogHelper.Instance.Log("[HardwareHealer] Configuration change end.");
		m_configured = ControllerConfiguration.Instance.EnableHardwareHealing;
		m_pauseTime = ControllerConfiguration.Instance.HardwareHealerPauseTime;
		if (EngineModes.Normal == ExecutionEngine.Instance.Mode)
		{
			Start();
		}
	}

	protected override bool CoreExecute()
	{
		ScheduleAndBlockOnJob().Trash();
		return true;
	}

	internal override void Initialize()
	{
		ServiceLocator.Instance.GetService<IConfigurationService>().FindConfiguration(Configurations.Controller).AddObserver(this);
		ServiceLocator.Instance.GetService<IExecutionService>().AddObserver(this);
	}

	private HardwareHealerPoller()
	{
	}
}
