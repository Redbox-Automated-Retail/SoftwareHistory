using System;
using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Core;
using Redbox.HAL.DataStorage;

namespace Redbox.HAL.Controller.Framework;

public class InventoryManager
{
	internal const int MaximumBarcodeLength = 100;

	private const string DatabaseName = "HALData.vdb3";

	private const string InventoryTable = "Inventory_v1";

	public static InventoryManager Instance => Singleton<InventoryManager>.Instance;

	internal string TableName => "Inventory_v1";

	internal VistaDBHelper Store { get; private set; }

	private bool Initialized { get; set; }

	public bool CreateUnknownDatabase(int maxSlotsPerDeck, bool setResetFlag)
	{
		List<DeckData> locs = new List<DeckData>();
		DecksManager.Instance.ForAllDecksDo(delegate(Deck deck)
		{
			int num = (deck.IsQlmDeck ? 80 : maxSlotsPerDeck);
			for (int i = 1; i <= num; i++)
			{
				locs.Add(new DeckData
				{
					Deck = deck.Number,
					Slot = i,
					ID = "UNKNOWN",
					ReturnDate = null
				});
			}
			return true;
		});
		if (setResetFlag)
		{
			SecretOptions.Instance.SetValue("InitializeState", "InitUnknown");
		}
		return Instance.BulkInsert(locs);
	}

	public static bool UpdateInventoryDatabaseOnUpgrade(TextWriter writer, bool validate, bool makeEmptyAfterInstall)
	{
		writer.Write("Upgrading inventory datbase...");
		string text = "LiveInventory_v2";
		InventoryManager instance = Instance;
		if (!instance.Store.TableExists("Inventory_v1"))
		{
			string createQuery = string.Format("CREATE TABLE [{0}] ( [Deck] INT NOT NULL, [Slot] INT NOT NULL, [ID] VarChar(100) NOT NULL, [LastReturnTime] DateTime, [Excluded] bit NOT NULL, [EmptyStuckCount] INT NOT NULL, [MerchFlags] INT NOT NULL)", "Inventory_v1");
			if (!instance.Store.CreateTable(createQuery))
			{
				writer.WriteLine("Unable to create new database table.");
				return false;
			}
		}
		if (!instance.Store.TableExists(text))
		{
			if (!makeEmptyAfterInstall)
			{
				writer.WriteLine("There is no legacy inventory to import.");
				return true;
			}
			instance.CreateNonEmptyTable("EMPTY");
			writer.WriteLine("There is no legacy inventory to import.");
			return true;
		}
		List<DeckData> legacyInventory = instance.GetLegacyInventory(text);
		if (legacyInventory.Count == 0)
		{
			instance.CreateNonEmptyTable("EMPTY");
			writer.WriteLine("There is no inventory to import.");
			return true;
		}
		DeckInfoSorter comparer = new DeckInfoSorter();
		legacyInventory.Sort(comparer);
		if (!instance.BulkInsert(legacyInventory))
		{
			writer.WriteLine("Unable to insert inventory.");
			return false;
		}
		List<ProblemLocation> list = CountersDBManager.Instance.LoadLocations();
		if (list.Count > 0)
		{
			writer.Write("Migrate problem locations ...");
			using (Transaction transaction = new Transaction(TransactionType.Update))
			{
				string format = "UPDATE {0} SET EmptyStuckCount = '{1}' WHERE  Deck = '{2}' AND Slot = '{3}'";
				foreach (ProblemLocation item in list)
				{
					transaction.Enqueue(string.Format(format, instance.TableName, item.ProblemCount, item.Deck, item.Slot));
				}
				transaction.Execute(instance.Store);
			}
			writer.WriteLine(" ok.");
		}
		else
		{
			writer.WriteLine("There are no problem locations to migrate.");
		}
		List<Location> list2 = CountersDBManager.Instance.ReadLegacyExcludes();
		if (list2.Count > 0)
		{
			writer.Write("Migrate legacy excludes ...");
			using (Transaction transaction2 = new Transaction(TransactionType.Update))
			{
				string format2 = "UPDATE {0} SET Excluded = '1' WHERE  Deck = '{1}' AND Slot = '{2}'";
				foreach (Location item2 in list2)
				{
					transaction2.Enqueue(string.Format(format2, instance.TableName, item2.Deck, item2.Slot));
				}
				transaction2.Execute(instance.Store);
			}
			writer.WriteLine(" ok.");
		}
		else
		{
			writer.WriteLine("There are no legacy excludes to migrate.");
		}
		try
		{
			if (!validate)
			{
				writer.WriteLine("Import successful.");
				return true;
			}
			List<DeckData> legacyInventory2 = instance.GetLegacyInventory(instance.TableName);
			if (legacyInventory2.Count != legacyInventory.Count)
			{
				writer.WriteLine(" ***import failure: new count {0} doesn't match old {1}", legacyInventory2.Count, legacyInventory.Count);
				return false;
			}
			int num = 0;
			for (int i = 0; i < legacyInventory2.Count; i++)
			{
				DeckData deckData = legacyInventory[i];
				DeckData obj = legacyInventory2[i];
				if (!deckData.Equals(obj))
				{
					num++;
					writer.WriteLine(" *** compare failure at index {0}", i);
				}
			}
			writer.WriteLine(" import complete.");
			return num == 0;
		}
		finally
		{
			instance.Store.DropTable(text);
		}
	}

