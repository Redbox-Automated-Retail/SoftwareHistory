using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[Poller(Name = "IceQubeBoard")]
internal sealed class IceQubeBoardPoller : ScriptPoller, IConfigurationObserver
{
	protected override string ThreadName => "IceQube Board Poller";

	protected override ProgramPriority Priority => ProgramPriority.High;

	protected override string JobName => "check-iceqube-board";

	protected override bool IsConfigured
	{
		get
		{
			IAirExchangerService service = ServiceLocator.Instance.GetService<IAirExchangerService>();
			return service.Configured;
		}
	}

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
		IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
		IAttributeXmlConfiguration attributeXmlConfiguration = service.FindConfiguration(Configurations.Controller);
		attributeXmlConfiguration.AddObserver(this);
		IExecutionService service2 = ServiceLocator.Instance.GetService<IExecutionService>();
		service2.AddObserver(this);
	}

	private IceQubeBoardPoller()
	{
	}
}
