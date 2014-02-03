using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class TimeStampInstruction : Instruction
{
	public override string Mnemonic => "TIMESTAMP";

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		if (tokens.GetTokensAfterMnemonic().Count > 0)
		{
			AddError("C002", "The TIMESTAMP mnemonic does not support operands.", result.Errors);
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		ExecutionEngine.Instance.ActiveContext.PushTop(DateTime.Now.ToString());
	}
}
