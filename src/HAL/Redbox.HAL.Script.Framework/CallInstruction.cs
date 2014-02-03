using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class CallInstruction : Instruction
{
	public override string Mnemonic => "CALL";

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count == 1)
		{
			if (tokensAfterMnemonic[0].Type != TokenType.StringLiteral)
			{
				AddError("C002", "Expected a string literal.", result.Errors);
			}
			else
			{
				base.Operands.AddRange(tokensAfterMnemonic);
			}
		}
		else
		{
			AddError("C002", "Expected a string literal.", result.Errors);
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		string value = base.Operands[0].Value;
		ProgramInstruction program = ExecutionEngine.Instance.ActiveContext.GetProgram(value);
		if (program == null)
		{
			AddError("E002", $"The program '{value}' was not found in the execution context program cache.", result.Errors);
			return;
		}
		ExecutionEngine.Instance.ActiveContext.Registers.PushCallFrame(value, program);
		result.RestartAtCurrentIP = true;
	}
}
