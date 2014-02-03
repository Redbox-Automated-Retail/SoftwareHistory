using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class SerialBoardInstruction : Instruction
{
	private enum ExpectedOperand
	{
		None,
		ClosePort,
		Reset
	}

	public override string Mnemonic => "SERIALBOARD";

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count == 0)
		{
			AddError("C002", "Expected one operand.", result.Errors);
		}
		else
		{
			base.Operands.AddRange(tokensAfterMnemonic);
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		switch (ConvertToOperand(base.Operands[0].Value))
		{
		case ExpectedOperand.None:
			ServiceLogHelper.Instance.WriteFormattedMessage(LogEntryType.Info, "SerialBoardInstruction: converted '{0}' to operand None", base.Operands[0].Value);
			break;
		case ExpectedOperand.ClosePort:
			ExecutionEngine.Instance.ActiveContext.PushTop(SerialBoard.Instance.ClosePort(3000) ? "SUCCESS" : "FAILURE");
			break;
		case ExpectedOperand.Reset:
			HandleTimeoutOrCommmError(SerialBoard.Instance.Reset());
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
