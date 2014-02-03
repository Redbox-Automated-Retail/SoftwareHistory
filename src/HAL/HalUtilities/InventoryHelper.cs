using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Redbox.HAL.Client;
using Redbox.HAL.Core;

namespace HALUtilities;

internal class InventoryHelper
{
	internal class Location
	{
		internal int Deck { get; set; }

		internal int Slot { get; set; }

		internal string ID { get; set; }
	}

	internal class Stats
	{
		internal int Unknown { get; set; }

		internal int Undefined { get; set; }

		internal int Max { get; set; }

		internal Stats()
		{
			Unknown = 0;
			Undefined = 0;
			Max = 0;
		}
	}

	internal class Config
	{
		internal int Deck { get; set; }

		internal bool IsQLM { get; set; }

		internal int Slots { get; set; }
	}

	private const string DefaultProtocol = "rcp://localhost:7001";

	private const string DuplicateToken = "(DUPLICATE)";

	private List<Location> m_inventory;

	private static HardwareService m_service;

	private static ClientHelper m_helper = null;

	private static InventoryHelper m_instance = null;

	private int m_qlmDeck = -1;

	internal HardwareService Service
	{
		get
		{
			if (m_service == null)
			{
				m_service = m_helper.AutoConnect();
			}
			return m_service;
		}
	}

	internal List<Location> Inventory => m_inventory;

