using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class VendDoorInstruction : Instruction
{
	private enum DoorCommands
	{
		None,
		Open,
		Rent,
		Close,
		Status
	}

	public override string Mnemonic => "VENDDOOR";

	protected override bool IsOperandRecognized(string operand)
	{
		return IsOperandValid(operand);
	}

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count != 1)
		{
			result.AddMissingOperandError("Expected a symbol: OPEN, CLOSE, RENT, or STATUS.");
		}
		else if (operandTokens[0].Type != TokenType.Symbol || !IsOperandValid(operandTokens[0].Value))
		{
			result.AddInvalidOperandError("Expected a symbol: OPEN, CLOSE, RENT, or STATUS.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		Token token = base.Operands[0];
		if (token.Type == TokenType.Symbol)
		{
			switch (Enum<DoorCommands>.ParseIgnoringCase(token.Value, DoorCommands.None))
			{
			case DoorCommands.Status:
				context.PushTop(VendDoor.IsClosed() ? "CLOSED" : "OPEN");
				break;
			case DoorCommands.Open:
				HandleTimeoutOrCommmError(VendDoor.TestOpen(), context);
				break;
			case DoorCommands.Rent:
				HandleTimeoutOrCommmError(VendDoor.TestRent(), context);
				break;
			case DoorCommands.Close:
				HandleTimeoutOrCommmError(VendDoor.TestClose(), context);
				break;
			default:
				result.AddError(ExecutionErrors.InvalidSymbolValue, $"The value {token.Value} is unrecognized.");
				break;
			}
		}
	}

	private bool IsOperandValid(string op)
	{
		return Enum<DoorCommands>.ParseIgnoringCase(op, DoorCommands.None) != DoorCommands.None;
	}
}
