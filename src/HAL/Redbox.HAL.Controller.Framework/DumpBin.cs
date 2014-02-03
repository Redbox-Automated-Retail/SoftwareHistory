using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public sealed class DumpBin : SpecialLocation
{
	private const string DumpBinCounterName = "DUMPBIN";

	private const string DumpBinPutFailureCounter = "DUMPBINPUTFAIL";

	private bool? m_isActive = null;

	private Location m_putLocation;

	private Location m_rotationLoc;

	private int m_currentCount;

	public static DumpBin Instance => Singleton<DumpBin>.Instance;

	public bool Active
	{
		get
		{
			if (m_isActive.HasValue)
			{
				return m_isActive.Value;
			}
			m_isActive = SecretOptions.Instance.GetValue("DumpSlotEnabled", defValue: false);
			base.CanMoveTo = m_isActive.Value;
			return m_isActive.Value;
		}
		internal set
		{
			m_isActive = value;
			SecretOptions.Instance.SetValue("DumpSlotEnabled", m_isActive.Value);
			base.CanMoveTo = m_isActive.Value;
		}
	}

	public Location PutLocation
	{
		get
		{
			if (!Active)
			{
				return null;
			}
			if (m_putLocation != null)
			{
				return m_putLocation;
			}
			return m_putLocation = new Location(base.Deck.Value, base.Slots.Start + 1);
		}
	}

	public int Capacity
	{
		get
		{
			if (Active)
			{
				return 60;
			}
			return 0;
		}
	}

	public override string EncryptedPassword => "zzKhFaPDNPI=";

	public bool IsFull()
	{
		return IsFullInner();
	}

	public bool IsBin(Location loc)
	{
		if (loc.Deck != base.Deck)
		{
			return false;
		}
		if (loc.Slot >= base.Slots.Start)
		{
			return loc.Slot <= base.Slots.End;
		}
		return false;
	}

	public bool IsBin(SearchLocation loc)
	{
		if (loc.Deck != base.Deck)
		{
			return false;
		}
		if (loc.Slot >= base.Slots.Start)
		{
			return loc.Slot <= base.Slots.End;
		}
		return false;
	}

	public override Location MoveableLocation()
	{
		return PutLocation;
	}

	public bool IncrementFailureCount()
	{
		if (!Active)
		{
			return false;
		}
		return CounterManager.Instance.Increment("DUMPBINPUTFAIL") != null;
	}

	public int CurrentCount()
	{
		if (Active)
		{
			return m_currentCount;
		}
		return 0;
	}

	public int RemainingSpace()
	{
		if (!IsFullInner())
		{
			return Capacity - m_currentCount;
		}
		return 0;
	}

	public List<DumpBinEntry> GetContents()
	{
		return CountersDBManager.Instance.GetBarcodesInBin();
	}

	public bool ResetCount()
	{
		if (!Active)
		{
			return false;
		}
		LogHelper.Instance.Log("Reset dumpbin counter: current bin count = {0}", m_currentCount);
		try
		{
			string path = Path.Combine(ControllerConfiguration.Instance.KioskLogsPath, "Service");
			string path2 = Path.Combine(path, "DumpBinInventory.log");
			using StreamWriter streamWriter = new StreamWriter(File.Open(path2, FileMode.Append, FileAccess.Write, FileShare.Read));
			List<DumpBinEntry> contents = Instance.GetContents();
			streamWriter.WriteLine("** Dump bin data **", contents.Count);
			foreach (DumpBinEntry item in contents)
			{
				streamWriter.WriteLine(" Barcode {0} put in at {1} {2}", item.ID, item.PutTime.ToShortDateString(), item.PutTime.ToShortTimeString());
			}
			streamWriter.WriteLine();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Failed to log dump bin contents.", e);
		}
		bool result = CountersDBManager.Instance.CodesRemoved(m_currentCount);
		m_currentCount = 0;
		Counter counter = CounterManager.Instance.Find("DUMPBIN");
		if (counter == null)
		{
			return false;
		}
		if (CounterManager.Instance.ResetCounter(counter))
		{
			return result;
		}
		return false;
	}

	public void DumpContents(IFormattedLog log)
	{
		if (!ControllerConfiguration.Instance.DumpVMZState)
		{
			return;
		}
		List<DumpBinEntry> barcodesInBin = CountersDBManager.Instance.GetBarcodesInBin();
		log.WriteFormatted("-- Dump bin inventory: {0} items in bin. --", barcodesInBin.Count);
		foreach (DumpBinEntry item in barcodesInBin)
		{
			log.WriteFormatted(" Barcode {0} put in at {1} {2}", item.ID, item.PutTime.ToShortDateString(), item.PutTime.ToShortTimeString());
		}
	}

	public bool Import(string file)
	{
		if (!File.Exists(file))
		{
			return false;
		}
		XmlDocument xmlDocument = new XmlDocument();
		try
		{
			xmlDocument.Load(file);
			if (xmlDocument.DocumentElement == null)
			{
				return false;
			}
			XmlNodeList xmlNodeList = xmlDocument.DocumentElement.SelectNodes("bin-item");
			if (xmlNodeList == null)
			{
				return false;
			}
			foreach (XmlNode item in xmlNodeList)
			{
				DumpBinEntry dumpBinEntry = DumpBinEntry.FromXml(item);
				if (dumpBinEntry != null)
				{
					AddToInventory(dumpBinEntry.ID);
				}
			}
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	internal bool AddToInventory(string id)
	{
		CountersDBManager.Instance.UpdateBinWithBarcode(id);
		m_currentCount++;
		return CounterManager.Instance.Increment("DUMPBIN") != null;
	}

	internal void Initialize()
	{
		Active = true;
		m_currentCount = CounterManager.Instance.Find("DUMPBIN")?.Value ?? Capacity;
	}

	internal override bool ExcludeFromESP(Deck deck, int slot)
	{
		if (!Active)
		{
			return false;
		}
		if (deck.Number != base.Deck.Value)
		{
			return false;
		}
		if (!base.Slots.InRange(slot))
		{
			return false;
		}
		deck.ExcludeSlot(slot, excluded: true);
		return true;
	}

	internal Location RotationTo()
	{
		if (m_rotationLoc == null)
		{
			m_rotationLoc = new Location(DecksManager.Instance.DeckCount, 1);
		}
		return m_rotationLoc;
	}

	private bool IsFullInner()
	{
		if (!Active)
		{
			return true;
		}
		return m_currentCount >= Capacity;
	}

	private DumpBin()
		: base("DumpBin", DecksManager.Instance.DeckCount, new Range(82, 84))
	{
		base.IsSecureLocation = true;
	}
}
