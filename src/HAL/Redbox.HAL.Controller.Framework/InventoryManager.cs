using System;
using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Core;
using Redbox.HAL.DataStorage;

namespace Redbox.HAL.Controller.Framework;

public sealed class InventoryManager
{
	internal const int MaximumBarcodeLength = 100;

	private bool Initialized;

	public static InventoryManager Instance => Singleton<InventoryManager>.Instance;

	public bool CreateUnknownDatabase(int maxSlotsPerDeck)
	{
		List<DeckData> locs = new List<DeckData>();
		DecksManager.Instance.ForAllDecksDo(delegate(Deck deck)
		{
			int num = (deck.IsQlmDeck ? 80 : maxSlotsPerDeck);
			for (int i = 1; i <= num; i++)
			{
				locs.Add(new DeckData(deck.Number, i)
				{
					ID = "UNKNOWN",
					ReturnDate = null,
					EmptyStuckCount = 0,
					Excluded = false,
					MerchFlags = MerchFlags.None
				});
			}
			return true;
		});
		IPersistentMap dataStore = StoreManager.Instance.GetDataStore();
		dataStore.SetValue("InitializeState", "InitUnknown");
		IDataStore dataStore2 = StoreManager.Instance.FindByName("HALData.vdb3");
		if (dataStore2 == null)
		{
			return false;
		}
		using Transaction transaction = new Transaction(TransactionType.Insert);
		foreach (DeckData item in locs)
		{
			transaction.Enqueue(item.InsertCommand(dataStore2));
		}
		return transaction.Execute(dataStore2) == locs.Count;
	}

	public static bool UpdateInventoryDatabaseOnUpgrade(TextWriter writer)
	{
		writer.Write("Upgrading inventory datbase...");
		InventoryManager instance = Instance;
		IDataStore dataStore = StoreManager.Instance.FindByName("HALData.vdb3");
		if (dataStore == null)
		{
			writer.WriteLine("UpdateInventoryDatabaseOnUpgrade: there is no store.");
			return false;
		}
		return instance.FillTable(dataStore, "EMPTY");
	}

	internal bool Initialize()
	{
		if (Initialized)
		{
			return true;
		}
		if (!ShouldCleanupInit())
		{
			return Initialized = true;
		}
		LogHelper.Instance.Log("First init - mark DB EMPTY.", LogEntryType.Info);
		IDataStore store = StoreManager.Instance.FindByName("HALData.vdb3");
		if (store == null)
		{
			LogHelper.Instance.Log("There is no store available. Application is in trouble.");
			throw new ApplicationException("No inventory data stores.");
		}
		Transaction tx = default(Transaction);
		ref Transaction reference = ref tx;
		reference = new Transaction(TransactionType.Update);
		try
		{
			int expected = 0;
			DecksManager.Instance.ForAllDecksDo(delegate(Deck deck)
			{
				for (int i = 1; i < deck.NumberOfSlots; i++)
				{
					DeckData deckData = new DeckData(deck.Number, i);
					Transaction transaction3 = tx;
					transaction3.Enqueue(deckData.UpdateCommand(store));
				}
				expected += deck.NumberOfSlots;
				return true;
			});
			Transaction transaction = tx;
			Initialized = transaction.Execute(store) == expected;
		}
		finally
		{
			Transaction transaction2 = tx;
			((IDisposable)transaction2/*cast due to constrained. prefix*/).Dispose();
		}
		return Initialized;
	}

	private InventoryManager()
	{
	}

	private static bool ShouldCleanupInit()
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return false;
		}
		IPersistentMap dataStore = StoreManager.Instance.GetDataStore();
		string value = dataStore.GetValue("InitializeState", "NONE");
		if (value.Equals("InitUnknown", StringComparison.CurrentCultureIgnoreCase))
		{
			dataStore.Remove("InitializeState");
			return true;
		}
		return false;
	}

	private bool FillTable(IDataStore store, string id)
	{
		if (store.GetTableRowCount(string.Format("SELECT COUNT(*) FROM {0}", "Inventory_v1")) > 0)
		{
			return true;
		}
		List<DeckData> locs = new List<DeckData>();
		DecksManager.Instance.ForAllDecksDo(delegate(Deck deck)
		{
			for (int i = 1; i <= deck.NumberOfSlots; i++)
			{
				locs.Add(new DeckData(deck.Number, i)
				{
					ID = id,
					ReturnDate = null,
					EmptyStuckCount = 0,
					Excluded = false,
					MerchFlags = MerchFlags.None
				});
			}
			return true;
		});
		using Transaction transaction = new Transaction(TransactionType.Insert);
		foreach (DeckData item in locs)
		{
			transaction.Enqueue(item.InsertCommand(store));
		}
		return transaction.Execute(store) == locs.Count;
	}
}
