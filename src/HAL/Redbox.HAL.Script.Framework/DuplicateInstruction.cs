using System;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class DuplicateInstruction : Instruction
{
	public override string Mnemonic => "DUP";

	protected override ExpectedOperands Expected => ExpectedOperands.None;

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		object obj = null;
		if (context.StackDepth == 0)
		{
			AddError("E002", "There needs to be at least one argument on the stack.", result.Errors);
			return;
		}
		obj = context.PopTop();
		context.PushTop(obj);
		context.PushTop(obj);
	}
}
