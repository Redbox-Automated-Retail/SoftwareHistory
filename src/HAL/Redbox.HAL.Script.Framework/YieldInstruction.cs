using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class YieldInstruction : Instruction
{
	public override string Mnemonic => "YIELD";

	public override bool IsCooperativeMultitask => true;

	protected override bool IsOperandRecognized(string operand)
	{
		return "RUNTIME-ONLY".Equals(operand);
	}

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count != 0)
		{
			if (tokensAfterMnemonic[0].Type != TokenType.NumericLiteral && !tokensAfterMnemonic[0].IsSymbolOrConst)
			{
				AddError("C002", "Expected a numeric literal.", result.Errors);
			}
			else
			{
				base.Operands.AddRange(tokensAfterMnemonic);
			}
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		if (base.Operands.Count == 1)
		{
			double? num = null;
			if (base.Operands[0].IsSymbolOrConst)
			{
				object symbolValue = ExecutionEngine.Instance.ActiveContext.GetSymbolValue(base.Operands[0].Value, result.Errors);
				if (symbolValue == null || result.Errors.Count > 0)
				{
					return;
				}
				if (symbolValue is int || symbolValue is decimal)
				{
					num = (double)Convert.ChangeType(symbolValue, typeof(double));
				}
			}
			else if (base.Operands[0].Type == TokenType.NumericLiteral)
			{
				num = (double)Convert.ChangeType(base.Operands[0].ConvertValue(), typeof(double));
			}
			if (num.HasValue)
			{
				DateTime value = DateTime.Now.AddMilliseconds(num.Value);
				ExecutionEngine.Instance.ActiveContext.StartTime = value;
			}
		}
		ExecutionEngine.Instance.PerformContextSwitch(runtimeOnly: false);
	}
}
