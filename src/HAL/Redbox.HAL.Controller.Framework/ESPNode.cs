namespace Redbox.HAL.Controller.Framework;

internal class ESPNode
{
	private readonly int Idx;

	private readonly Location Location;

	public int Deck => Location.Deck;

	public int Slot => Location.Slot;

	public int Index => Idx;

	public static implicit operator Location(ESPNode node)
	{
		return node.Location;
	}

	internal ESPNode(int deck, int slot, int idx)
	{
		Location = new Location(deck, slot);
		Idx = idx;
	}
}
