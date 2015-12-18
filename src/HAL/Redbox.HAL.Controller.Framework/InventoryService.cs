using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Core;
using Redbox.HAL.DataStorage;

namespace Redbox.HAL.Controller.Framework;

public sealed class InventoryService
{
	public static InventoryService Instance => Singleton<InventoryService>.Instance;

	public void UpdateInventoryOnPut(PutResult result, ILocation location, string it, out string prev)
	{
		UpdateInventoryOnPutInner(result, location, it, out prev);
	}

	public bool IsBarcodeDuplicate(string id, out Location original)
	{
		original = null;
		if (InventoryConstants.IsKnownInventoryToken(id))
		{
			return false;
		}
		original = Lookup(id);
		return original != null;
	}

	public List<Location> GetUnknowns()
	{
		List<Location> rv = new List<Location>();
		DecksManager.Instance.ForAllDecksDo(delegate(Deck deck)
		{
			if (deck.IsQlmDeck)
			{
				return true;
			}
			deck.GetUnknownLocations(rv);
			return true;
		});
		return rv;
	}

	public List<Location> GetEmptySlots()
	{
		List<Location> rv = new List<Location>();
		DecksManager.Instance.ForAllDecksDo(delegate(Deck deck)
		{
			if (deck.IsQlmDeck)
			{
				return true;
			}
			deck.GetEmptyLocations(rv);
			return true;
		});
		return rv;
	}

	public int GetEmptyCount()
	{
		int count = 0;
		DecksManager.Instance.ForAllDecksDo(delegate(Deck deck)
		{
			if (!deck.IsQlmDeck)
			{
				count += deck.GetEmptyCount();
			}
			return true;
		});
		return count;
	}

	public void LogProblemLocations(TextWriter log, int threshold)
	{
		DecksManager.Instance.ForAllDecksDo(delegate(Deck deck)
		{
			for (int i = 1; i <= deck.NumberOfSlots; i++)
			{
				int emptyStuckCount = deck.GetEmptyStuckCount(i);
				if (emptyStuckCount >= threshold)
				{
					string value = string.Format("Empty/Stuck count for Deck = {0} Slot = {1} is {2} {3).", deck.Number, i, emptyStuckCount, (emptyStuckCount == 1) ? "instance" : "instances");
					log.WriteLine(value);
				}
			}
			return true;
		});
	}

	public List<Location> ComputeExcludedSlots()
	{
		List<Location> rv = new List<Location>();
		DecksManager.Instance.ForAllDecksDo(delegate(Deck deck)
		{
			for (int i = 1; i <= deck.NumberOfSlots; i++)
			{
				if (deck.IsExcluded(i))
				{
					rv.Add(new Location(deck.Number, i));
				}
			}
			return true;
		});
		return rv;
	}

	public string Inspect(int deck, int slot)
	{
		Deck deck2 = DecksManager.Instance.GetDeck(deck);
		if (!(deck2 == null))
		{
			return deck2.GetIdForSlot(slot);
		}
		return "DECK OUT OF RANGE";
	}

	public Location Lookup(string id)
	{
		Location rv = null;
		DecksManager.Instance.ForAllDecksDo(delegate(Deck each)
		{
			if (each.IsQlmDeck)
			{
				return true;
			}
			int num = each.FindId(id);
			if (num == -1)
			{
				return true;
			}
			rv = new Location(each.Number, num);
			return false;
		});
		return rv;
	}

	public bool EmptyCountExceeds(int numberOfEmpty)
	{
		int emptyCount = 0;
		DecksManager.Instance.ForAllDecksDo(delegate(Deck deck)
		{
			if (!deck.IsQlmDeck)
			{
				emptyCount += deck.GetEmptyCount();
			}
			return true;
		});
		return emptyCount > numberOfEmpty;
	}

	public bool MachineIsFull()
	{
		return !EmptyCountExceeds(ControllerConfiguration.Instance.ReturnSlotBuffer);
	}

	private void UpdateInventoryOnPutInner(PutResult pr, ILocation location, string id, out string previous)
	{
		bool flag = false;
		string text = (Deck.IsSuspiciousId(id) ? "UNKNOWN" : id);
		Deck deck = DecksManager.Instance.GetDeck(location.Deck);
		if (location.Deck == 7 && deck.NumberOfSlots == 72 && location.Slot % 6 == 0)
		{
			text = "UNKNOWN";
			flag = true;
		}
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			previous = ControllerEmulationState.Instance.ExpectedSetSlotIdPreviousIdResult;
			pr.IsDuplicate = ControllerEmulationState.Instance.ExpectedSetSlotIdIsDuplicateInventoryResult;
			return;
		}
		Location original = null;
		if (pr.IsDuplicate = IsBarcodeDuplicate(id, out original))
		{
			text = (ControllerConfiguration.Instance.MarkDuplicatesUnknown ? "UNKNOWN" : string.Format("{0}{1}", id, "(DUPLICATE)"));
			LogHelper.Instance.Log($"The ID {id} is a duplicate ( original at Deck = {original.Deck} Slot = {original.Slot} ); marking as {text}", LogEntryType.Info);
			if (ControllerConfiguration.Instance.MarkOriginalMatrixUnknown)
			{
				LogHelper.Instance.Log(string.Format("Mark the original matrix ID = {0} at Deck = {1} Slot = {2} ); marking as {3}", id, original.Deck, original.Slot, "UNKNOWN"), LogEntryType.Info);
				Deck deck2 = DecksManager.Instance.GetDeck(original.Deck);
				deck2.SetSlotId(original.Slot, "UNKNOWN");
			}
			CounterManager.Instance.Increment("DUPLICATE-COUNT");
		}
		previous = deck.SetSlotId(location.Slot, text);
		if (flag)
		{
			previous = "UNKNOWN";
		}
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			pr.Code = ControllerEmulationState.Instance.ExpectedPutResult.Code;
		}
	}

	private InventoryService()
	{
	}
}
