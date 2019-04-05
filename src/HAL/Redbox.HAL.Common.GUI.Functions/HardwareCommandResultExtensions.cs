using Redbox.HAL.Client;
using Redbox.HAL.Component.Model;
using Redbox.IPC.Framework;

namespace Redbox.HAL.Common.GUI.Functions;

public static class HardwareCommandResultExtensions
{
	public static void Dump(this HardwareCommandResult result)
	{
		if (!((ClientCommandResult)(object)result).Success && ((ClientCommandResult)(object)result).Errors.Count > 0)
		{
			LogHelper.Instance.Log("\nClient Errors:");
			foreach (Error error in ((ClientCommandResult)(object)result).Errors)
			{
				LogHelper.Instance.Log(error.ToString());
				LogHelper.Instance.Log("Details: {0}\n", error.Details);
			}
		}
		if (((ClientCommandResult)(object)result).CommandMessages.Count > 0)
		{
			LogHelper.Instance.Log("Command Messages:");
			for (int i = 0; i < ((ClientCommandResult)(object)result).CommandMessages.Count; i++)
			{
				LogHelper.Instance.Log(((ClientCommandResult)(object)result).CommandMessages[i]);
			}
		}
		LogHelper.Instance.Log("Command {0}, execution time = {1}", ((ClientCommandResult)(object)result).Success ? "successfully executed" : "failed during execution", ((ClientCommandResult)(object)result).ExecutionTime);
	}
}
