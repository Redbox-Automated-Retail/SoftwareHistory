using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[Poller(Name = "HardwareHealer")]
internal class HardwareHealerPoller : ScriptPoller, IConfigurationObserver
{
	private bool m_configured;

	protected override string ThreadName => "Hardware Healer";

	protected override string JobName => "hardware-healer";

	protected override string JobLabel => "Hardware healer process";

	protected override ProgramPriority Priority => ProgramPriority.Highest;

	protected override int PollSleep => 1800000;

	protected override bool IsConfigured => m_configured;

	public void NotifyConfigurationLoaded()
	{
		LogHelper.Instance.Log("[HardwareHealer] Configuration loaded.");
		m_configured = ControllerConfiguration.Instance.EnableHardwareHealing;
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
		if (EngineModes.Normal == ExecutionEngine.Instance.Mode)
		{
			Start();
		}
	}

	protected override bool CoreExecute()
	{
		ExecutionContext executionContext = ScheduleAndBlockOnJob();
		executionContext.Trash();
		return true;
	}

	internal override void Initialize()
	{
		IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
		IAttributeXmlConfiguration attributeXmlConfiguration = service.FindConfiguration(Configurations.Controller);
		attributeXmlConfiguration.AddObserver(this);
		IExecutionService service2 = ServiceLocator.Instance.GetService<IExecutionService>();
		service2.AddObserver(this);
	}

	private HardwareHealerPoller()
	{
	}
}
