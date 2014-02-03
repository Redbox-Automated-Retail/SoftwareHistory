namespace Redbox.HAL.Controller.Framework;

internal class ESPNode
{
	private int m_index = -1;

	private Location m_location;

	public int Deck => m_location.Deck;

	public int Slot => m_location.Slot;

	public int Index => m_index;

	public static implicit operator Location(ESPNode node)
	{
		return node.m_location;
	}

	internal ESPNode(int deck, int slot, int idx)
	{
		m_location = new Location(deck, slot);
		m_index = idx;
	}
}
