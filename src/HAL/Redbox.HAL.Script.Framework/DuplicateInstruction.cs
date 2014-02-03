using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class DuplicateInstruction : Instruction
{
	public override string Mnemonic => "DUP";

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count != 0)
		{
			AddError("C002", "Expected no operand or a symbol operand for DUP.", result.Errors);
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		object obj = null;
		if (ExecutionEngine.Instance.ActiveContext.StackDepth == 0)
		{
			AddError("E002", "There needs to be at least one argument on the stack.", result.Errors);
			return;
		}
		obj = ExecutionEngine.Instance.ActiveContext.PopTop();
		ExecutionEngine.Instance.ActiveContext.PushTop(obj);
		ExecutionEngine.Instance.ActiveContext.PushTop(obj);
	}
}
