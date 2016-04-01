using System;
using System.Threading;
using Redbox.HAL.Component.Model;
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
		ExpectedOperand expectedOperand = Enum<ExpectedOperand>.ParseIgnoringCase(base.Operands[0].Value, ExpectedOperand.None);
		IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
		switch (expectedOperand)
		{
		case ExpectedOperand.None:
			context.ServiceLog("SerialBoardInstruction: converted '{0}' to operand None", base.Operands[0].Value);
			break;
		case ExpectedOperand.ClosePort:
			service.Shutdown();
			Thread.Sleep(500);
			context.PushTop("SUCCESS");
			break;
		case ExpectedOperand.Reset:
		{
			IControlResponse response = service.Initialize();
			HandleError(response, context);
			break;
		}
		}
	}

	private bool IsValidOperand(string operand)
	{
		return Enum<ExpectedOperand>.ParseIgnoringCase(operand, ExpectedOperand.None) != ExpectedOperand.None;
	}
}
