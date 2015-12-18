using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class PutInstruction : Instruction
{
	private static class KeyNames
	{
		public const string Peek = "PEEK";

		public const string HomeOnFailure = "HOMEONFAILURE";

		public const string MarkReturnTime = "MARKRETURNTIME";
	}

	public override string Mnemonic => "PUT";

	protected override ExpectedOperands Expected => ExpectedOperands.Optional;

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		if (context.StackDepth == 0)
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
		if (!(context.PopTop() is string id))
		{
			AddError("E004", "The PUT instruction expects a valid string literal ID on the stack.", result.Errors);
			return;
		}
		PutResult putResult = ControllerService.Instance.Put(id, context.AppLog);
		context.PushTop(putResult.ToString().ToUpper());
		context.PushTop(1);
	}
}
