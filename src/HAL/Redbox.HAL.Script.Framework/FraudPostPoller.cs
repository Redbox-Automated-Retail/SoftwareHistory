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
		ScheduleAndBlockOnJob().Trash();
		return true;
	}

	internal override void Initialize()
	{
		ServiceLocator.Instance.GetService<IConfigurationService>().FindConfiguration(Configurations.Controller).AddObserver(this);
		ServiceLocator.Instance.GetService<IExecutionService>().AddObserver(this);
	}

	private FraudPostPoller()
	{
	}
}
