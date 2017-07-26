using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[Poller(Name = "IceQubeBoard")]
internal sealed class IceQubeBoardPoller : ScriptPoller, IConfigurationObserver
{
	protected override string ThreadName => "IceQube Board Poller";

	protected override ProgramPriority Priority => ProgramPriority.High;

	protected override string JobName => "check-iceqube-board";

	protected override bool IsConfigured => ServiceLocator.Instance.GetService<IAirExchangerService>().Configured;

	protected override int PollSleep => 600000;

	public void NotifyConfigurationLoaded()
	{
		LogHelper.Instance.Log("[IceQubePoller] Configuration loaded.");
	}

	public void NotifyConfigurationChangeStart()
	{
		LogHelper.Instance.Log("[IceQubePoller] Configuration change start.");
		Shutdown();
	}

	public void NotifyConfigurationChangeEnd()
	{
		LogHelper.Instance.Log("[IceQubePoller] Configuration change end.");
		if (EngineModes.Normal == ExecutionEngine.Instance.Mode)
		{
			Start();
		}
	}

	protected override bool CoreExecute()
	{
		ExecutionContext executionContext = ScheduleAndBlockOnJob();
		if (executionContext.Status == ExecutionContextStatus.Errored)
		{
			LogHelper.Instance.Log(LogEntryType.Error, "Query job ( ID {0} ) errored; halt polling.", executionContext.ID);
			return false;
		}
		executionContext.Trash();
		return true;
	}

	internal override void Initialize()
	{
		ServiceLocator.Instance.GetService<IConfigurationService>().FindConfiguration(Configurations.Controller).AddObserver(this);
		ServiceLocator.Instance.GetService<IExecutionService>().AddObserver(this);
	}

	private IceQubeBoardPoller()
	{
	}
}
