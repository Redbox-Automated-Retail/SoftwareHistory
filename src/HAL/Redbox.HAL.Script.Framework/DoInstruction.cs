using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class DoInstruction : Instruction
{
	public override bool BeginsContext => true;

	public override string EndContextMnemonic => "LOOP";

	public override string Mnemonic => "DO";

	internal override bool ValidateConditionalBranch(Instruction target, ExecutionResult result)
	{
		if (!target.Mnemonic.Equals(EndContextMnemonic))
		{
			LogIncorrectBranch(target.Mnemonic, result);
			return false;
		}
		CounterSymbol = Mnemonic + base.LineNumber;
		target.FalseBranchTarget = TrueBranchTarget;
		target.CounterSymbol = CounterSymbol;
		return true;
	}

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count == 1)
		{
			if (tokensAfterMnemonic[0].Type != TokenType.NumericLiteral && !tokensAfterMnemonic[0].IsSymbolOrConst)
			{
				AddError("C002", "Expected a numeric literal or (CONST) symbol.", result.Errors);
			}
			else
			{
				base.Operands.AddRange(tokensAfterMnemonic);
			}
		}
		else
		{
			AddError("C002", "The DO mnemonic requires a single numeric literal or symbol.", result.Errors);
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		object obj = base.Operands[0].Value;
		if (base.Operands[0].Type == TokenType.Symbol)
		{
			obj = ExecutionEngine.Instance.ActiveContext.GetSymbolValue(base.Operands[0].Value, result.Errors);
			if (obj == null || result.Errors.Count > 0)
			{
				return;
			}
		}
		int num = Convert.ToInt32(obj);
		if (num > 0)
		{
			base.X = num;
			result.SwitchToContext = this;
		}
	}
}