	internal static InventoryHelper Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = new InventoryHelper();
			}
			return m_instance;
		}
	}

	internal int QlmDeck
	{
		get
		{
			if (m_qlmDeck != -1)
			{
				return m_qlmDeck;
			}
			string hALConfigurationInner = GetHALConfigurationInner("controller");
			if (!string.IsNullOrEmpty(hALConfigurationInner))
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(hALConfigurationInner);
				XmlNodeList xmlNodeList = xmlDocument.DocumentElement.SelectNodes("property");
				foreach (XmlNode item in xmlNodeList)
				{
					XmlNode namedItem = item.Attributes.GetNamedItem("display-name");
					if (namedItem == null || !namedItem.Value.Equals("Decks"))
					{
						continue;
					}
					foreach (XmlNode childNode in item.ChildNodes)
					{
						if (childNode.Name.Equals("Deck") && bool.Parse(childNode.Attributes.GetNamedItem("IsQlm").Value))
						{
							m_qlmDeck = int.Parse(childNode.Attributes.GetNamedItem("Number").Value);
							break;
						}
					}
				}
			}
			return m_qlmDeck;
		}
	}

	internal void DumpInventoryToFile(string path)
	{
		HardwareCommandResult inventoryState = Service.GetInventoryState();
		if (!inventoryState.Errors.ContainsError())
		{
			File.WriteAllText(path, inventoryState.CommandMessages[0]);
		}
	}

	internal void DumpInventoryToFile(TextWriter outputFile, List<Location> inventory)
	{
		foreach (Location item in inventory)
		{
			if (item.Deck != 8)
			{
				outputFile.WriteLine($"Deck={item.Deck} Slot={item.Slot} has ID {item.ID}");
			}
		}
	}

	internal bool ImportInventoryFromFile(string fileName)
	{
		try
		{
			string inventoryState = File.ReadAllText(fileName);
			return Service.SetInventoryState(inventoryState).Success;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
			return false;
		}
	}

	internal string GetHALConfiguration(string configName)
	{
		return GetHALConfigurationInner(configName);
	}

	internal List<Config> GetConfiguration(string configName)
	{
		List<Config> list = new List<Config>();
		string hALConfigurationInner = GetHALConfigurationInner(configName);
		if (string.IsNullOrEmpty(hALConfigurationInner))
		{
			Console.WriteLine("FAILED reading configuration from HALsvc");
			Environment.Exit(0);
		}
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(hALConfigurationInner);
		XmlNodeList xmlNodeList = xmlDocument.DocumentElement.SelectNodes("property");
		foreach (XmlNode item in xmlNodeList)
		{
			XmlNode namedItem = item.Attributes.GetNamedItem("display-name");
			if (namedItem == null || !namedItem.Value.Equals("Decks"))
			{
				continue;
			}
			foreach (XmlNode childNode in item.ChildNodes)
			{
				Config config = new Config();
				if (childNode.Name.Equals("Deck"))
				{
					config.Deck = int.Parse(childNode.Attributes.GetNamedItem("Number").Value);
					config.IsQLM = bool.Parse(childNode.Attributes.GetNamedItem("IsQlm").Value);
					config.Slots = int.Parse(childNode.Attributes.GetNamedItem("NumberOfSlots").Value);
				}
				list.Add(config);
			}
		}
		return list;
	}

	internal string GetBarcode(string duplicate)
	{
		int num = duplicate.IndexOf("(DUPLICATE)");
		if (-1 == num)
		{
			return string.Empty;
		}
		return duplicate.Substring(0, num);
	}

	internal bool ScheduleSyncForSlot(int deck, int slot)
	{
		HardwareCommandResult hardwareCommandResult = Service.HardSync(new SyncRange(deck, deck, new Range(slot, slot)), new HardwareJobSchedule
		{
			Priority = HardwareJobPriority.Low,
			StartTime = DateTime.Now
		}, out var job);
		if (hardwareCommandResult.Success)
		{
			job.Pend();
		}
		return hardwareCommandResult.Success;
	}

	internal bool SyncLocations(List<Location> locations, string jobLabel)
	{
		HardwareCommandResult hardwareCommandResult = Service.ScheduleJob("sync-locations", jobLabel, enableDebugging: false, new HardwareJobSchedule
		{
			Priority = HardwareJobPriority.Low,
			StartTime = DateTime.Now
		}, out var job);
		if (hardwareCommandResult.Success)
		{
			foreach (Location location in locations)
			{
				job.Push(location.Slot);
				job.Push(location.Deck);
			}
			job.Push(locations.Count);
			job.Pend();
		}
		return hardwareCommandResult.Success;
	}

	internal List<Location> FindSlotsMarked(string type)
	{
		List<Location> list = new List<Location>();
		HardwareCommandResult nonBarcodeInventory = Service.GetNonBarcodeInventory(new HardwareJobSchedule
		{
			Priority = HardwareJobPriority.High,
			StartTime = DateTime.Now
		}, out var job);
		if (!nonBarcodeInventory.Success)
		{
			return list;
		}
		m_helper.WaitForJob(job, out var _);
		if (!job.GetResults(out var results).Success)
		{
			return list;
		}
		if (type == "EMPTY")
		{
			ProgramResult[] array = results;
			foreach (ProgramResult programResult in array)
			{
				if (programResult.Code == "EmptyLocation")
				{
					list.Add(new Location
					{
						Deck = programResult.Deck,
						Slot = programResult.Slot,
						ID = type
					});
				}
			}
		}
		else
		{
			if (!(type == "UNKNOWN"))
			{
				return list;
			}
			ProgramResult[] array2 = results;
			foreach (ProgramResult programResult2 in array2)
			{
				if (programResult2.Code == "UnknownLocation")
				{
					list.Add(new Location
					{
						Deck = programResult2.Deck,
						Slot = programResult2.Slot,
						ID = type
					});
				}
			}
		}
		list.RemoveAll((Location each) => each.Deck == QlmDeck);
		list.Reverse();
		return list;
	}

	internal bool FindDuplicates(bool fixDuplicates, string outFile)
	{
		List<Location> list = FindSlotsMarked("(DUPLICATE)");
		if (list.Count > 0)
		{
			using StreamWriter outputFile = new StreamWriter(outFile);
			DumpInventoryToFile(outputFile, m_inventory);
			if (fixDuplicates)
			{
				List<Location> list2 = new List<Location>();
				foreach (Location item in list)
				{
					string origBC = GetBarcode(item.ID);
					Location location = m_inventory.Find((Location each) => each.ID == origBC);
					if (location != null && !list2.Contains(location))
					{
						list2.Add(location);
					}
				}
				list.AddRange(list2);
				list2.Clear();
				SyncLocations(list, "Duplicate Inventory sync.");
			}
		}
		return true;
	}

	internal bool FixType(string fileName, string type, string label)
	{
		if (type == "UNKNOWN")
		{
			return Service.ServiceUnknownSync().Success;
		}
		if (type == "EMPTY")
		{
			return Service.ServiceEmptySync().Success;
		}
		return false;
	}

	internal bool ScheduleFullSync()
	{
		return Service.FullSync().Success;
	}

	private string GetHALConfigurationInner(string configName)
	{
		HardwareCommandResult configuration = Service.GetConfiguration(configName);
		if (!configuration.Success)
		{
			return string.Empty;
		}
		return configuration.CommandMessages[0];
	}

	private void DumpFoundRangesForDeck(TextWriter log, int deck, List<Range> ranges)
	{
		log.WriteLine("Ranges for deck {0}", deck);
		foreach (Range range in ranges)
		{
			log.WriteLine("Start: {0} -> End: {1}", range.Start, range.End);
		}
	}

	private List<Location> GetInventoryFromService()
	{
		List<Location> list = new List<Location>();
		HardwareCommandResult inventoryState = Service.GetInventoryState();
		if (!inventoryState.Success)
		{
			return list;
		}
		XmlDocument xmlDocument = new XmlDocument();
		using StringReader txtReader = new StringReader(inventoryState.CommandMessages[0]);
		xmlDocument.Load(txtReader);
		if (xmlDocument.DocumentElement == null)
		{
			list.Clear();
			return list;
		}
		XmlNodeList elementsByTagName = xmlDocument.GetElementsByTagName("item");
		for (int i = 0; i < elementsByTagName.Count; i++)
		{
			list.Add(new Location
			{
				Deck = Convert.ToInt32(elementsByTagName[i].Attributes["deck"].Value),
				Slot = Convert.ToInt32(elementsByTagName[i].Attributes["slot"].Value),
				ID = elementsByTagName[i].Attributes["id"].Value
			});
		}
		return list;
	}

	private bool DeleteFile(string path)
	{
		if (!File.Exists(path))
		{
			return false;
		}
		try
		{
			File.Delete(path);
			return true;
		}
		catch (Exception ex)
		{
			Console.WriteLine("Unable to delete file {0} - {1}", path, ex.Message);
			return false;
		}
	}

	private InventoryHelper()
	{
		m_helper = new ClientHelper();
		m_inventory = GetInventoryFromService();
	}
}
