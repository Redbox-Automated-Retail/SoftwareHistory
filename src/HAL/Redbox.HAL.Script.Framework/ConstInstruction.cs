using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class ConstInstruction : Instruction
{
	public override string Mnemonic => "CONST";

	internal override bool CreatesVariableDef => true;

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count != 2)
		{
			AddError("C002", "Expected a symbol operand with a value.", result.Errors);
			return;
		}
		bool flag = true;
		if (tokensAfterMnemonic[0].Type != TokenType.ConstSymbol)
		{
			AddError("C002", "Expected first operand to be a symbol operand.", result.Errors);
			flag = false;
		}
		if (tokensAfterMnemonic[1].Type != TokenType.StringLiteral && tokensAfterMnemonic[1].Type != TokenType.NumericLiteral)
		{
			AddError("C002", "Expected second operand to be a string literal or numeric literal.", result.Errors);
			flag = false;
		}
		if (flag)
		{
			base.Operands.AddRange(tokensAfterMnemonic);
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		Token token = base.Operands[0];
		Token token2 = base.Operands[1];
		new ErrorList();
		_ = token2.Value;
		if (token.Type == TokenType.ConstSymbol)
		{
			if (ExecutionEngine.Instance.ActiveContext.SymbolTable.TryGetValue(token.Value, out var _))
			{
				string text = "Attempt to redefine CONST " + token.Value;
				AddError("E002", text + "; not allowing update.", result.Errors);
			}
			else
			{
				object value2 = token2.ConvertValue();
				ExecutionEngine.Instance.ActiveContext.SetSymbolValue(token.Value, value2);
			}
		}
	}
}
