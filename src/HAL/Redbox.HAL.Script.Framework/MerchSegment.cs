using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal abstract class MerchSegment
{
	internal virtual bool CanDump => false;

	internal MerchFlags Flags { get; private set; }

	internal bool Debug { get; private set; }

	protected internal virtual MerchFlags NextHigher => MerchFlags.None;

	protected internal virtual MerchFlags NextLower => MerchFlags.None;

	protected MerchSegment(MerchFlags flags, bool debug)
	{
		Flags = flags;
		Debug = debug;
		if (Debug)
		{
			LogHelper.Instance.Log("Merchdata dump for [Context = {0}]", Flags.ToString());
			LogHelper.Instance.Log(" Lower priority: {0} Higher Priority = {1}", NextLower.ToString(), NextHigher.ToString());
		}
	}

	internal int ItemCount()
	{
		VMZSearchResult high = default(VMZSearchResult);
		VMZSearchResult low = default(VMZSearchResult);
		ComputeBounds(ref low, ref high);
		if (!high.Found)
		{
			return 0;
		}
		int count = 0;
		VMZ.Instance.ForAllBetweenDo(delegate(Location loc)
		{
			if (loc.Flags == Flags)
			{
				count++;
			}
			return true;
		}, ref low, ref high);
		return count;
	}

	internal bool Contains(Location location)
	{
		VMZSearchResult high = default(VMZSearchResult);
		VMZSearchResult low = default(VMZSearchResult);
		ComputeBounds(ref low, ref high);
		if (!high.Found)
		{
			return false;
		}
		bool found = false;
		VMZ.Instance.ForAllBetweenDo(delegate(Location loc)
		{
			if (location.Equals(loc))
			{
				found = true;
				return false;
			}
			return true;
		}, ref low, ref high);
		return found;
	}

	internal Location GetEmpty()
	{
		VMZSearchResult high = default(VMZSearchResult);
		VMZSearchResult low = default(VMZSearchResult);
		ComputeBounds(ref low, ref high);
		Location empty = null;
		if (!high.Found)
		{
			return empty;
		}
		VMZ.Instance.ForAllBetweenDo(delegate(Location loc)
		{
			if (loc.IsEmpty)
			{
				empty = loc;
				return false;
			}
			return true;
		}, ref low, ref high);
		return empty;
	}

	internal void FindHigh(ref VMZSearchResult result)
	{
		VMZ.Instance.FindHighestLocation(Flags, ref result);
	}

	internal void FindLow(ref VMZSearchResult result)
	{
		VMZ.Instance.FindFirstLocation(Flags, ref result);
	}

	internal void ComputeBounds(ref VMZSearchResult low, ref VMZSearchResult high)
	{
		FindHigh(ref high);
		FindLow(ref low);
	}
}
