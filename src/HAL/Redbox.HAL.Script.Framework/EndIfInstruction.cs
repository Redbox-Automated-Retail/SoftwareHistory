using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class EndIfInstruction : Instruction
{
	public override bool EndsContext => true;

	public override string Mnemonic => "ENDIF";

	internal override Branch BranchType => Branch.Conditional;

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count > 0)
		{
			AddError("C002", "The ENDIF mnemonic does not support operands.", result.Errors);
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
	}
}
