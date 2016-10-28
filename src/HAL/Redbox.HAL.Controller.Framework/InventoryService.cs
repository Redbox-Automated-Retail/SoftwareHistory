using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Configuration;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public sealed class InventoryService : IInventoryService, IConfigurationObserver, IPackageInstallHandler
{
	private const int MaximumBarcodeLength = 100;

	private readonly IDataTable<ILocation> Table;

	private readonly string EmptyStuckLogFile;

	private readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();

	private ILocation[][] LocationInfo;

	public bool IsTrackingEmptyStuck { get; private set; }

	public void OnRegister()
	{
		ControllerConfiguration.Instance.Initialize();
	}

	public void OnUpgrade(ErrorList errors, TextWriter writer)
	{
	}

	public void OnNewInstall(ErrorList errors, TextWriter writer)
	{
		GampHelper gampHelper = new GampHelper();
		int platterSlots = gampHelper.GetPlatterSlots();
		int registrySlotCount = ControllerConfiguration.Instance.RegistrySlotCount;
		ITableTypeFactory factory = ServiceLocator.Instance.GetService<ITableTypeFactory>();
		if (platterSlots == registrySlotCount)
		{
			List<ILocation> locs = new List<ILocation>();
			IDecksService service = ServiceLocator.Instance.GetService<IDecksService>();
			service.ForAllDecksDo(delegate(IDeck deck)
			{
				for (int i = 1; i <= deck.NumberOfSlots; i++)
				{
					locs.Add(factory.NewLocation(deck.Number, i));
				}
				return true;
			});
			if (!Table.Insert(locs))
			{
				errors.Add(Error.NewError("I055", "Database insert failed.", "Failed to insert entries into inventory table."));
			}
		}
		else
		{
			writer.WriteLine("The machine is configured for {0} slots; the gamp data says there are {1} slots - mark db UNKNOWN with {2} slots", registrySlotCount, platterSlots, registrySlotCount);
		}
	}

	public void NotifyConfigurationLoaded()
	{
		LogHelper.Instance.Log("[InventoryService] Configuration loaded.");
		IDecksService service = ServiceLocator.Instance.GetService<IDecksService>();
		using (new WithWriteLock(Lock))
		{
			List<ILocation> inventory = Table.LoadEntries();
			using (new DisposeableList<ILocation>(inventory))
			{
				LocationInfo = new ILocation[service.DeckCount][];
				service.ForAllDecksDo(delegate(IDeck deck)
				{
					List<ILocation> list = inventory.FindAll((ILocation each) => each.Deck == deck.Number);
					if (list.Count == deck.NumberOfSlots)
					{
						LogHelper.Instance.Log("[InventoryService] Loaded {0} entries for deck {1} ( # slots = {2} )", list.Count, deck.Number, deck.NumberOfSlots);
					}
					else
					{
						LogHelper.Instance.Log(LogEntryType.Error, "[InventoryService] LoadInventory: res.count = {0}; deck count = {1}; deleting deck & initializing to EMPTY", list.Count, deck.NumberOfSlots);
						Table.Delete(list);
						list.Clear();
						ITableTypeFactory service2 = ServiceLocator.Instance.GetService<ITableTypeFactory>();
						for (int num = 1; num <= deck.NumberOfSlots; num++)
						{
							list.Add(service2.NewLocation(deck.Number, num));
						}
						Table.Insert(list);
					}
					list.Sort((ILocation x, ILocation y) => x.Slot.CompareTo(y.Slot));
					LocationInfo[deck.Number - 1] = list.ToArray();
					return true;
				});
			}
		}
		IsTrackingEmptyStuck = ControllerConfiguration.Instance.TrackProblemLocations;
		LogHelper.Instance.Log("[InventoryService] The service {0} configured to track problem locations.", IsTrackingEmptyStuck ? "is" : "is not");
	}

	public void NotifyConfigurationChangeStart()
	{
	}

	public void NotifyConfigurationChangeEnd()
	{
		LogHelper.Instance.Log("[InventoryService] Configuration change end.");
		RecomputeInventory();
		IsTrackingEmptyStuck = ControllerConfiguration.Instance.TrackProblemLocations;
		LogHelper.Instance.Log("[InventoryService] The service {0} configured to track problem locations.", IsTrackingEmptyStuck ? "is" : "is not");
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

	public ILocation Lookup(string id)
	{
		using (new WithReadLock(Lock))
		{
			return LookupUnderLock(id);
		}
	}

	public ILocation Get(int deck, int slot)
	{
		using (new WithReadLock(Lock))
		{
			return GetUnderLock(deck, slot);
		}
	}

	public bool Reset(ILocation loc)
	{
		using (new WithWriteLock(Lock))
		{
			ResetLocation(loc);
			return Table.Update(loc);
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
			return Table.Update(locs);
		}
	}

	public bool Save(ILocation location)
	{
		using (new WithWriteLock(Lock))
		{
			return Table.Update(location);
		}
	}

	public bool Save(IList<ILocation> locations)
	{
		using (new WithWriteLock(Lock))
		{
			return Table.Update(locations);
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

	public IList<ILocation> GetExcludedSlots()
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

	public int GetMachineEmptyCount()
	{
		Predicate<ILocation> predicate = (ILocation loc) => !loc.Excluded && loc.ID == "EMPTY";
		return ComputeCount(predicate, excludeQLM: true);
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
			return Table.Update(location);
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
			List<ILocation> list = Table.LoadEntries();
			writer.WriteLine("-- {0} Dump inventory store ( {1} total items )--", DateTime.Now.ToLongDateString(), list.Count);
			using (new DisposeableList<ILocation>(list))
			{
				foreach (ILocation item in list)
				{
					writer.WriteLine("{0} ID = {1} ReturnTime = {2} Excluded = {3} StuckCount = {4} MerchFlags = {5}", item.ToString(), item.ID, item.ReturnDate.HasValue ? item.ReturnDate.ToString() : "NONE", item.Excluded, item.StuckCount, item.Flags);
				}
			}
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
				return Table.Update(list);
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
				Table.Update(list2);
				return list;
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
				return Table.Update(list);
			}
		}
	}

	public void GetState(XmlTextWriter writer)
	{
		using (new WithReadLock(Lock))
		{
			for (int i = 0; i < LocationInfo.GetLength(0); i++)
			{
				int num = LocationInfo[i].Length;
				for (int j = 0; j < num; j++)
				{
					ILocation location = LocationInfo[i][j];
					writer.WriteStartElement("item");
					writer.WriteAttributeString("deck", XmlConvert.ToString(location.Deck));
					writer.WriteAttributeString("slot", XmlConvert.ToString(location.Slot));
					writer.WriteAttributeString("id", location.ID);
					writer.WriteAttributeString("ReturnTime", (!location.ReturnDate.HasValue) ? "NONE" : location.ReturnDate.Value.ToString());
					writer.WriteAttributeString("excluded", location.Excluded.ToString());
					writer.WriteAttributeString("emptyStuckCount", location.StuckCount.ToString());
					writer.WriteAttributeString("merchFlags", location.Flags.ToString());
					writer.WriteEndElement();
				}
			}
		}
	}

	public void ResetState(XmlDocument xmlDocument, ErrorList errors)
	{
		List<ILocation> inventory = new List<ILocation>();
		using (new WithUpgradeableReadLock(Lock))
		{
			using (new DisposeableList<ILocation>(inventory))
			{
				try
				{
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
							List<ILocation> list = inventory.FindAll((ILocation each) => each.Deck == deck.Number);
							if (list.Count != deck.NumberOfSlots)
							{
								errors.Add(Error.NewError("P004", $"The slot count {list.Count} did not match the deck slot count for deck {deck.Number} ({deck.NumberOfSlots} slots).", "Ensure the inventory is valid."));
								LogHelper.Instance.Log($"Deck {deck.Number} expected {deck.NumberOfSlots} slots to be imported but received {list.Count}.", LogEntryType.Error);
								return true;
							}
							list.Sort((ILocation x, ILocation y) => x.Slot.CompareTo(y.Slot));
							Table.Update(list);
							LocationInfo[deck.Number - 1] = list.ToArray();
							return true;
						});
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

	public InventoryService()
	{
		IDataTableService service = ServiceLocator.Instance.GetService<IDataTableService>();
		Table = service.GetTable<ILocation>();
		IFormattedLogFactoryService service2 = ServiceLocator.Instance.GetService<IFormattedLogFactoryService>();
		string path = service2.CreateSubpath("ErrorLogs");
		EmptyStuckLogFile = Path.Combine(path, "ErrorLocationsHistory.txt");
		ControllerConfiguration.Instance.AddObserver(this);
	}

	private void LogFormattedMessage(string format, params object[] parms)
	{
		if (!IsTrackingEmptyStuck)
		{
			return;
		}
		string arg = string.Format(format, parms);
		try
		{
			using StreamWriter streamWriter = new StreamWriter(File.Open(EmptyStuckLogFile, FileMode.Append, FileAccess.Write, FileShare.Read));
			streamWriter.WriteLine("{0} {1} {2}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), arg);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("[InventoryService] Failed to write to empty/stuck log.", e);
		}
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
					LogHelper.Instance.Log(LogEntryType.Error, "[InventoryService] RecomputeInventory failure: unsupported deck slot count change ( old count = {0} new count = {1} )", LocationInfo[number - 1].Length, deck.NumberOfSlots);
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
							Table.Update(list);
						}
					}
				}
				return true;
			});
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

	private ILocation FromXML(XmlNode node, ErrorList errors)
	{
		IDecksService service = ServiceLocator.Instance.GetService<IDecksService>();
		int? attributeValue = XmlNodeExtensions.GetAttributeValue<int?>(node, "deck", null);
		int? attributeValue2 = XmlNodeExtensions.GetAttributeValue<int?>(node, "slot", null);
		string attributeValue3 = XmlNodeExtensions.GetAttributeValue<string>(node, "id", null);
		string attributeValue4 = XmlNodeExtensions.GetAttributeValue<string>(node, "ReturnTime", null);
		DateTime? returnTime = null;
		if (!string.IsNullOrEmpty(attributeValue4) && !attributeValue4.Equals("NONE"))
		{
			try
			{
				returnTime = DateTime.Parse(attributeValue4);
			}
			catch (Exception)
			{
				LogHelper.Instance.Log($"Unable to parse date time {attributeValue4}", LogEntryType.Error);
				returnTime = null;
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
		ITableTypeFactory service2 = ServiceLocator.Instance.GetService<ITableTypeFactory>();
		return service2.NewLocation(attributeValue.Value, attributeValue2.Value, attributeValue3, returnTime, attributeValue5, attributeValue6, flags);
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
