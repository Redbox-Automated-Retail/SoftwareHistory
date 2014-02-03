using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

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
			if (deck != null && deck.IsSlotValid(top3))
			{
				deck.ResetLocation(top3);
				num++;
				ResultHelper.Instance.CreateResult("LocationReset", "The location was reset.", top2, top3, null, null, null);
			}
		}
		ResultHelper.Instance.CreateInfoResult("TotalLocationsReset", num.ToString());
	}
}
