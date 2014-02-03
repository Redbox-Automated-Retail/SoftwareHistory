using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal sealed class AirExchangerStatusJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		if (!IceQubeBoard.Instance.Configured)
		{
			ResultHelper.Instance.CreateInfoResult("AirExchangerNotConfigured", "There is no air exchanger configured.");
			return;
		}
		ResultHelper.Instance.CreateInfoResult("AirExchangerConfigured", "There is an air exchanger configured.");
		if (!IceQubeBoard.Instance.ShouldRetry())
		{
			ResultHelper.Instance.CreateInfoResult("PersistentErrorStatus", "The board is in an persistent error state.");
		}
		else
		{
			ResultHelper.Instance.CreateInfoResult("BoardStatusOk", "The board is not in a persistent error state.");
		}
	}
}