	internal void EnqueueUpdate(Transaction txn, DeckData data)
	{
		txn.Enqueue(UpdateCommand(data));
	}

	internal void EnqueueInsert(Transaction txn, DeckData data)
	{
		txn.Enqueue(ComputeInsertCommand(data));
	}

	internal bool ExecuteTransaction(Transaction txn)
	{
		return txn.Execute(Store);
	}

	internal bool UpdateSlot(DeckData data)
	{
		lock (this)
		{
			Validate();
			return Store.Update(UpdateCommand(data));
		}
	}

	internal bool BulkUpdate(List<DeckData> locs)
	{
		lock (this)
		{
			using Transaction transaction = new Transaction(Store, TransactionType.Update);
			foreach (DeckData loc in locs)
			{
				transaction.Enqueue(UpdateCommand(loc));
			}
			return transaction.Execute();
		}
	}

	internal bool BulkInsert(List<DeckData> locs)
	{
		lock (this)
		{
			using Transaction transaction = new Transaction(Store, TransactionType.Insert);
			foreach (DeckData loc in locs)
			{
				transaction.Enqueue(ComputeInsertCommand(loc));
			}
			return transaction.Execute();
		}
	}

	internal bool RemoveEntries(Deck deck, Range slots)
	{
		lock (this)
		{
			Validate();
			using Transaction transaction = new Transaction(Store, TransactionType.Delete);
			for (int i = slots.Start; i <= slots.End; i++)
			{
				transaction.Enqueue($"DELETE FROM {TableName} WHERE Deck = '{deck.Number}' AND Slot = '{i}'");
			}
			return transaction.Execute();
		}
	}

	internal bool Initialize()
	{
		if (Initialized)
		{
			return true;
		}
		LogHelper.Instance.Log(" >> InventoryManager.InitializeRepresentation()", LogEntryType.Debug);
		lock (this)
		{
			if (ShouldCleanupInit())
			{
				LogHelper.Instance.Log("First init - mark DB EMPTY.", LogEntryType.Info);
				MarkInventoryEmpty();
			}
		}
		Initialized = true;
		LogHelper.Instance.Log(" << InventoryManager.Initialize()", LogEntryType.Debug);
		return true;
	}

	internal bool UpdateDeckInventory(Deck deck, string setAs)
	{
		lock (this)
		{
			Validate();
			string format = "UPDATE {0} SET ID = '{1}' WHERE  Deck = '{2}' AND Slot = '{3}'";
			using Transaction transaction = new Transaction(Store, TransactionType.Update);
			for (int i = 1; i <= deck.NumberOfSlots; i++)
			{
				string statement = string.Format(format, TableName, setAs, deck.Number, i);
				transaction.Enqueue(statement);
			}
			if (!transaction.Execute())
			{
				LogHelper.Instance.Log($"There was an error setting the inventory on deck {deck.Number} to {setAs}", LogEntryType.Info);
				return false;
			}
			return true;
		}
	}

	internal string UpdateCommand(DeckData data)
	{
		string text = Store.FormatNullableTime(data.ReturnDate);
		return $"UPDATE {TableName} SET ID = '{data.ID}', LastReturnTime = {text}, Excluded = '{(data.Excluded ? 1 : 0)}', EmptyStuckCount = '{data.EmptyStuckCount}', MerchFlags = '{(int)data.MerchFlags}' WHERE  Deck = '{data.Deck}' AND Slot = '{data.Slot}'";
	}

