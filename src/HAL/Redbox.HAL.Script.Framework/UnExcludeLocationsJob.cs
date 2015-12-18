using System.Collections.Generic;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "unexclude-locations", Operand = "UNEXCLUDE-LOCATIONS")]
internal sealed class UnExcludeLocationsJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		Dictionary<int, List<int>> dictionary = new Dictionary<int, List<int>>();
		int top = GetTop<int>();
		int num = 0;
		for (int i = 0; i < top; i++)
		{
			int top2 = GetTop<int>();
			int top3 = GetTop<int>();
			Deck deck = DecksManager.Instance.GetDeck(top2);
			if (!(deck == null) && deck.IsSlotValid(top3))
			{
				if (!dictionary.ContainsKey(top2))
				{
					dictionary[top2] = new List<int>();
				}
				dictionary[top2].Add(top3);
			}
		}
		foreach (int key in dictionary.Keys)
		{
			List<int> list = dictionary[key];
			Deck deck2 = DecksManager.Instance.GetDeck(key);
			deck2.UnExclude(list);
			list.Clear();
		}
		dictionary.Clear();
		base.Context.CreateInfoResult("TotalLocationsExcluded", num.ToString());
	}

	internal UnExcludeLocationsJob(ExecutionResult r, ExecutionContext c)
		: base(r, c)
	{
	}
}
