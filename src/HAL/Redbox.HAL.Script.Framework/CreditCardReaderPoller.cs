using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[Poller(Name = "CreditCardReaderPoller")]
internal sealed class CreditCardReaderPoller : ScriptPoller, IConfigurationObserver
{
	private int m_pollSleep;

	protected override string ThreadName => "CCR Watchdog";

	protected override ProgramPriority Priority => ProgramPriority.High;

	protected override string JobName => "test-and-reset-ccr";

	protected override bool IsConfigured => m_pollSleep > 0;

	protected override int PollSleep => m_pollSleep;

	public void NotifyConfigurationLoaded()
	{
		LogHelper.Instance.Log("[CreditCardReaderPoller] Configuration loaded.");
		m_pollSleep = ControllerConfiguration.Instance.CCRWatchdogSleep;
	}

	public void NotifyConfigurationChangeStart()
	{
		LogHelper.Instance.Log("[CreditCardReaderPoller] Configuration change start.");
		Shutdown();
	}

	public void NotifyConfigurationChangeEnd()
	{
		LogHelper.Instance.Log("[CreditCardReaderPoller] Configuration change end.");
		m_pollSleep = ControllerConfiguration.Instance.CCRWatchdogSleep;
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

	private CreditCardReaderPoller()
	{
	}
}
