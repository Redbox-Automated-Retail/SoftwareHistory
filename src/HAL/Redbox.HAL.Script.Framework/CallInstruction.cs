using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class CallInstruction : Instruction
{
	public override string Mnemonic => "CALL";

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count != 1 || operandTokens[0].Type != TokenType.StringLiteral)
		{
			result.AddInvalidOperandError($"{Mnemonic} Expected a string literal.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		string value = base.Operands[0].Value;
		ProgramInstruction program = context.GetProgram(value);
		if (program == null)
		{
			result.AddError(ExecutionErrors.MissingProgram, $"The program '{value}' was not found in the execution context program cache.");
			return;
		}
		context.Registers.PushCallFrame(value, program);
		result.RestartAtCurrentIP = true;
	}
}
