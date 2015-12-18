using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class AirExchangerInstruction : Instruction
{
	private enum ExpectedOperand
	{
		None,
		IsConfigured,
		FanOn,
		FanOff,
		FanStatus,
		Enable,
		Disable,
		ResetFailureCounter,
		BoardStatus,
		Reset
	}

	public override string Mnemonic => "AIRXCHGR";

	protected override bool IsOperandRecognized(string operand)
	{
		return IsValidOperand(operand);
	}

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (!IsValidOperand(operandTokens[0].Value))
		{
			result.AddInvalidOperandError($"Unexpected operand '{operandTokens[0].Value}'");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		switch (Enum<ExpectedOperand>.ParseIgnoringCase(base.Operands[0].Value, ExpectedOperand.None))
		{
		case ExpectedOperand.None:
			context.ServiceLog("{0} instruction: converted '{1}' to operand None", Mnemonic, base.Operands[0].Value);
			break;
		case ExpectedOperand.IsConfigured:
			context.PushTop(IceQubeBoard.Instance.Configured);
			break;
		case ExpectedOperand.FanOn:
			IceQubeBoard.Instance.TurnOnFan();
			break;
		case ExpectedOperand.FanOff:
			IceQubeBoard.Instance.TurnOffFan();
			break;
		case ExpectedOperand.Enable:
			IceQubeBoard.Instance.Configured = true;
			context.PushTop(IceQubeBoard.Instance.Configured);
			break;
		case ExpectedOperand.Disable:
			IceQubeBoard.Instance.TurnOnFan();
			IceQubeBoard.Instance.Configured = false;
			context.PushTop(IceQubeBoard.Instance.Configured);
			break;
		case ExpectedOperand.FanStatus:
			context.PushTop(IceQubeBoard.Instance.FanState.ToString().ToUpper());
			break;
		case ExpectedOperand.ResetFailureCounter:
			IceQubeBoard.Instance.ResetFailureCount();
			break;
		case ExpectedOperand.BoardStatus:
			context.PushTop(IceQubeBoard.Instance.CheckStatus().ToString().ToUpper());
			break;
		case ExpectedOperand.Reset:
			context.PushTop(IceQubeBoard.Instance.Reset().ToString().ToUpper());
			break;
		}
	}

	private bool IsValidOperand(string operand)
	{
		return Enum<ExpectedOperand>.ParseIgnoringCase(operand, ExpectedOperand.None) != ExpectedOperand.None;
	}
}
