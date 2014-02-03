using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class PopInstruction : Instruction
{
	public override string Mnemonic => "POP";

	internal override bool CreatesVariableDef => true;

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count >= 0 && tokensAfterMnemonic.Count <= 2)
		{
			if (tokensAfterMnemonic.Count >= 1 && !tokensAfterMnemonic[0].IsSymbolOrConst && tokensAfterMnemonic[0].Type != TokenType.NumericLiteral)
			{
				AddError("C002", "Expected no operand or a symbol operand, with an optional DEPTH=n key value pair.", result.Errors);
			}
			else
			{
				base.Operands.AddRange(tokensAfterMnemonic);
			}
		}
		else
		{
			AddError("C002", "Expected one symbol to receive top of stack, with an optional DEPTH=n key value pair.", result.Errors);
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		object value = null;
		int? num = GetKeyValuePairValue<int?>(base.Operands.GetKeyValuePair("DEPTH"), result.Errors);
		if (result.Errors.Count > 0)
		{
			return;
		}
		if (!num.HasValue)
		{
			num = 1;
		}
		if (ExecutionEngine.Instance.ActiveContext.StackDepth >= num)
		{
			for (int i = 0; i < num; i++)
			{
				value = ExecutionEngine.Instance.ActiveContext.PopTop();
			}
			if (base.Operands.Count != 0)
			{
				Token token = base.Operands[0];
				if (token.Type == TokenType.Symbol)
				{
					ExecutionEngine.Instance.ActiveContext.SetSymbolValue(token.Value, value);
				}
			}
		}
		else
		{
			AddError("E002", $"The specified DEPTH of {num} exceeds the current depth of the stack: {ExecutionEngine.Instance.ActiveContext.StackDepth}.", result.Errors);
		}
	}
}
