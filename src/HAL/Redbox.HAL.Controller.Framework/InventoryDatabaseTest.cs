using Redbox.HAL.Core;
using Redbox.HAL.DataStorage;

namespace Redbox.HAL.Controller.Framework;

public sealed class InventoryDatabaseTest
{
	public bool Test()
	{
		int failures = 0;
		IDataStore store = StoreManager.Instance.FindByName("HALData.vdb3");
		DecksManager.Instance.ForAllDecksDo(delegate(Deck deck)
		{
			DeckData deckData = deck.FindFirst("EMPTY");
			if (deckData == null)
			{
				failures++;
				return true;
			}
			DeckData deckData2 = DeckData.CloneFrom(deckData);
			deckData2.EmptyStuckCount++;
			if (!store.Update(deckData2.UpdateCommand(store)))
			{
				LogHelper.Instance.Log(LogEntryType.Error, "[InventoryDBTest] Update failure on deck {0}", deck.Number);
				failures++;
			}
			store.Update(deckData.UpdateCommand(store));
			return true;
		});
		return 0 == failures;
	}
}
