using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class ResetInstruction : Instruction
{
	private enum Target
	{
		None,
		Serial
	}

	public override string Mnemonic => "RESET";

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.Operands.AddRange(tokens.GetTokensAfterMnemonic());
	}

	protected override void OnExecute(ExecutionResult result)
	{
		if (base.Operands.Count == 0)
		{
			ExecutionEngine.Instance.ActiveContext.PushTop(Instruction.ConvertTimeoutBoolToMessage(ControllerService.Instance.Reset()));
			return;
		}
		Token token = base.Operands[0];
		Target target;
		try
		{
			target = Enum<Target>.ParseIgnoringCase(token.Value, Target.None);
		}
		catch (Exception)
		{
			result.Errors.Add(Error.NewError("E001", "Unrecognized target option.", $"The option {token.Value} is not a target."));
			return;
		}
		if (target == Target.None)
		{
			result.Errors.Add(Error.NewError("E001", "Unrecognized target option.", $"The option {token.Value} is not a target."));
			return;
		}
		KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("CLOSEPORT");
		if (keyValuePair != null && bool.TryParse(keyValuePair.Value, out var result2) && result2)
		{
			SerialBoard.Instance.ClosePort(3000);
		}
		HandleTimeoutOrCommmError(SerialBoard.Instance.Reset());
	}
}
