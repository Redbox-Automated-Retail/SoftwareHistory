using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Configuration;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public sealed class InventoryService : IInventoryService, IConfigurationObserver, IPackageInstallHandler
{
	private const int MaximumBarcodeLength = 100;

	private const string InventoryTable = "Inventory_v1";

	private const string DumpbinInventoryTable = "BarcodesInBin";

	private readonly IDataStore Store;

	private TextWriter EmptyStuckWriter = StreamWriter.Null;

	private readonly string EmptyStuckLogFile;

	private readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();

	private readonly List<IDumpBinInventoryItem> DumpbinItems = new List<IDumpBinInventoryItem>();

	private ILocation[][] LocationInfo;

	public bool IsTrackingEmptyStuck { get; private set; }

	public int DumpbinItemCount
	{
		get
		{
			using (new WithReadLock(Lock))
			{
				return DumpbinItems.Count;
			}
		}
	}

	public void OnRegister()
	{
		IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
		service.RegisterConfiguration(Configurations.Controller.ToString(), ControllerConfiguration.Instance);
		ServiceLocator.Instance.AddService(typeof(IDecksService), ControllerConfiguration.Instance.Manager);
	}

	public void OnUpgrade(ErrorList errors, TextWriter writer)
	{
		CountersStore countersStore = new CountersStore();
		if (Store.TableExists("BarcodesInBin"))
		{
			writer.WriteLine("[InventoryService.Upgrade] The bin table already exists.");
			return;
		}
		string createTable = string.Format("CREATE TABLE [{0}] ( [Barcode] VarChar(100) NOT NULL, [PutTime] DateTime NOT NULL )", "BarcodesInBin");
		if (!Store.CreateTable(createTable))
		{
			errors.Add(Error.NewError("I0777", "Create table error.", "Unable to create dumpbin table in haldata.vdb3."));
			return;
		}
		List<IDumpBinInventoryItem> oldStuff = new List<IDumpBinInventoryItem>();
		using (new DisposeableList<IDumpBinInventoryItem>(oldStuff))
		{
			string query = string.Format("SELECT Barcode, PutTime FROM [{0}]", "BarcodesInBin");
			countersStore.Store.ExectuteSelectQuery(delegate(IDataReader reader)
			{
				string id = reader.ReadString(0);
				DateTime? dateTime = reader.ReadDateTime(1);
				oldStuff.Add(new DumpBinEntry(id, dateTime.Value));
			}, query);
			using Transaction transaction = new Transaction(TransactionType.Insert);
			foreach (IDumpBinInventoryItem item in oldStuff)
			{
				string arg = Store.FormatNullableTime(item.PutTime);
				string statement = string.Format("INSERT INTO [{0}] ( Barcode, PutTime) VALUES ('{1}', {2})", "BarcodesInBin", item.ID, arg);
				transaction.Enqueue(statement);
			}
			if (transaction.Execute(Store) == oldStuff.Count)
			{
				writer.WriteLine("[InventoryService.OnUpgrade] Successfully imported {0} dumpbin entries.", oldStuff.Count);
			}
			else
			{
				errors.Add(Error.NewError("I667", "Import error.", "Unable to move dumpbin table data from halcounters."));
			}
		}
		countersStore.Store.DropTable("BarcodesInBin");
	}

	public void OnNewInstall(ErrorList errors, TextWriter writer)
	{
		GampHelper gampHelper = new GampHelper();
		int platterSlots = gampHelper.GetPlatterSlots();
		int registrySlotCount = ControllerConfiguration.Instance.RegistrySlotCount;
		if (platterSlots == registrySlotCount)
		{
			List<ILocation> locs = new List<ILocation>();
			IDecksService service = ServiceLocator.Instance.GetService<IDecksService>();
			service.ForAllDecksDo(delegate(IDeck deck)
			{
				for (int i = 1; i <= deck.NumberOfSlots; i++)
				{
					locs.Add(new Location(deck.Number, i, "EMPTY", null, excluded: false, 0, MerchFlags.None));
				}
				return true;
			});
			using Transaction transaction = new Transaction(TransactionType.Insert);
			foreach (ILocation item in locs)
			{
				transaction.Enqueue(InsertCommand(item));
			}
			if (transaction.Execute(Store) != locs.Count)
			{
				errors.Add(Error.NewError("I055", "Database insert failed.", "Failed to insert entries into inventory table."));
			}
			return;
		}
		writer.WriteLine("The machine is configured for {0} slots; the gamp data says there are {1} slots - mark db UNKNOWN with {2} slots", registrySlotCount, platterSlots, registrySlotCount);
	}

	public void NotifyConfigurationLoaded()
	{
		LogHelper.Instance.Log("[InventoryService] Configuration loaded.");
		Initialize();
		ConfigureStuck();
	}

	public void NotifyConfigurationChangeStart()
	{
		try
		{
			EmptyStuckWriter.Dispose();
		}
		catch
		{
		}
		finally
		{
			EmptyStuckWriter = StreamWriter.Null;
		}
	}

	public void NotifyConfigurationChangeEnd()
	{
		LogHelper.Instance.Log("[InventoryService] Configuration change end.");
		RecomputeInventory();
		ConfigureStuck();
	}

	public int GetMachineEmptyCount()
	{
		Predicate<ILocation> predicate = (ILocation loc) => !loc.Excluded && loc.ID == "EMPTY";
		return ComputeCount(predicate, excludeQLM: true);
	}

	public bool MachineIsFull()
	{
		return !EmptyCountExceeds(ControllerConfiguration.Instance.ReturnSlotBuffer);
	}

	public bool EmptyCountExceeds(int numberOfEmpty)
	{
		Predicate<ILocation> predicate = (ILocation loc) => !loc.Excluded && loc.ID == "EMPTY";
		int num = ComputeCount(predicate, excludeQLM: true);
		return num > numberOfEmpty;
	}

	public string GetInventoryState()
	{
		using (new WithReadLock(Lock))
		{
			using MemoryStream memoryStream = new MemoryStream();
			XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.ASCII);
			xmlTextWriter.Formatting = Formatting.Indented;
			XmlTextWriter xmlTextWriter2 = xmlTextWriter;
			xmlTextWriter2.WriteStartDocument();
			xmlTextWriter2.WriteStartElement("inventory-items");
			for (int i = 0; i < LocationInfo.GetLength(0); i++)
			{
				int num = LocationInfo[i].Length;
				for (int j = 0; j < num; j++)
				{
					ILocation location = LocationInfo[i][j];
					xmlTextWriter2.WriteStartElement("item");
					xmlTextWriter2.WriteAttributeString("deck", XmlConvert.ToString(location.Deck));
					xmlTextWriter2.WriteAttributeString("slot", XmlConvert.ToString(location.Slot));
					xmlTextWriter2.WriteAttributeString("id", location.ID);
					xmlTextWriter2.WriteAttributeString("ReturnTime", (!location.ReturnDate.HasValue) ? "NONE" : location.ReturnDate.Value.ToString());
					xmlTextWriter2.WriteAttributeString("excluded", location.Excluded.ToString());
					xmlTextWriter2.WriteAttributeString("emptyStuckCount", location.StuckCount.ToString());
					xmlTextWriter2.WriteAttributeString("merchFlags", location.Flags.ToString());
					xmlTextWriter2.WriteEndElement();
				}
			}
			foreach (IDumpBinInventoryItem dumpbinItem in DumpbinItems)
			{
				dumpbinItem.WriteElement(xmlTextWriter2);
			}
			xmlTextWriter2.WriteEndElement();
			xmlTextWriter2.WriteEndDocument();
			xmlTextWriter2.Flush();
			return Encoding.ASCII.GetString(memoryStream.ToArray());
		}
	}

	public void ResetInventoryState(string xml, ErrorList errors)
	{
		List<ILocation> inventory = new List<ILocation>();
		List<IDumpBinInventoryItem> list = new List<IDumpBinInventoryItem>();
		using (new WithUpgradeableReadLock(Lock))
		{
			using (new DisposeableList<ILocation>(inventory))
			{
				using (new DisposeableList<IDumpBinInventoryItem>(list))
				{
					XmlDocument xmlDocument = new XmlDocument();
					try
					{
						xmlDocument.LoadXml(xml);
						if (xmlDocument.DocumentElement == null)
						{
							errors.Add(Error.NewError("I001", "Invalid document.", "The specified document is invalid."));
							return;
						}
						XmlNodeList xmlNodeList = xmlDocument.DocumentElement.SelectNodes("item");
						if (xmlNodeList == null)
						{
							errors.Add(Error.NewError("I001", "Invalid document.", "The specified document has no nodes."));
							return;
						}
						foreach (XmlNode item in xmlNodeList)
						{
							ILocation location = FromXML(item, errors);
							if (location == null)
							{
								errors.Add(Error.NewError("I002", "Load error.", "Unable to load the deck from the XML."));
								return;
							}
							inventory.Add(location);
						}
						using (new WithWriteLock(Lock))
						{
							IDecksService service = ServiceLocator.Instance.GetService<IDecksService>();
							service.ForAllDecksDo(delegate(IDeck deck)
							{
								if (deck.Number - 1 < 0 || deck.Number - 1 > LocationInfo.GetLength(0))
								{
									errors.Add(Error.NewError("P004", $"The deck {deck.Number} was not present in the deck configuration.", "Ensure the inventory is valid."));
									return false;
								}
								List<ILocation> list2 = inventory.FindAll((ILocation each) => each.Deck == deck.Number);
								if (list2.Count != deck.NumberOfSlots)
								{
									errors.Add(Error.NewError("P004", $"The slot count {list2.Count} did not match the deck slot count for deck {deck.Number} ({deck.NumberOfSlots} slots).", "Ensure the inventory is valid."));
									LogHelper.Instance.Log($"Deck {deck.Number} expected {deck.NumberOfSlots} slots to be imported but received {list2.Count}.", LogEntryType.Error);
									return true;
								}
								list2.Sort((ILocation x, ILocation y) => x.Slot.CompareTo(y.Slot));
								SaveUnderLock(list2);
								LocationInfo[deck.Number - 1] = list2.ToArray();
								return true;
							});
							ClearDumpBinItemsUnderLock();
							xmlNodeList = xmlDocument.DocumentElement.SelectNodes("bin-item");
							if (xmlNodeList != null && xmlNodeList.Count > 0)
							{
								foreach (XmlNode item2 in xmlNodeList)
								{
									AddDumpbinItemUnderLock(DumpBinEntry.FromXml(item2, errors));
								}
							}
						}
						if (errors.Count > 0)
						{
							LogHelper.Instance.Log("RESET INVENTORY Failed to import inventory.", LogEntryType.Error);
						}
					}
					catch (Exception e)
					{
						errors.Add(Error.NewError("P999", "An unhandled exception was raised in ControllerConfiguration.Load.", e));
						LogHelper.Instance.Log("RESET INVENTORY Failed to import inventory.", e, LogEntryType.Error);
					}
				}
			}
		}
	}

	public ILocation Lookup(string id)
	{
		using (new WithReadLock(Lock))
		{
			return LookupUnderLock(id);
		}
	}

	public int GetExcludedSlotsCount()
	{
		using (new WithReadLock(Lock))
		{
			Predicate<ILocation> predicate = (ILocation loc) => loc.Excluded;
			return ComputeCount(predicate, excludeQLM: true);
		}
	}

	public IList<ILocation> ComputeExcludedSlots()
	{
		using (new WithReadLock(Lock))
		{
			Predicate<ILocation> predicate = (ILocation loc) => loc.Excluded;
			List<ILocation> list = ComputeList(predicate, excludeQLM: true);
			list.Sort((ILocation x, ILocation y) => (x.Deck == y.Deck) ? x.Slot.CompareTo(y.Slot) : x.Deck.CompareTo(y.Deck));
			return list;
		}
	}

	public IList<ILocation> GetEmptySlots()
	{
		using (new WithReadLock(Lock))
		{
			Predicate<ILocation> predicate = (ILocation loc) => !loc.Excluded && loc.ID == "EMPTY";
			return ComputeList(predicate, excludeQLM: true);
		}
	}

	public IList<ILocation> GetUnknowns()
	{
		using (new WithReadLock(Lock))
		{
			Predicate<ILocation> predicate = (ILocation loc) => !loc.Excluded && loc.ID == "UNKNOWN";
			return ComputeList(predicate, excludeQLM: true);
		}
	}

	public bool IsBarcodeDuplicate(string id, out ILocation original)
	{
		original = null;
		if (InventoryConstants.IsKnownInventoryToken(id))
		{
			return false;
		}
		using (new WithReadLock(Lock))
		{
			original = LookupUnderLock(id);
			return original != null;
		}
	}

	public bool AddBinItem(string matrix)
	{
		using (new WithWriteLock(Lock))
		{
			return AddDumpbinItemUnderLock(matrix, DateTime.Now);
		}
	}

	public bool Save(ILocation location)
	{
		using (new WithWriteLock(Lock))
		{
			return SaveUnderLock(location);
		}
	}

	public bool Save(IEnumerable<ILocation> locations)
	{
		using (new WithWriteLock(Lock))
		{
			return SaveUnderLock(locations);
		}
	}

	public bool ClearDumpbinInventory()
	{
		using (new WithWriteLock(Lock))
		{
			return ClearDumpBinItemsUnderLock();
		}
	}

	public IList<IDumpBinInventoryItem> GetBarcodesInBin()
	{
		using (new WithReadLock(Lock))
		{
			List<IDumpBinInventoryItem> list = new List<IDumpBinInventoryItem>();
			list.AddRange(DumpbinItems);
			return list;
		}
	}

	public bool Reset(IList<ILocation> locs)
	{
		using (new WithWriteLock(Lock))
		{
			foreach (ILocation loc in locs)
			{
				ResetLocation(loc);
			}
			return SaveUnderLock(locs);
		}
	}

	public bool Reset(ILocation loc)
	{
		using (new WithWriteLock(Lock))
		{
			ResetLocation(loc);
			return SaveUnderLock(loc);
		}
	}

	public bool MarkDeckInventory(IDeck deck, string newMatrix)
	{
		List<ILocation> list = new List<ILocation>();
		using (new WithWriteLock(Lock))
		{
			using (new DisposeableList<ILocation>(list))
			{
				for (int i = 1; i <= deck.NumberOfSlots; i++)
				{
					ILocation underLock = GetUnderLock(deck.Number, i);
					underLock.ID = newMatrix;
					list.Add(underLock);
				}
				return SaveUnderLock(list);
			}
		}
	}

	public bool ResetAndMark(IDeck deck, string id)
	{
		List<ILocation> list = new List<ILocation>();
		using (new DisposeableList<ILocation>(list))
		{
			using (new WithWriteLock(Lock))
			{
				for (int i = 1; i <= deck.NumberOfSlots; i++)
				{
					ILocation underLock = GetUnderLock(deck.Number, i);
					ResetLocation(underLock);
					underLock.ID = id;
					list.Add(underLock);
				}
				return SaveUnderLock(list);
			}
		}
	}

	public IList<int> SwapEmptyWith(IDeck deck, string id, MerchFlags flags, IRange<int> range)
	{
		List<int> list = new List<int>();
		List<ILocation> list2 = new List<ILocation>();
		using (new WithWriteLock(Lock))
		{
			using (new DisposeableList<ILocation>(list2))
			{
				for (int i = range.Start; i <= range.End; i++)
				{
					ILocation underLock = GetUnderLock(deck.Number, i);
					if (underLock.ID == "EMPTY" && !underLock.Excluded)
					{
						underLock.ID = id;
						underLock.Flags = flags;
						list2.Add(underLock);
						list.Add(i);
					}
				}
				SaveUnderLock(list2);
				return list;
			}
		}
	}

	public bool ExcludeSlots(IDeck deck, IList<int> slots)
	{
		List<ILocation> list = new List<ILocation>();
		using (new WithWriteLock(Lock))
		{
			using (new DisposeableList<ILocation>(list))
			{
				foreach (int slot in slots)
				{
					ILocation underLock = GetUnderLock(deck.Number, slot);
					Reset(underLock);
					underLock.Excluded = true;
					list.Add(underLock);
				}
				return SaveUnderLock(list);
			}
		}
	}

	public bool UnExcludeSlots(IDeck deck, IList<int> slots)
	{
		List<ILocation> list = new List<ILocation>();
		using (new WithWriteLock(Lock))
		{
			using (new DisposeableList<ILocation>(list))
			{
				foreach (int slot in slots)
				{
					ILocation underLock = GetUnderLock(deck.Number, slot);
					underLock.Excluded = false;
					list.Add(underLock);
				}
				return SaveUnderLock(list);
			}
		}
	}

	public ILocation Get(int deck, int slot)
	{
		using (new WithReadLock(Lock))
		{
			return GetUnderLock(deck, slot);
		}
	}

	public bool IsStuck(ILocation location)
	{
		if (!IsTrackingEmptyStuck)
		{
			return false;
		}
		return location.StuckCount > 0;
	}

	public void DumpStore(TextWriter writer)
	{
		using (new WithReadLock(Lock))
		{
			int rows = 0;
			writer.WriteLine("-- {0} Dump inventory store --", DateTime.Now.ToLongDateString());
			string query = string.Format("select Deck, Slot, ID, LastReturnTime, Excluded, EmptyStuckCount, MerchFlags FROM [{0}]", "Inventory_v1");
			Store.ExectuteSelectQuery(delegate(IDataReader reader)
			{
				DateTime? dateTime = reader.ReadDateTime(3);
				writer.WriteLine("Deck = {0} Slot = {1} ID = {2} ReturnTime = {3} Excluded = {4} StuckCount = {5} MerchFlags = {6}", reader.ReadInt(0), reader.ReadInt(1), reader.ReadString(2), dateTime.HasValue ? dateTime.Value.ToString() : "NONE", reader.ReadBoolean(4).ToString(), reader.ReadInt(5), reader.ReadString(6));
				rows++;
			}, query);
			writer.WriteLine("Processed {0} rows", rows);
			rows = 0;
			writer.WriteLine("-- Dump bin items --");
			query = string.Format("SELECT Barcode, PutTime FROM [{0}]", "BarcodesInBin");
			Store.ExectuteSelectQuery(delegate(IDataReader reader)
			{
				string arg = reader.ReadString(0);
				DateTime? dateTime = reader.ReadDateTime(1);
				writer.WriteLine("Dumpbin Item ID = {0} PutTime = {1}", arg, dateTime.HasValue ? dateTime.Value.ToString() : "NONE");
				rows++;
			}, query);
			writer.WriteLine("Processed {0} rows", rows);
			writer.WriteLine();
		}
	}

	public bool ResetCount(ILocation location)
	{
		if (!IsTrackingEmptyStuck)
		{
			return false;
		}
		using (new WithUpgradeableReadLock(Lock))
		{
			if (location.StuckCount == 0)
			{
				return true;
			}
			using (new WithWriteLock(Lock))
			{
				location.StuckCount = 0;
				LogFormattedMessage("Resetting Deck = {0} Slot = {1}", location.Deck, location.Slot);
				return SaveUnderLock(location);
			}
		}
	}

	public bool UpdateEmptyStuck(int deck, int slot)
	{
		ILocation location = Get(deck, slot);
		return UpdateEmptyStuck(location);
	}

	public bool UpdateEmptyStuck(ILocation location)
	{
		if (!IsTrackingEmptyStuck)
		{
			LogHelper.Instance.Log("[UpdateEmptyStuck] Service is not tracking.");
			return false;
		}
		using (new WithWriteLock(Lock))
		{
			location.StuckCount++;
			LogFormattedMessage("Incrementing Deck = {0} Slot = {1}; current stuck count = {2}", location.Deck, location.Slot, location.StuckCount);
			if (location.StuckCount >= ControllerConfiguration.Instance.MarkLocationUnknownThreshold && location.ID != "UNKNOWN")
			{
				LogFormattedMessage(" ** Error threshold met: set inventory to {0} at Deck = {1} Slot = {2}", "UNKNOWN", location.Deck, location.Slot);
				location.ID = "UNKNOWN";
			}
			return SaveUnderLock(location);
		}
	}

	public InventoryService()
		: this(new InventoryStore().Store)
	{
	}

	internal void LogFormattedMessage(string format, params object[] parms)
	{
		try
		{
			string arg = string.Format(format, parms);
			arg = $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} {arg}";
			EmptyStuckWriter.WriteLine(arg);
		}
		catch (Exception)
		{
		}
	}

	internal InventoryService(IDataStore store)
	{
		Store = store;
		ServiceLocator.Instance.AddService(typeof(IInventoryService), this);
		IFormattedLogFactoryService service = ServiceLocator.Instance.GetService<IFormattedLogFactoryService>();
		string path = service.CreateSubpath("ErrorLogs");
		EmptyStuckLogFile = Path.Combine(path, "ErrorLocationsHistory.txt");
	}

	private List<ILocation> ComputeList(Predicate<ILocation> predicate, bool excludeQLM)
	{
		List<ILocation> list = new List<ILocation>();
		IDecksService service = ServiceLocator.Instance.GetService<IDecksService>();
		int length = LocationInfo.GetLength(0);
		for (int i = 0; i < length; i++)
		{
			IDeck byNumber = service.GetByNumber(i + 1);
			if (excludeQLM && byNumber.IsQlm)
			{
				continue;
			}
			int num = LocationInfo[i].Length;
			for (int j = 0; j < num; j++)
			{
				ILocation location = LocationInfo[i][j];
				if (predicate(location))
				{
					list.Add(location);
				}
			}
		}
		return list;
	}

	private void RecomputeInventory()
	{
		using (new WithWriteLock(Lock))
		{
			IDecksService service = ServiceLocator.Instance.GetService<IDecksService>();
			service.ForAllDecksDo(delegate(IDeck deck)
			{
				int number = deck.Number;
				if (LocationInfo[number - 1].Length != deck.NumberOfSlots)
				{
					LogHelper.Instance.Log(LogEntryType.Error, "[InventoryService] RecomputeInventory current slot count = {0}; new deck count = {1}; deleting deck & initializing to EMPTY", LocationInfo[number - 1].Length, deck.NumberOfSlots);
					LocationInfo[number - 1] = CreateDeckEntries(deck);
				}
				else
				{
					List<ILocation> list = new List<ILocation>();
					using (new DisposeableList<ILocation>(list))
					{
						foreach (Quadrant quadrant in deck.Quadrants)
						{
							for (int i = quadrant.Slots.Start; i <= quadrant.Slots.End; i++)
							{
								ILocation underLock = GetUnderLock(deck.Number, i);
								if (underLock.Excluded != quadrant.IsExcluded)
								{
									underLock.Excluded = quadrant.IsExcluded;
									list.Add(underLock);
								}
							}
						}
						if (list.Count > 0)
						{
							SaveUnderLock(list);
						}
					}
				}
				return true;
			});
		}
	}

	private bool Initialize()
	{
		IDecksService service = ServiceLocator.Instance.GetService<IDecksService>();
		using (new WithWriteLock(Lock))
		{
			LocationInfo = new ILocation[service.DeckCount][];
			service.ForAllDecksDo(delegate(IDeck deck)
			{
				LocationInfo[deck.Number - 1] = LoadFromStore(deck);
				return true;
			});
			string query = string.Format("SELECT Barcode, PutTime FROM [{0}]", "BarcodesInBin");
			Store.ExectuteSelectQuery(delegate(IDataReader reader)
			{
				string id = reader.ReadString(0);
				DateTime? dateTime = reader.ReadDateTime(1);
				DumpbinItems.Add(new DumpBinEntry(id, dateTime.Value));
			}, query);
			LogHelper.Instance.Log("[InventoryService] Initialize loaded {0} dumpbin items.", DumpbinItems.Count);
			return true;
		}
	}

	private int ComputeCount(Predicate<ILocation> predicate, bool excludeQLM)
	{
		int num = 0;
		IDecksService service = ServiceLocator.Instance.GetService<IDecksService>();
		int length = LocationInfo.GetLength(0);
		for (int i = 0; i < length; i++)
		{
			IDeck byNumber = service.GetByNumber(i + 1);
			if (excludeQLM && byNumber.IsQlm)
			{
				continue;
			}
			int num2 = LocationInfo[i].Length;
			for (int j = 0; j < num2; j++)
			{
				ILocation obj = LocationInfo[i][j];
				if (predicate(obj))
				{
					num++;
				}
			}
		}
		return num;
	}

	private string UpdateCommand(ILocation location)
	{
		string text = Store.FormatNullableTime(location.ReturnDate);
		return string.Format("UPDATE {0} SET ID = '{1}', LastReturnTime = {2}, Excluded = '{3}', EmptyStuckCount = '{4}', MerchFlags = '{5}' WHERE  Deck = '{6}' AND Slot = '{7}'", "Inventory_v1", location.ID, text, location.Excluded ? 1 : 0, location.StuckCount, (int)location.Flags, location.Deck, location.Slot);
	}

	private string InsertCommand(ILocation location)
	{
		string text = Store.FormatNullableTime(location.ReturnDate);
		return string.Format("INSERT INTO [{0}] (Deck, Slot, ID, LastReturnTime, Excluded, EmptyStuckCount, MerchFlags) VALUES ('{1}', '{2}', '{3}', {4}, '{5}', '{6}', '{7}')", "Inventory_v1", location.Deck, location.Slot, location.ID, text, location.Excluded ? 1 : 0, location.StuckCount, (int)location.Flags);
	}

	private bool SaveUnderLock(ILocation location)
	{
		string update = UpdateCommand(location);
		return Store.Update(update);
	}

	private ILocation LookupUnderLock(string id)
	{
		IDecksService service = ServiceLocator.Instance.GetService<IDecksService>();
		for (int i = 0; i < LocationInfo.GetLength(0); i++)
		{
			IDeck byNumber = service.GetByNumber(i + 1);
			if (byNumber.IsQlm)
			{
				continue;
			}
			int num = LocationInfo[i].Length;
			for (int j = 0; j < num; j++)
			{
				ILocation location = LocationInfo[i][j];
				if (location.ID == id)
				{
					return location;
				}
			}
		}
		return null;
	}

	private bool AddDumpbinItemUnderLock(DumpBinEntry entry)
	{
		return AddDumpbinItemUnderLock(entry.ID, entry.PutTime);
	}

	private bool AddDumpbinItemUnderLock(string matrix, DateTime dt)
	{
		string arg = Store.FormatNullableTime(dt);
		string insert = string.Format("INSERT INTO [{0}] ( Barcode, PutTime) VALUES ('{1}', {2})", "BarcodesInBin", matrix, arg);
		DumpbinItems.Add(new DumpBinEntry(matrix, dt));
		return Store.Insert(insert);
	}

	private bool ClearDumpBinItemsUnderLock()
	{
		try
		{
			int count = DumpbinItems.Count;
			DumpbinItems.Clear();
			string deleteQuery = string.Format("DELETE FROM [{0}]", "BarcodesInBin");
			return Store.Delete(deleteQuery) == count;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Failed to delete inventory from dump bin table.", e);
			return false;
		}
	}

	private bool SaveUnderLock(IEnumerable<ILocation> locations)
	{
		using Transaction transaction = new Transaction(TransactionType.Update);
		int num = 0;
		foreach (ILocation location in locations)
		{
			transaction.Enqueue(UpdateCommand(location));
			num++;
		}
		if (transaction.Execute(Store) != num)
		{
			LogHelper.Instance.Log("[SaveUnderLock] WARNING: transaction failed.", LogEntryType.Error);
			return false;
		}
		return true;
	}

	private void ConfigureStuck()
	{
		IsTrackingEmptyStuck = ControllerConfiguration.Instance.TrackProblemLocations;
		LogHelper.Instance.Log("[InventoryService] The service {0} configured to track problem locations.", IsTrackingEmptyStuck ? "is" : "is not");
		if (IsTrackingEmptyStuck)
		{
			try
			{
				EmptyStuckWriter = new StreamWriter(File.Open(EmptyStuckLogFile, FileMode.Append, FileAccess.Write, FileShare.Read))
				{
					AutoFlush = true
				};
				return;
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("Unable to open EmptyStuck log.", e);
				EmptyStuckWriter = StreamWriter.Null;
				return;
			}
		}
		EmptyStuckWriter.Dispose();
		EmptyStuckWriter = StreamWriter.Null;
	}

	private ILocation FromXML(XmlNode node, ErrorList errors)
	{
		IDecksService service = ServiceLocator.Instance.GetService<IDecksService>();
		int? attributeValue = XmlNodeExtensions.GetAttributeValue<int?>(node, "deck", null);
		int? attributeValue2 = XmlNodeExtensions.GetAttributeValue<int?>(node, "slot", null);
		string attributeValue3 = XmlNodeExtensions.GetAttributeValue<string>(node, "id", null);
		string attributeValue4 = XmlNodeExtensions.GetAttributeValue<string>(node, "ReturnTime", null);
		DateTime? rt = null;
		if (!string.IsNullOrEmpty(attributeValue4) && !attributeValue4.Equals("NONE"))
		{
			try
			{
				rt = DateTime.Parse(attributeValue4);
			}
			catch (Exception)
			{
				LogHelper.Instance.Log($"Unable to parse date time {attributeValue4}", LogEntryType.Error);
				rt = null;
			}
		}
		bool attributeValue5 = XmlNodeExtensions.GetAttributeValue(node, "excluded", defaultValue: false);
		int attributeValue6 = XmlNodeExtensions.GetAttributeValue(node, "emptyStuckCount", 0);
		string attributeValue7 = XmlNodeExtensions.GetAttributeValue(node, "merchFlags", "None");
		MerchFlags flags = Enum<MerchFlags>.ParseIgnoringCase(attributeValue7, MerchFlags.None);
		if (!attributeValue.HasValue || !attributeValue2.HasValue || string.IsNullOrEmpty(attributeValue3))
		{
			errors.Add(Error.NewError("P004", "A valid deck, slot, and id must be specified for each item element.", "Add a valid item element with a deck, slot, and id attributes."));
			LogHelper.Instance.Log($"Attempt to import invalid item. XmlNode: {node.OuterXml}", LogEntryType.Error);
			return null;
		}
		if (attributeValue3.Length > 100)
		{
			errors.Add(Error.NewError("P005", $"Barcode {attributeValue3} exceeds the maximum length of {100} characters.", "Ensure that the length of the barcode is valid."));
			LogHelper.Instance.Log($"Barcode {attributeValue3} exceeds the maximum length of {100} characters.", LogEntryType.Error);
			return null;
		}
		IDeck byNumber = service.GetByNumber(attributeValue.Value);
		if (byNumber == null)
		{
			errors.Add(Error.NewError("P003", $"The deck value must be between 1 and {service.DeckCount}.", "Ensure the deck attribute value is within the valid range."));
			LogHelper.Instance.Log($"Deck {attributeValue} doesn't match any decks in the configuration.", LogEntryType.Error);
			return null;
		}
		if (attributeValue2 < 1 || byNumber.NumberOfSlots < attributeValue2)
		{
			errors.Add(Error.NewError("P003", $"The slot must be between 1 and {byNumber.NumberOfSlots}.", "Ensure the slot number is within the valid range."));
			LogHelper.Instance.Log($"Deck {byNumber.Number} expects a value between 1 and {byNumber.NumberOfSlots}, received {attributeValue2}", LogEntryType.Error);
			return null;
		}
		return new Location(attributeValue.Value, attributeValue2.Value, attributeValue3, rt, attributeValue5, attributeValue6, flags);
	}

	private ILocation[] LoadFromStore(IDeck deck)
	{
		List<ILocation> list = new List<ILocation>();
		using (new DisposeableList<ILocation>(list))
		{
			string query = string.Format("SELECT Slot, ID, LastReturnTime, Excluded, EmptyStuckCount, MerchFlags FROM [{0}] where Deck={1}", "Inventory_v1", deck.Number);
			Store.ExectuteSelectQuery(delegate(IDataReader reader)
			{
				DateTime? rt = null;
				string text = reader.ReadString(2);
				if (!string.IsNullOrEmpty(text))
				{
					try
					{
						rt = DateTime.Parse(text);
					}
					catch
					{
						rt = null;
					}
				}
				Location item = new Location(deck.Number, reader.ReadInt(0), reader.ReadString(1), rt, reader.ReadBoolean(3), reader.ReadInt(4), (MerchFlags)reader.ReadInt64(5));
				list.Add(item);
			}, query);
			list.Sort((ILocation x, ILocation y) => x.Slot.CompareTo(y.Slot));
			if (deck.NumberOfSlots == list.Count)
			{
				LogHelper.Instance.Log("[InventoryService] Loaded {0} entries for deck {1} ( # slots = {2} )", list.Count, deck.Number, deck.NumberOfSlots);
				return list.ToArray();
			}
			LogHelper.Instance.Log(LogEntryType.Error, "[InventoryService] LoadInventory: res.count = {0}; deck count = {1}; deleting deck & initializing to EMPTY", list.Count, deck.NumberOfSlots);
			return CreateDeckEntries(deck);
		}
	}

	private ILocation[] CreateDeckEntries(IDeck deck)
	{
		List<ILocation> list = new List<ILocation>();
		using (new DisposeableList<ILocation>(list))
		{
			new Range(1, deck.NumberOfSlots);
			string deleteQuery = string.Format("DELETE FROM {0} WHERE Deck = '{1}'", "Inventory_v1", deck.Number);
			Store.Delete(deleteQuery);
			using (Transaction transaction = new Transaction(TransactionType.Insert))
			{
				for (int i = 1; i <= deck.NumberOfSlots; i++)
				{
					Location location = new Location(deck.Number, i, "EMPTY", null, excluded: false, 0, MerchFlags.None);
					list.Add(location);
					transaction.Enqueue(InsertCommand(location));
				}
				transaction.Execute(Store);
			}
			list.Sort((ILocation x, ILocation y) => x.Slot.CompareTo(y.Slot));
			return list.ToArray();
		}
	}

	private ILocation GetUnderLock(int deck, int slot)
	{
		return LocationInfo[deck - 1][slot - 1];
	}

	private void ResetLocation(ILocation loc)
	{
		loc.ReturnDate = null;
		loc.ID = "EMPTY";
		loc.StuckCount = 0;
		loc.Flags = MerchFlags.None;
	}
}
