using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class ElseInstruction : Instruction
{
	public override string Mnemonic => "ELSE";

	internal override Branch BranchType => Branch.Conditional;

	internal override bool ValidateConditionalBranch(Instruction target, ExecutionResult result)
	{
		if (target.Mnemonic.Equals("ENDIF"))
		{
			return true;
		}
		LogIncorrectBranch(target.Mnemonic, result);
		return false;
	}

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
