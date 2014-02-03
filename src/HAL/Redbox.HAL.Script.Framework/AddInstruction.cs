using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class AddInstruction : Instruction
{
	public override string Mnemonic => "ADD";

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count == 0 || tokensAfterMnemonic.Count > 2)
		{
			AddError("C002", "Expectd a symbol as target of the addition and either a numeric literal or symbol as the addend.", result.Errors);
		}
		else if (tokensAfterMnemonic[0].Type != TokenType.Symbol)
		{
			AddError("C002", "Expectd a (non-CONST) symbol as target of the addition operation.", result.Errors);
		}
		else if (tokensAfterMnemonic[1].Type != TokenType.NumericLiteral && tokensAfterMnemonic[1].Type != TokenType.Symbol && tokensAfterMnemonic[1].Type != TokenType.ConstSymbol)
		{
			AddError("C002", "Expectd either a numeric literal or (CONST) symbol as the addend.", result.Errors);
		}
		else
		{
			base.Operands.AddRange(tokensAfterMnemonic);
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		Token token = base.Operands[0];
		object obj = ExecutionEngine.Instance.ActiveContext.GetSymbolValue(token.Value, result.Errors);
		if (obj == null || result.Errors.Count > 0)
		{
			return;
		}
		object obj2 = 0;
		Token token2 = base.Operands[1];
		if (token2.Type == TokenType.NumericLiteral)
		{
			obj2 = token2.ConvertValue();
		}
		else if (token2.Type == TokenType.Symbol || token2.Type == TokenType.ConstSymbol)
		{
			obj2 = ExecutionEngine.Instance.ActiveContext.GetSymbolValue(token2.Value, result.Errors);
			if (obj2 == null || result.Errors.Count > 0)
			{
				return;
			}
		}
		Type type = obj.GetType();
		if (type != typeof(int) && type != typeof(decimal))
		{
			AddError("E002", "Only numeric values of type integer and decimal may be added.", result.Errors);
			return;
		}
		if (type == typeof(int))
		{
			obj = (int)decimal.Add(Convert.ToDecimal(obj), Convert.ToDecimal(obj2));
		}
		else if (type == typeof(decimal))
		{
			obj = decimal.Add((decimal)obj, (decimal)obj2);
		}
		ExecutionEngine.Instance.ActiveContext.SetSymbolValue(token.Value, obj);
	}
}
