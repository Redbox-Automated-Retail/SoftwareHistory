using Redbox.HAL.Client;

namespace Redbox.HAL.Common.GUI.Functions;

public class ExecuteStatementImmediateCommand : ImmediateCommand
{
	protected override void OnExecute(ImmediateCommandResult result)
	{
		if (base.Arguments.Count != 0 && !string.IsNullOrEmpty(base.Arguments[0]))
		{
			HardwareJob val = default(HardwareJob);
			result.CommandResult = ImmediateCommand.Service.ExecuteImmediate(base.Arguments[0], (int?)120000, ref val);
		}
	}
}
