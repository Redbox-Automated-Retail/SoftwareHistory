using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class HomeYInstruction : Instruction
{
	public override string Mnemonic => "HOMEY";

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count != 0)
		{
			if (tokensAfterMnemonic.Count == 1 && tokensAfterMnemonic[0].IsKeyValuePair && IsOperandValid(tokensAfterMnemonic[0].Value))
			{
				base.Operands.AddRange(tokensAfterMnemonic);
			}
			else
			{
				AddError("C002", "The HOMEY mnemonic supports the CLEARGRIPPER key value pair.", result.Errors);
			}
		}
	}

	protected override void OnExecute(ExecutionResult result)
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
			Gripper.Instance.ClearFront();
		}
		MotionResult motionResult = ControllerService.Instance.HomeAxis(Axis.Y, ExecutionEngine.Instance.ActiveContext.AppLog);
		string instance = motionResult.ToString().ToUpper();
		ExecutionEngine.Instance.ActiveContext.PushTop(instance);
	}

	private bool IsOperandValid(string op)
	{
		return op.StartsWith("CLEARGRIPPER");
	}
}
