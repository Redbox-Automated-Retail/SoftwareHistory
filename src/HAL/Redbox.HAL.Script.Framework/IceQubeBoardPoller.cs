using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal class IceQubeBoardPoller : ScriptPoller
{
	internal static IceQubeBoardPoller Instance => Singleton<IceQubeBoardPoller>.Instance;

	protected override string ThreadName => "IceQube Board Poller";

	protected override ProgramPriority Priority => ProgramPriority.High;

	protected override string JobLabel => "None";

	protected override string JobName => "check-iceqube-board";

	protected override bool IsConfigured => IceQubeBoard.Instance.Configured;

	private int PollInterval { get; set; }

	protected override void ExecuteInner()
	{
		if (!IceQubeBoard.Instance.Configured)
		{
			LogHelper.Instance.Log("The IceQubeBoard is not configured - bail poller.", LogEntryType.Info);
			Shutdown(joinThread: false);
			return;
		}
		while (!PollEvent.WaitOne(PollInterval))
		{
			ExecutionContext executionContext = ScheduleAndBlockOnJob();
			if (executionContext.Status == ExecutionContextStatus.Errored)
			{
				LogHelper.Instance.Log(LogEntryType.Error, "Query job ( ID {0} ) errored; halt polling.", executionContext.ID);
				Shutdown(joinThread: false);
			}
			executionContext.Trash();
		}
	}

	private IceQubeBoardPoller()
	{
		PollInterval = RegistryOptions.Instance.GetSecretValue<int?>("SOFTWARE\\Redbox\\HAL", "IceQubeSleepTime") ?? 600000;
	}
}
