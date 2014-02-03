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
		FanStatus
	}

	public override string Mnemonic => "AIRXCHGR";

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
			ServiceLogHelper.Instance.WriteFormattedMessage(LogEntryType.Info, "{0} instruction: converted '{1}' to operand None", Mnemonic, base.Operands[0].Value);
			break;
		case ExpectedOperand.IsConfigured:
			ExecutionEngine.Instance.ActiveContext.PushTop(IceQubeBoard.Instance.Configured);
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
