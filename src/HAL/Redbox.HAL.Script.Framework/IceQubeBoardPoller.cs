using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[Poller(Name = "IceQubeBoard", ConfigNotifications = true)]
internal sealed class IceQubeBoardPoller : ScriptPoller
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

	private IceQubeBoardPoller()
	{
	}
}
