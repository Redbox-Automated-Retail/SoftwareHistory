namespace Redbox.HAL.Controller.Framework;

public struct SearchLocation
{
	public int Deck { get; private set; }

	public int Slot { get; private set; }

	public SearchLocation(int deck, int slot)
	{
		this = default(SearchLocation);
		Deck = deck;
		Slot = slot;
	}

	public SearchLocation(Location location)
	{
		this = default(SearchLocation);
		Deck = location.Deck;
		Slot = location.Slot;
	}
}
