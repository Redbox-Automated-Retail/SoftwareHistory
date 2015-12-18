using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "reset-locations", Operand = "RESET-LOCATIONS")]
internal sealed class ResetLocationsJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		int top = GetTop<int>();
		int num = 0;
		for (int i = 0; i < top; i++)
		{
			int top2 = GetTop<int>();
			int top3 = GetTop<int>();
			Deck deck = DecksManager.Instance.GetDeck(top2);
			if (!(deck == null) && deck.IsSlotValid(top3))
			{
				Location location = new Location(top2, top3);
				location.Reset();
				num++;
				base.Context.CreateResult("LocationReset", "The location was reset.", top2, top3, null, null, null);
			}
		}
		base.Context.CreateInfoResult("TotalLocationsReset", num.ToString());
	}

	internal ResetLocationsJob(ExecutionResult r, ExecutionContext c)
		: base(r, c)
	{
	}
}
