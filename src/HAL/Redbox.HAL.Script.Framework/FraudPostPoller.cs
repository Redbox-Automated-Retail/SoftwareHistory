using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[Poller(Name = "FraudPost")]
internal sealed class FraudPostPoller : ScriptPoller, IConfigurationObserver
{
	private bool m_configured;

	private int m_pollSleep = 3600000;

	protected override string ThreadName => "Fraud Sensor POST";

	protected override ProgramPriority Priority => ProgramPriority.High;

	protected override string JobName => "fraud-sensor-post-test";

	protected override bool IsConfigured
	{
		get
		{
			IDiskValidatorService service = ServiceLocator.Instance.GetService<IDiskValidatorService>();
			if (m_configured)
			{
				return service.IsConfigured;
			}
			return false;
		}
	}

	protected override int PollSleep => m_pollSleep;

	public void NotifyConfigurationLoaded()
	{
		LogHelper.Instance.Log("[FraudPostPoller] Configuration loaded.");
		m_configured = ControllerConfiguration.Instance.EnableFraudSensorCheck;
		m_pollSleep = ControllerConfiguration.Instance.FraudPostPollerPauseTime;
	}

	public void NotifyConfigurationChangeStart()
	{
		LogHelper.Instance.Log("[FraudPostPoller] Configuration change start.");
		Shutdown();
	}

	public void NotifyConfigurationChangeEnd()
	{
		LogHelper.Instance.Log("[FraudPostPoller] Configuration change end.");
		m_configured = ControllerConfiguration.Instance.EnableFraudSensorCheck;
		m_pollSleep = ControllerConfiguration.Instance.FraudPostPollerPauseTime;
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

	private FraudPostPoller()
	{
	}
}
