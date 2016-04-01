using System;
using Redbox.HAL.Component.Model;

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
		IEmptySearchPatternService service = ServiceLocator.Instance.GetService<IEmptySearchPatternService>();
		using IEmptySearchResult emptySearchResult = service.FindOneEmpty();
		if (emptySearchResult.FoundEmpty == 0)
		{
			context.AppLog.LogMachineFull();
			context.PushTop("MACHINE FULL");
			return;
		}
		ILocation location = emptySearchResult.EmptyLocations[0];
		context.PushTop(location.Slot);
		context.PushTop(location.Deck);
		context.PushTop("FOUND");
	}
}
