using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class MoveVendInstruction : Instruction
{
	protected static class KeyNames
	{
		public const string Mode = "MODE";

		public const string ResetOnCommunicationFailure = "RESETONCOMMERROR";
	}

	public override string Mnemonic => "MOVEVEND";

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count != 0)
		{
			if (tokensAfterMnemonic.Count <= 2)
			{
				base.Operands.AddRange(tokensAfterMnemonic);
			}
			else
			{
				AddError("C002", "Expected either no arguments, or single key value pair MODE=GET.", result.Errors);
			}
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		MoveMode mode = MoveMode.None;
		if (base.Operands.Count > 0)
		{
			KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("MODE");
			if (keyValuePair != null)
			{
				mode = Enum<MoveMode>.ParseIgnoringCase(keyValuePair.Value, MoveMode.None);
			}
		}
		KeyValuePair keyValuePair2 = base.Operands.GetKeyValuePair("RESETONCOMMERROR");
		if (keyValuePair2 != null)
		{
			new ErrorList();
			bool.TryParse(keyValuePair2.Value, out var _);
		}
		MotionResult motionResult = ControllerService.Instance.MoveToVend(mode, ExecutionEngine.Instance.ActiveContext.AppLog);
		string instance = motionResult.ToString().ToUpper();
		ExecutionEngine.Instance.ActiveContext.PushTop(instance);
	}
}
