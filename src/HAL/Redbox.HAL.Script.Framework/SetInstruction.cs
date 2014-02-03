using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class SetInstruction : Instruction
{
	public override string Mnemonic => "SET";

	internal override bool CreatesVariableDef => true;

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count < 2 || tokensAfterMnemonic.Count > 2)
		{
			AddError("C002", "A symbol and one of: symbol, numeric literal, string literal or key value pair expected.", result.Errors);
		}
		else if (CheckAssignment(tokensAfterMnemonic[0].Type, result.Errors))
		{
			base.Operands.AddRange(tokensAfterMnemonic);
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		Token token = base.Operands[0];
		if (token.Type != TokenType.Symbol)
		{
			return;
		}
		object obj = base.Operands[1].ConvertValue();
		if (IsSymbolic(base.Operands[1]) && base.Operands[1].Value.Equals(obj))
		{
			obj = ExecutionEngine.Instance.ActiveContext.GetSymbolValue(base.Operands[1].Value, result.Errors);
			if (result.Errors.Count > 0)
			{
				return;
			}
		}
		ExecutionEngine.Instance.ActiveContext.SetSymbolValue(token.Value, obj);
	}
}