	private bool MarkInventoryEmpty()
	{
		Transaction tx = new Transaction(Store, TransactionType.Update);
		try
		{
			DeckData templateData = new DeckData
			{
				Excluded = false,
				EmptyStuckCount = 0,
				ReturnDate = null,
				ID = "EMPTY",
				MerchFlags = MerchFlags.None
			};
			DecksManager.Instance.ForAllDecksDo(delegate(Deck deck)
			{
				for (int i = 1; i < deck.NumberOfSlots; i++)
				{
					templateData.Deck = deck.Number;
					templateData.Slot = i;
					tx.Enqueue(UpdateCommand(templateData));
				}
				return true;
			});
			return tx.Execute();
		}
		finally
		{
			if (tx != null)
			{
				((IDisposable)tx).Dispose();
			}
		}
	}

	internal List<DeckData> LoadDeck(int deck)
	{
		List<DeckData> tempList = new List<DeckData>();
		string query = $"SELECT Slot, ID, LastReturnTime, Excluded, EmptyStuckCount, MerchFlags FROM [{TableName}] where Deck={deck}";
		Store.ExectuteSelectQuery(delegate(IDataReader reader)
		{
			DateTime? returnDate = null;
			string text = reader.ReadString(2);
			if (!string.IsNullOrEmpty(text))
			{
				try
				{
					returnDate = DateTime.Parse(text);
				}
				catch
				{
					returnDate = null;
				}
			}
			DeckData item = new DeckData
			{
				Deck = deck,
				Slot = reader.ReadInt(0),
				ID = reader.ReadString(1),
				ReturnDate = returnDate,
				Excluded = reader.ReadBoolean(3),
				EmptyStuckCount = reader.ReadInt(4),
				MerchFlags = (MerchFlags)reader.ReadInt64(5)
			};
			tempList.Add(item);
		}, query);
		tempList.Sort(new DeckSlotSorter());
		return tempList;
	}

	private void Validate()
	{
		if (!Initialized)
		{
			Exception e = new Exception();
			LogHelper.Instance.Log("Inventory method called without initialization!", e);
		}
	}

	private InventoryManager()
	{
		string source = Path.Combine(ControllerConfiguration.Instance.DataDirectory, "HALData.vdb3");
		Store = new VistaDBHelper(source, closeOnDispose: true);
	}

	private static bool ShouldCleanupInit()
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return false;
		}
		string value = SecretOptions.Instance.GetValue("InitializeState", "NONE");
		if (value.Equals("InitUnknown", StringComparison.CurrentCultureIgnoreCase))
		{
			SecretOptions.Instance.RemoveOption("InitializeState");
			return true;
		}
		return false;
	}

	private string ComputeInsertCommand(DeckData loc)
	{
		string text = Store.FormatNullableTime(loc.ReturnDate);
		return $"INSERT INTO [{TableName}] (Deck, Slot, ID, LastReturnTime, Excluded, EmptyStuckCount, MerchFlags) VALUES ('{loc.Deck}', '{loc.Slot}', '{loc.ID}', {text}, false, '0', '0')";
	}

	private int RecordCountInner()
	{
		return Store.GetTableRowCount($"SELECT COUNT(*) FROM {TableName}");
	}

	private bool RowExistsQuery(int deck, int slot)
	{
		string query = $"select count(1) from {TableName} where deck = '{deck}' and slot = '{slot}'";
		return Store.RowExists(query);
	}

	private List<DeckData> GetLegacyInventory(string table)
	{
		List<DeckData> tempList = new List<DeckData>();
		string query = $"SELECT Deck, Slot, ID, LastReturnTime FROM [{table}]";
		Store.ExectuteSelectQuery(delegate(IDataReader reader)
		{
			int deck = reader.ReadInt(0);
			int slot = reader.ReadInt(1);
			string iD = reader.ReadString(2);
			DateTime? returnDate = null;
			if (!reader.IsNull(3))
			{
				returnDate = reader.ReadDateTime(3);
			}
			tempList.Add(new DeckData
			{
				Deck = deck,
				Slot = slot,
				ID = iD,
				ReturnDate = returnDate
			});
		}, query);
		return tempList;
	}

	private bool CreateNonEmptyTable(string fillIdAs)
	{
		if (RecordCountInner() > 0)
		{
			LogHelper.Instance.Log("Cannot fill table: rows exist.");
			return false;
		}
		List<DeckData> locs = new List<DeckData>();
		DecksManager.Instance.ForAllDecksDo(delegate(Deck deck)
		{
			for (int i = 1; i <= deck.NumberOfSlots; i++)
			{
				locs.Add(new DeckData
				{
					Deck = deck.Number,
					Slot = i,
					ID = fillIdAs,
					ReturnDate = null,
					EmptyStuckCount = 0,
					Excluded = false,
					MerchFlags = MerchFlags.None
				});
			}
			return true;
		});
		return Instance.BulkInsert(locs);
	}
}
