using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public sealed class VMZIterator : IDisposable
{
	public Location Location => VMZ.Instance.LocationAt(Index);

	public bool IsEmpty => VMZ.Instance.LocationAt(Index).IsEmpty;

	public bool IsStuck => VMZ.Instance.LocationAt(Index).IsStuck;

	public MerchFlags Flags => VMZ.Instance.LocationAt(Index).Flags;

	internal int Index { get; private set; }

	private bool Disposed { get; set; }

	public void Dispose()
	{
		if (!Disposed)
		{
			Disposed = true;
		}
	}

	public override string ToString()
	{
		Location location = VMZ.Instance.LocationAt(Index);
		if (location != null)
		{
			return $"Deck = {location.Deck} Slot = {location.Slot} Flags = {location.Flags.ToString()}";
		}
		return "NONE";
	}

	public void ResetLocation()
	{
		Location location = VMZ.Instance.LocationAt(Index);
		Deck deck = DecksManager.Instance.GetDeck(location.Deck);
		if (deck == null)
		{
			LogHelper.Instance.Log("The deck {0} is invalid.", location.Deck);
		}
		else
		{
			deck.ResetLocation(location.Slot);
		}
	}

	public void ResetToFirst()
	{
		Index = 0;
	}

	public void ResetTo(ref VMZSearchResult result)
	{
		Index = result.Index;
	}

	public void ResetTo(Location start)
	{
		Index = VMZ.Instance.FindIndex(start);
	}

	public bool Up()
	{
		if (Index == -1 || Index == VMZ.Instance.LastIndex)
		{
			return false;
		}
		Index++;
		return true;
	}

	public void Invalidate()
	{
		Index = -1;
	}

	public IteratorComareResult RelativeTo(ref VMZSearchResult sr)
	{
		if (Index < sr.Index)
		{
			return IteratorComareResult.Below;
		}
		if (Index <= sr.Index)
		{
			return IteratorComareResult.Equal;
		}
		return IteratorComareResult.Above;
	}

	public VMZIterator(ref VMZSearchResult result)
	{
		Index = result.Index;
	}

	public VMZIterator()
	{
		ResetToFirst();
	}
}
