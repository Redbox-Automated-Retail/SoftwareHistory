using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal class TestIceQubeExchangerJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "Service", flushOnWrite: true, "IceQubeBoard");
		if (!IceQubeBoard.Instance.ShouldRetry())
		{
			applicationLog.Write("The board is in persistent error; no further queries.", LogEntryType.Info);
			AddError("IceQube board in persistent error state.");
			return;
		}
		IceQubeBoardStatus iceQubeBoardStatus = IceQubeBoard.Instance.CheckStatus();
		if (iceQubeBoardStatus != IceQubeBoardStatus.Error)
		{
			return;
		}
		applicationLog.Write("Board status in error; attempt reset.", LogEntryType.Info);
		IceQubeBoardStatus iceQubeBoardStatus2 = IceQubeBoardStatus.NotConfigured;
		for (int i = 0; i < 3; i++)
		{
			iceQubeBoardStatus2 = IceQubeBoard.Instance.Reset();
			applicationLog.Write($"Reset status is {iceQubeBoardStatus2.ToString().ToUpper()}");
			if (IceQubeBoardStatus.Ok == iceQubeBoardStatus2)
			{
				IceQubeBoard.Instance.ResetFailureCount();
				return;
			}
			PerfFunctions.Wait(2000);
		}
		if (IceQubeBoardStatus.Error == iceQubeBoardStatus2)
		{
			IceQubeBoard.Instance.IncrementResetFailureCount();
			if (!IceQubeBoard.Instance.ShouldRetry())
			{
				AddError("Unable to reset IceQube board.");
			}
		}
	}
}
