using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class HomeXInstruction : Instruction
{
	public override string Mnemonic => "HOMEX";

	protected override ExpectedOperands Expected => ExpectedOperands.None;

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		IMotionControlService service = ServiceLocator.Instance.GetService<IMotionControlService>();
		ErrorCodes errorCodes = service.HomeAxis(Axis.X);
		string instance = errorCodes.ToString().ToUpper();
		context.PushTop(instance);
	}
}
