using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class HomeXInstruction : Instruction
{
	public override string Mnemonic => "HOMEX";

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		if (tokens.GetTokensAfterMnemonic().Count > 0)
		{
			AddError("C002", "The HOMEX mnemonic does not support operands.", result.Errors);
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		MotionResult motionResult = ControllerService.Instance.HomeAxis(Axis.X, ExecutionEngine.Instance.ActiveContext.AppLog);
		string instance = motionResult.ToString().ToUpper();
		ExecutionEngine.Instance.ActiveContext.PushTop(instance);
	}
}
