using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class SerialBoardInstruction : Instruction
{
	private enum ExpectedOperand
	{
		None,
		ClosePort,
		Reset
	}

	public override string Mnemonic => "SERIALBOARD";

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		switch (Enum<ExpectedOperand>.ParseIgnoringCase(base.Operands[0].Value, ExpectedOperand.None))
		{
		case ExpectedOperand.None:
			context.ServiceLog("SerialBoardInstruction: converted '{0}' to operand None", base.Operands[0].Value);
			break;
		case ExpectedOperand.ClosePort:
			context.PushTop(ControlSystem.Instance.ClosePort(500) ? "SUCCESS" : "FAILURE");
			break;
		case ExpectedOperand.Reset:
			HandleTimeoutOrCommmError(ControlSystem.Instance.Initialize(), context);
			break;
		}
	}

	private bool IsValidOperand(string operand)
	{
		return Enum<ExpectedOperand>.ParseIgnoringCase(operand, ExpectedOperand.None) != ExpectedOperand.None;
	}
}
