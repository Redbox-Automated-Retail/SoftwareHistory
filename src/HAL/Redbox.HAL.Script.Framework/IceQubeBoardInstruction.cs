using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class IceQubeBoardInstruction : Instruction
{
	private enum ExpectedOperand
	{
		None,
		BoardStatus,
		Reset,
		HaltPoll,
		RestartPoll,
		PollStatus,
		IsConfigured,
		Enable,
		Disable,
		ResetOn,
		ResetOff,
		ResetFailureCounter,
		LatchOff,
		LatchOn,
		FanOn,
		FanOff,
		FanStatus
	}

	public override string Mnemonic => "ICEQUBEBOARD";

	protected override bool IsOperandRecognized(string operand)
	{
		return IsValidOperand(operand);
	}

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count <= 0)
		{
			result.Errors.Add(Error.NewError("C002", "Expected one operand.", "Correct the statement and recompile."));
		}
		else if (IsValidOperand(tokensAfterMnemonic[0].Value))
		{
			base.Operands.AddRange(tokensAfterMnemonic);
		}
		else
		{
			result.Errors.Add(Error.NewError("C002", $"Unexpected operand '{tokensAfterMnemonic[0].Value}'", "Correct the statement and recompile."));
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		switch (ConvertToOperand(base.Operands[0].Value))
		{
		case ExpectedOperand.None:
			ServiceLogHelper.Instance.WriteFormattedMessage(LogEntryType.Info, "IceQubeBoardInstruction instruction: converted '{0}' to operand None", base.Operands[0].Value);
			break;
		case ExpectedOperand.BoardStatus:
			ExecutionEngine.Instance.ActiveContext.PushTop(IceQubeBoard.Instance.CheckStatus().ToString().ToUpper());
			break;
		case ExpectedOperand.Reset:
			ExecutionEngine.Instance.ActiveContext.PushTop(IceQubeBoard.Instance.Reset().ToString().ToUpper());
			break;
		case ExpectedOperand.HaltPoll:
			IceQubeBoardPoller.Instance.Shutdown(joinThread: false);
			break;
		case ExpectedOperand.RestartPoll:
			IceQubeBoardPoller.Instance.Start();
			break;
		case ExpectedOperand.PollStatus:
			if (!IceQubeBoard.Instance.Configured)
			{
				ExecutionEngine.Instance.ActiveContext.PushTop("INACTIVE");
			}
			else
			{
				ExecutionEngine.Instance.ActiveContext.PushTop(IceQubeBoardPoller.Instance.Running ? "ACTIVE" : "INACTIVE");
			}
			break;
		case ExpectedOperand.IsConfigured:
			ExecutionEngine.Instance.ActiveContext.PushTop(IceQubeBoard.Instance.Configured);
			break;
		case ExpectedOperand.Enable:
			IceQubeBoard.Instance.Configured = true;
			ExecutionEngine.Instance.ActiveContext.PushTop(IceQubeBoard.Instance.Configured);
			break;
		case ExpectedOperand.Disable:
			IceQubeBoard.Instance.TurnOnFan();
			IceQubeBoard.Instance.Configured = false;
			IceQubeBoardPoller.Instance.Shutdown(joinThread: false);
			ExecutionEngine.Instance.ActiveContext.PushTop(IceQubeBoard.Instance.Configured);
			break;
		case ExpectedOperand.ResetOn:
			IceQubeBoard.Instance.LatchOn();
			break;
		case ExpectedOperand.ResetOff:
			IceQubeBoard.Instance.LatchOff();
			break;
		case ExpectedOperand.ResetFailureCounter:
			IceQubeBoard.Instance.ResetFailureCount();
			break;
		case ExpectedOperand.LatchOn:
			IceQubeBoard.Instance.LatchOn();
			break;
		case ExpectedOperand.LatchOff:
			IceQubeBoard.Instance.LatchOff();
			break;
		case ExpectedOperand.FanOn:
			IceQubeBoard.Instance.TurnOnFan();
			break;
		case ExpectedOperand.FanOff:
			IceQubeBoard.Instance.TurnOffFan();
			break;
		case ExpectedOperand.FanStatus:
			ExecutionEngine.Instance.ActiveContext.PushTop(IceQubeBoard.Instance.FanState.ToString().ToUpper());
			break;
		}
	}

	private bool IsValidOperand(string operand)
	{
		return ConvertToOperand(operand) != ExpectedOperand.None;
	}

	private ExpectedOperand ConvertToOperand(string operand)
	{
		ExpectedOperand expectedOperand = ExpectedOperand.None;
		try
		{
			return (ExpectedOperand)Enum.Parse(typeof(ExpectedOperand), operand, ignoreCase: true);
		}
		catch (ArgumentException)
		{
			return ExpectedOperand.None;
		}
	}
}
