using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal sealed class SegmentManager : IGetObserver
{
	internal readonly ReadOnlyCollection<MerchFlags> IncreasingPriorityOrder;

	internal readonly ReadOnlyCollection<MerchFlags> DecreasingPriorityOrder;

	private readonly VMZ VMZ;

	private readonly bool Debug;

	private readonly MerchFlags[] IncreasingPriorityFlags = new MerchFlags[3]
	{
		MerchFlags.Thin,
		MerchFlags.ThinRedeploy,
		MerchFlags.Rebalance
	};

	internal static SegmentManager Instance => Singleton<SegmentManager>.Instance;

	public void OnStuck(GetResult result, Deck deck, int slot)
	{
		deck.UpdateEmptyStuck(slot);
	}

	public bool OnEmpty(GetResult result, Deck deck, int slot)
	{
		result.UpdateCode(GetResultCode.ItemStuck);
		deck.UpdateEmptyStuck(slot);
		return false;
	}

	internal bool IsThinType(MerchFlags flags)
	{
		return !IsNonThinType(flags);
	}

	internal bool IsNonThinType(MerchFlags flags)
	{
		if (MerchFlags.None != flags)
		{
			return MerchFlags.Unload == flags;
		}
		return true;
	}

	internal List<Location> GetNonThinItemsInCompressedZone()
	{
		List<Location> rv = new List<Location>();
		VMZSearchResult high = default(VMZSearchResult);
		FindHighest(ref high);
		if (!high.Found)
		{
			return rv;
		}
		VMZ.Instance.ForAllBetweenDo(delegate(Location loc)
		{
			if (loc.IsEmpty)
			{
				return true;
			}
			if (IsNonThinType(loc.Flags))
			{
				rv.Add(loc);
			}
			return true;
		}, ref high);
		return rv;
	}

	internal bool InCompressedZone(Location target)
	{
		VMZSearchResult high = default(VMZSearchResult);
		FindHighest(ref high);
		if (!high.Found)
		{
			return false;
		}
		bool found = false;
		VMZ.Instance.ForAllBetweenDo(delegate(Location loc)
		{
			if (loc.Equals(target))
			{
				found = true;
				return false;
			}
			return true;
		}, ref high);
		return found;
	}

	internal void FindHighest(ref VMZSearchResult high)
	{
		foreach (MerchFlags item in IncreasingPriorityOrder)
		{
			MerchSegment merchSegment = MerchSegmentFactory.Get(item);
			merchSegment.FindHigh(ref high);
			if (high.Found)
			{
				return;
			}
		}
		high.Invalidate();
	}

	internal Location SelectLocation(MerchFlags flags, MerchTransferManager manager, out MerchandizeResult result)
	{
		result = MerchandizeResult.Success;
		VMZSearchResult result2 = default(VMZSearchResult);
		MerchSegment merchSegment = MerchSegmentFactory.Get(flags);
		merchSegment.FindHigh(ref result2);
		VMZSearchResult sr = default(VMZSearchResult);
		FindLastHigherPriority(merchSegment, ref sr);
		bool found = sr.Found;
		if (!found)
		{
			sr.ResetToFirst();
		}
		VMZIterator vMZIterator = null;
		MerchandizeResult _mr = result;
		Location target = null;
		if (result2.Found)
		{
			VMZ.ForAllBetweenDo(delegate(Location loc)
			{
				if (loc.IsEmpty)
				{
					target = loc;
					return false;
				}
				if (IsThinType(loc.Flags))
				{
					return true;
				}
				if (!loc.IsStuck)
				{
					MerchandizeResult merchandizeResult = MoveNonThin(loc, manager);
					if (merchandizeResult == MerchandizeResult.Success)
					{
						target = loc;
						return false;
					}
					if (MerchandizeResult.EmptyStuck != merchandizeResult)
					{
						_mr = merchandizeResult;
						return false;
					}
				}
				return true;
			}, ref sr, ref result2);
			result = _mr;
			if (target != null)
			{
				if (Debug)
				{
					LogHelper.Instance.Log("[SelectLocation] Found an interior location at {0} for flags {1}", target.ToString(), flags.ToString());
				}
				return target;
			}
			vMZIterator = new VMZIterator(ref result2);
			if (!vMZIterator.Up())
			{
				result = MerchandizeResult.VMZFull;
				return null;
			}
		}
		else
		{
			vMZIterator = new VMZIterator(ref sr);
			if (found && !vMZIterator.Up())
			{
				result = MerchandizeResult.VMZFull;
				return null;
			}
		}
		target = FindTarget(vMZIterator, flags, manager, out result);
		if (target != null && Debug)
		{
			LogHelper.Instance.Log("[SelectLocation] Found an interior location at {0} for flags {1}", target.ToString(), flags.ToString());
		}
		return target;
	}

	internal MerchSegment FindContainingOrHigher(VMZIterator iterator)
	{
		foreach (MerchFlags item in DecreasingPriorityOrder)
		{
			if (ThinHelper.Debug)
			{
				LogHelper.Instance.Log("[FindContainingOrHigher] Look at segment {0}", item.ToString());
			}
			MerchSegment merchSegment = MerchSegmentFactory.Get(item);
			if (merchSegment.ItemCount() == 0)
			{
				continue;
			}
			if (merchSegment.Contains(iterator.Location))
			{
				return merchSegment;
			}
			VMZSearchResult result = default(VMZSearchResult);
			merchSegment.FindLow(ref result);
			if (iterator.RelativeTo(ref result) == IteratorComareResult.Below)
			{
				if (ThinHelper.Debug)
				{
					LogHelper.Instance.Log("[FindContainingOrHigher] Iterator at {0} is below {1}", iterator.Location.ToString(), result.Location.ToString());
				}
				return merchSegment;
			}
		}
		return null;
	}

	internal MerchandizeResult MoveNonThin(Location source, Location outside, MerchTransferManager manager)
	{
		List<Location> list = VMZ.Instance.FindFarthestEmptyOutsideOf(outside);
		if (list.Count == 0)
		{
			LogHelper.Instance.Log("[SegmentManager]: There are no empty slots available for transfer.");
			return MerchandizeResult.MachineFull;
		}
		using (new DisposeableList<Location>(list))
		{
			foreach (Location item in list)
			{
				if (ControllerService.Instance.MoveTo(source, MoveMode.Get, manager.Context.AppLog) != MotionResult.Success)
				{
					return MerchandizeResult.HardwareError;
				}
				GetResult getResult = ControllerService.Instance.Get(this, manager.Context.AppLog);
				if (getResult.EmptyOrStuck)
				{
					return MerchandizeResult.EmptyStuck;
				}
				if (ControllerService.Instance.MoveTo(item, MoveMode.Put, manager.Context.AppLog) != MotionResult.Success)
				{
					return MerchandizeResult.HardwareError;
				}
				PutResult putResult = ControllerService.Instance.Put(getResult.Previous, manager.Context.AppLog);
				if (putResult.Success)
				{
					manager.ProcessTransfer(source, item, getResult.Previous);
					if (getResult.ReturnTime.HasValue)
					{
						Deck deckNumber = DecksManager.Instance.GetDeckNumber(item.Deck);
						deckNumber.UpdateFromResult(getResult, item.Slot);
					}
					return MerchandizeResult.Success;
				}
				if (putResult.IsSlotInUse)
				{
					if (ControllerService.Instance.MoveTo(source, MoveMode.Put, manager.Context.AppLog) != MotionResult.Success)
					{
						return MerchandizeResult.HardwareError;
					}
					putResult = ControllerService.Instance.Put(getResult.Previous, manager.Context.AppLog);
					return MerchandizeResult.TransferFailure;
				}
			}
			return MerchandizeResult.EmptyStuck;
		}
	}

	internal MerchandizeResult MoveNonThin(Location source, MerchTransferManager manager)
	{
		VMZSearchResult high = default(VMZSearchResult);
		FindHighest(ref high);
		Location outside = (high.Found ? high.Location : source);
		return MoveNonThin(source, outside, manager);
	}

	internal MerchandizeResult MoveThin(Location source, Location empty, MerchTransferManager mgr)
	{
		MerchFlags flags = source.Flags;
		TransferResult transferResult = ControllerService.Instance.Transfer(source, empty, mgr.Context.AppLog);
		if (transferResult.Transferred)
		{
			if (Debug)
			{
				LogHelper.Instance.Log("MerchMetadata: xfer of {0} [Flags = {1}] to {2} was successful.", source.ToString(), flags.ToString(), transferResult.TransferredItem.Destination.ToString());
			}
			empty.ChangeMerchFlags(flags);
		}
		else
		{
			LogHelper.Instance.Log("MerchMetadata: xfer of {0} [Flags = {1}] failed with error {2}", source.ToString(), source.Flags.ToString(), (transferResult.ErrorCode == null) ? transferResult.FailedError : transferResult.ErrorCode);
		}
		mgr.Process(transferResult);
		return MerchandizeResult.Success;
	}

	private int Compare(MerchFlags left, MerchFlags right)
	{
		int num = IncreasingPriorityOrder.IndexOf(left);
		int num2 = IncreasingPriorityOrder.IndexOf(right);
		return num - num2;
	}

	private Location FindTarget(VMZIterator iterator, MerchFlags flags, MerchTransferManager manager, out MerchandizeResult result)
	{
		result = MerchandizeResult.Success;
		while (true)
		{
			if (iterator.IsEmpty)
			{
				return iterator.Location;
			}
			if (IsThinType(iterator.Location.Flags))
			{
				int num = Compare(iterator.Location.Flags, flags);
				if (num < 0)
				{
					MerchSegment merchSegment = MerchSegmentFactory.Get(iterator.Location.Flags);
					Location empty = merchSegment.GetEmpty();
					if (empty != null)
					{
						result = MoveThin(iterator.Location, empty, manager);
						if (result == MerchandizeResult.Success)
						{
							return iterator.Location;
						}
						if (MerchandizeResult.EmptyStuck != result)
						{
							return null;
						}
					}
					result = MerchandizeResult.VMZFull;
					break;
				}
			}
			else if (!iterator.IsStuck)
			{
				MerchandizeResult merchandizeResult = MoveNonThin(iterator.Location, manager);
				if (merchandizeResult == MerchandizeResult.Success)
				{
					return iterator.Location;
				}
				if (MerchandizeResult.EmptyStuck != merchandizeResult)
				{
					result = merchandizeResult;
					break;
				}
			}
			if (!iterator.Up())
			{
				result = MerchandizeResult.VMZFull;
				break;
			}
		}
		return null;
	}

	private void AddTransfer(TransferResult xferResult, ExecutionContext context)
	{
		if (xferResult.Transferred)
		{
			context.CreateResult("DiskTransferSuccessful", Instance.InCompressedZone(xferResult.TransferredItem.Source) ? "The item was moved internally in the VMZ." : "Transferred disk from VMZ to drum.", xferResult.TransferredItem.Source.Deck, xferResult.TransferredItem.Source.Slot, "EMPTY", null, xferResult.TransferredItem.Matrix);
			if (DumpBin.Instance.IsBin(xferResult.TransferredItem.Destination))
			{
				context.CreateResult("ThinTransferSuccessful", "The item was transferred from storage to the dump bin.", null, null, xferResult.TransferredItem.Matrix, null, null);
			}
			else
			{
				context.CreateResult("DiskTransferSuccessful", Instance.InCompressedZone(xferResult.TransferredItem.Destination) ? "The item was moved internally in the VMZ." : "Transferred disk from VMZ to drum.", xferResult.TransferredItem.Destination.Deck, xferResult.TransferredItem.Destination.Slot, xferResult.TransferredItem.Matrix, null, "EMPTY");
			}
		}
	}

	private void FindLastHigherPriority(MerchSegment start, ref VMZSearchResult sr)
	{
		MerchFlags nextHigher = start.NextHigher;
		while (true)
		{
			MerchSegment merchSegment = MerchSegmentFactory.Get(nextHigher);
			if (merchSegment == null)
			{
				break;
			}
			merchSegment.FindHigh(ref sr);
			if (sr.Found)
			{
				if (Debug)
				{
					LogHelper.Instance.Log("FindLastHigher: [Context = {0}]: found higher priority [Context = {1}] at {2}", start.Flags.ToString(), start.NextHigher.ToString(), sr.Location.ToString());
				}
				return;
			}
			nextHigher = merchSegment.NextHigher;
		}
		if (Debug)
		{
			LogHelper.Instance.Log("FindLastHigherPriority: [Context = {0}]: no higher priority segments found.", start.Flags.ToString());
		}
		sr.Invalidate();
	}

	private SegmentManager()
	{
		Debug = true;
		VMZ = VMZ.Instance;
		IncreasingPriorityOrder = Array.AsReadOnly(IncreasingPriorityFlags);
		MerchFlags[] array = new MerchFlags[IncreasingPriorityFlags.Length];
		int num = IncreasingPriorityFlags.Length - 1;
		int num2 = 0;
		while (num >= 0)
		{
			array[num2] = IncreasingPriorityFlags[num];
			num--;
			num2++;
		}
		DecreasingPriorityOrder = Array.AsReadOnly(array);
	}
}
