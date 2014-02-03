using Redbox.HAL.Core;

namespace Redbox.HAL.Client;

public class SyncRange
{
	public int EndDeck { get; set; }

	public Range Slots { get; set; }

	public int StartDeck { get; set; }

	public SyncRange()
	{
	}

	public SyncRange(int startDeck, int endDeck, Range slots)
	{
		StartDeck = startDeck;
		EndDeck = endDeck;
		Slots = slots;
	}
}
