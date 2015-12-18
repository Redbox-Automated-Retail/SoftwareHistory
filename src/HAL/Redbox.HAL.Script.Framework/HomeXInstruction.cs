using System;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class HomeXInstruction : Instruction
{
	public override string Mnemonic => "HOMEX";

	protected override ExpectedOperands Expected => ExpectedOperands.None;

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		MotionResult motionResult = ControllerService.Instance.HomeAxis(Axis.X, context.AppLog);
		string instance = motionResult.ToString().ToUpper();
		context.PushTop(instance);
	}
}
