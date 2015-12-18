using Redbox.HAL.Client;

namespace HALUtilities;

internal class TestLocation
{
	internal Location Location { get; private set; }

	internal bool IsQlm { get; private set; }

	internal TestLocation(DeckConfig config, int slot)
	{
		Location = new Location
		{
			Deck = config.Deck,
			Slot = slot
		};
		IsQlm = config.IsQlm;
	}
}
