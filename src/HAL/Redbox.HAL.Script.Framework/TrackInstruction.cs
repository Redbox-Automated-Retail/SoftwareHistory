using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class TrackInstruction : Instruction
{
	private enum Commands
	{
		None,
		Open,
		Close,
		Status,
		Cycle
	}

	public override string Mnemonic => "TRACK";

	protected override bool IsOperandRecognized(string operand)
	{
		return IsValidOperand(operand);
	}

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count != 1)
		{
			result.AddMissingOperandError("Expected a symbol: OPEN, CLOSE, CYCLE or STATUS.");
		}
		else if (operandTokens[0].Type != TokenType.Symbol || !IsValidOperand(operandTokens[0].Value))
		{
			result.AddInvalidOperandError("Expected a symbol: OPEN, CLOSE, CYCLE or STATUS.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		_ = context.ProgramName;
		Token token = base.Operands[0];
		if (token.Type == TokenType.Symbol)
		{
			switch (Enum<Commands>.ParseIgnoringCase(token.Value, Commands.None))
			{
			case Commands.Status:
				context.PushTop(Track.IsTrackOpen(read: false) ? "OPEN" : "CLOSED");
				break;
			case Commands.Open:
				HandleTimeoutOrCommmError(Track.TestOpen(), context);
				break;
			case Commands.Close:
				HandleTimeoutOrCommmError(Track.TestClose(), context);
				break;
			case Commands.Cycle:
			{
				bool timedOut = Track.Cycle();
				context.PushTop(Instruction.ConvertTimeoutBoolToMessage(timedOut));
				break;
			}
			}
		}
	}

	private bool IsValidOperand(string operand)
	{
		Commands commands = Enum<Commands>.ParseIgnoringCase(operand, Commands.None);
		return commands != Commands.None;
	}
}
