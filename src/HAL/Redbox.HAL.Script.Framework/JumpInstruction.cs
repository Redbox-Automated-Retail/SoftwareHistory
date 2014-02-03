using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class JumpInstruction : Instruction
{
	public override string Mnemonic => "JUMP";

	internal override Branch BranchType => Branch.Unconditional;

	internal override string TargetLabel => base.Operands[0].Value;

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count == 1)
		{
			if (tokensAfterMnemonic[0].Type != TokenType.Symbol)
			{
				AddError("C002", "Expected a symbol that equates to a label name.", result.Errors);
			}
			else
			{
				base.Operands.AddRange(tokensAfterMnemonic);
			}
		}
		else
		{
			AddError("C002", "Expected a symbol that equates to a label name.", result.Errors);
		}
	}

	protected override bool CoreExecute(ExecutionResult result)
	{
		return true;
	}

	protected override void OnExecute(ExecutionResult result)
	{
		Token token = base.Operands[0];
		if (token.Type == TokenType.Symbol && !ExecutionEngine.Instance.ActiveContext.Registers.JumpToLabel(token.Value))
		{
			AddError("E002", "The label " + token.Value + " does not exist.", result.Errors);
		}
	}
}
