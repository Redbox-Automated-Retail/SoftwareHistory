using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class PutInstruction : Instruction
{
	protected static class KeyNames
	{
		public const string Peek = "PEEK";

		public const string HomeOnFailure = "HOMEONFAILURE";

		public const string MarkReturnTime = "MARKRETURNTIME";
	}

	public override string Mnemonic => "PUT";

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		base.Operands.AddRange(tokens.GetTokensAfterMnemonic());
	}

	protected override void OnExecute(ExecutionResult result)
	{
		if (ExecutionEngine.Instance.ActiveContext.StackDepth == 0)
		{
			AddError("E004", "The PUT instruction expects a valid string literal ID on the stack.", result.Errors);
			return;
		}
		KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("HOMEONFAILURE");
		if (keyValuePair != null)
		{
			bool.TryParse(keyValuePair.Value, out var _);
		}
		KeyValuePair keyValuePair2 = base.Operands.GetKeyValuePair("PEEK");
		if (keyValuePair2 != null)
		{
			bool.TryParse(keyValuePair2.Value, out var _);
		}
		if (!(ExecutionEngine.Instance.ActiveContext.PopTop() is string id))
		{
			AddError("E004", "The PUT instruction expects a valid string literal ID on the stack.", result.Errors);
			return;
		}
		PutResult putResult = ControllerService.Instance.Put(id, ExecutionEngine.Instance.ActiveContext.AppLog);
		ExecutionEngine.Instance.ActiveContext.PushTop(putResult.ToString().ToUpper());
		ExecutionEngine.Instance.ActiveContext.PushTop(1);
	}
}
