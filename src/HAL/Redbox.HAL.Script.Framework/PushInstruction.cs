using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class PushInstruction : Instruction
{
	public override string Mnemonic => "PUSH";

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count == 0)
		{
			AddError("C002", "At least one immediate operand is expected.", result.Errors);
		}
		else
		{
			base.Operands.AddRange(tokens.GetTokensAfterMnemonic());
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		foreach (Token operand in base.Operands)
		{
			object obj = operand.ConvertValue();
			if (operand.Type == TokenType.Symbol && operand.Value.Equals(obj))
			{
				obj = ExecutionEngine.Instance.ActiveContext.GetSymbolValue(operand.Value, result.Errors);
				if (obj == null)
				{
					break;
				}
			}
			else
			{
				obj = operand.ConvertValue();
			}
			ExecutionEngine.Instance.ActiveContext.PushTop(obj);
		}
	}
}
