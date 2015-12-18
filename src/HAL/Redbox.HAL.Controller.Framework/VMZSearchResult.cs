namespace Redbox.HAL.Controller.Framework;

public struct VMZSearchResult
{
	public bool Found => -1 != Index;

	public Location Location
	{
		get
		{
			if (!Found)
			{
				return null;
			}
			return VMZ.Instance.LocationAt(Index);
		}
	}

	public int Deck
	{
		get
		{
			if (!Found)
			{
				return -1;
			}
			return Location.Deck;
		}
	}

	public int Slot
	{
		get
		{
			if (!Found)
			{
				return -1;
			}
			return Location.Slot;
		}
	}

	internal int Index { get; set; }

	public override string ToString()
	{
		if (!Found)
		{
			return "None";
		}
		return $"Deck = {Deck} Slot = {Slot} Flags = {Location.Flags.ToString()}";
	}

	public void ResetTo(Location location)
	{
		Index = VMZ.Instance.FindIndex(location);
	}

	public void ResetToFirst()
	{
		Index = 0;
	}

	public void Invalidate()
	{
		Index = -1;
	}
}
