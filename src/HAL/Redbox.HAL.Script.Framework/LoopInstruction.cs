using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class LoopInstruction : Instruction
{
	public override bool EndsContext => true;

	public override string Mnemonic => "LOOP";

	internal override Branch BranchType => Branch.Conditional;

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		if (tokens.GetTokensAfterMnemonic().Count > 0)
		{
			AddError("C002", "The LOOP mnemonic does not support operands.", result.Errors);
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		ExecutionEngine.Instance.ActiveContext.Registers.ActiveFrame.X--;
		if (ExecutionEngine.Instance.ActiveContext.Registers.ActiveFrame.X != 0)
		{
			ExecutionEngine.Instance.ActiveContext.Registers.ActiveFrame.ResetPC();
			result.RestartAtCurrentIP = true;
		}
	}
}
