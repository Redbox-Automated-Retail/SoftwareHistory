using System.Collections.Generic;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "reset-locations", Operand = "RESET-LOCATIONS")]
internal sealed class ResetLocationsJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		int top = GetTop<int>();
		List<ILocation> list = new List<ILocation>();
		using (new DisposeableList<ILocation>(list))
		{
			for (int i = 0; i < top; i++)
			{
				int top2 = GetTop<int>();
				int top3 = GetTop<int>();
				IDeck byNumber = base.DecksService.GetByNumber(top2);
				if (byNumber != null && byNumber.IsSlotValid(top3))
				{
					ILocation item = service.Get(top2, top3);
					list.Add(item);
					base.Context.CreateResult("LocationReset", "The location was reset.", top2, top3, null, null, null);
				}
			}
			service.Reset(list);
			base.Context.CreateInfoResult("TotalLocationsReset", list.Count.ToString());
		}
	}

	internal ResetLocationsJob(ExecutionResult r, ExecutionContext c)
		: base(r, c)
	{
	}
}
