using System;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class FindEmptyInstruction : Instruction
{
	protected static class ResultCodes
	{
		public const string MachineFull = "MACHINE FULL";

		public const string Found = "FOUND";
	}

	public override string Mnemonic => "FINDEMPTY";

	protected override ExpectedOperands Expected => ExpectedOperands.None;

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		using EmptySearchResult emptySearchResult = new EmptySearchResult();
		if (!emptySearchResult.FindOneEmpty())
		{
			context.AppLog.LogMachineFull();
			context.PushTop("MACHINE FULL");
			return;
		}
		Location location = emptySearchResult.EmptyLocations[0];
		context.PushTop(location.Slot);
		context.PushTop(location.Deck);
		context.PushTop("FOUND");
	}
}
