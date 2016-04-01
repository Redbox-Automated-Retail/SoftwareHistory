using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "exclude-locations", Operand = "EXCLUDE-LOCATIONS")]
internal sealed class ExcludeLocationsJob : NativeJobAdapter
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
			IDeck byNumber = base.DecksService.GetByNumber(top2);
			if (byNumber != null && byNumber.IsSlotValid(top3))
			{
				if (!dictionary.ContainsKey(top2))
				{
					dictionary[top2] = new List<int>();
				}
				dictionary[top2].Add(top3);
				num++;
			}
		}
		foreach (int key in dictionary.Keys)
		{
			List<int> list = dictionary[key];
			IDeck byNumber2 = base.DecksService.GetByNumber(key);
			base.InventoryService.ExcludeSlots(byNumber2, list);
			list.Clear();
		}
		dictionary.Clear();
		base.Context.CreateInfoResult("TotalLocationsExcluded", num.ToString());
	}

	internal ExcludeLocationsJob(ExecutionResult r, ExecutionContext c)
		: base(r, c)
	{
	}
}
