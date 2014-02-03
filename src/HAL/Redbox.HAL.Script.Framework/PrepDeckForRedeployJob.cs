using System.Collections.Generic;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal class PrepDeckForRedeployJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		int top = GetTop<int>();
		int num = 0;
		bool flag = ControllerConfiguration.Instance.MerchandizingMode != MerchandizingMode.QLM;
		for (int i = 0; i < top; i++)
		{
			int top2 = GetTop<int>();
			Deck deck = DecksManager.Instance.GetDeck(top2);
			if (deck == null)
			{
				LogHelper.Instance.Log("Invalid deck '{0}' specified to prep redeploy deck job.", top2);
				continue;
			}
			if (!flag && deck.IsQlmDeck)
			{
				LogHelper.Instance.Log("Deck specified '{0}' is QLM; will not alter.", top2);
				continue;
			}
			int start = ((!flag) ? 1 : VMZ.Instance.EndsWithSlot(deck));
			Range range = new Range(start, deck.NumberOfSlots);
			List<int> list = deck.SwapEmptyWith("UNKNOWN", MerchFlags.None, range);
			foreach (int item in list)
			{
				ResultHelper.Instance.CreateResult("LocationChanged", "The location was set to UNKNOWN", top2, item, null, null, null);
			}
			num += list.Count;
			LogHelper.Instance.Log("Inventory: changed {0} empty slots on deck {1} to UNKNOWN", list.Count, top2);
			list.Clear();
		}
		ResultHelper.Instance.CreateInfoResult("LocationChangedCount", num.ToString());
	}
}
