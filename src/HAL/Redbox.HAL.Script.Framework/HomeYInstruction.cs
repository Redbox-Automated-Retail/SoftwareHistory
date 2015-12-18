using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class HomeYInstruction : Instruction
{
	public override string Mnemonic => "HOMEY";

	protected override ExpectedOperands Expected => ExpectedOperands.Optional;

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count != 0 && (operandTokens.Count != 1 || !operandTokens[0].IsKeyValuePair || !IsOperandValid(operandTokens[0].Value)))
		{
			result.AddInvalidOperandError("The HOMEY mnemonic supports the CLEARGRIPPER key value pair.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		bool flag = true;
		if (base.Operands.Count == 1)
		{
			KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("CLEARGRIPPER");
			if (keyValuePair != null && bool.TryParse(keyValuePair.Value, out var result2))
			{
				flag = result2;
			}
		}
		if (flag)
		{
			Gripper.ClearFront();
		}
		MotionResult motionResult = ControllerService.Instance.HomeAxis(Axis.Y, context.AppLog);
		string instance = motionResult.ToString().ToUpper();
		context.PushTop(instance);
	}

	private bool IsOperandValid(string op)
	{
		return op.StartsWith("CLEARGRIPPER");
	}
}
