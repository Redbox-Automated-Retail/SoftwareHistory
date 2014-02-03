using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal sealed class ExcludeSlotsJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		int top = GetTop<int>();
		for (int i = 0; i < top; i++)
		{
			int top2 = GetTop<int>();
			int top3 = GetTop<int>();
			Location location = new Location(top2, top3);
			if (!DecksManager.Instance.IsValidLocation(location))
			{
				LogHelper.Instance.Log("The location {0} is invalid.", location.ToString());
			}
		}
	}
}
