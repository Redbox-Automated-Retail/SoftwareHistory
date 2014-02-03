using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class DecInstruction : Instruction
{
	public override string Mnemonic => "DEC";

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count == 0 || tokensAfterMnemonic.Count > 1)
		{
			AddError("C002", "Expectd a symbol to decrement.", result.Errors);
		}
		else if (tokensAfterMnemonic[0].Type == TokenType.ConstSymbol)
		{
			AddError("C002", "Illegal attempt to " + Mnemonic + " a CONST symbol.", result.Errors);
		}
		else
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
		object obj = ExecutionEngine.Instance.ActiveContext.GetSymbolValue(token.Value, result.Errors);
		if (obj == null || result.Errors.Count > 0)
		{
			return;
		}
		Type type = obj.GetType();
		if (type != typeof(int) && type != typeof(decimal))
		{
			AddError("E002", "Only numeric values of type integer and decimal may be decremented.", result.Errors);
			return;
		}
		if (type == typeof(int))
		{
			int num = (int)obj;
			num--;
			obj = num;
		}
		else if (type == typeof(decimal))
		{
			decimal num2 = (decimal)obj;
			--num2;
			obj = num2;
		}
		ExecutionEngine.Instance.ActiveContext.SetSymbolValue(token.Value, Convert.ChangeType(obj, type));
	}
}
