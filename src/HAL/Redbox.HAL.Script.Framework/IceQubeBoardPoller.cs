using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Poller(Name = "IceQubeBoard")]
internal sealed class IceQubeBoardPoller : ScriptPoller
{
	protected override string ThreadName => "IceQube Board Poller";

	protected override ProgramPriority Priority => ProgramPriority.High;

	protected override string JobName => "check-iceqube-board";

	protected override bool IsConfigured => IceQubeBoard.Instance.Configured;

	protected override int PollSleep => 600000;

	internal static IceQubeBoardPoller Instance => PollerManager.Instance.Locate("IceQubeBoard") as IceQubeBoardPoller;

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

	internal override void OnConfigChangeComplete()
	{
		IceQubeBoard.Instance.ConfigurationUpdateFinish();
		if (EngineModes.Normal == ExecutionEngine.Instance.Mode)
		{
			Start();
		}
	}

	internal override void OnConfigChangeStart()
	{
		Shutdown();
		IceQubeBoard.Instance.ConfigurationChangeStart();
	}

	private IceQubeBoardPoller()
	{
	}
}
