using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class NopInstruction : Instruction
{
	public override string Mnemonic => "NOP";

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count > 0)
		{
			AddError("C002", "The NOP mnemonic does not support operands.", result.Errors);
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
	}
}
