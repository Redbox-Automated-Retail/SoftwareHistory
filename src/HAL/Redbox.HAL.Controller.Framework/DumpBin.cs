using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Redbox.HAL.Core;
using Redbox.HAL.DataStorage;

namespace Redbox.HAL.Controller.Framework;

public sealed class DumpBin : SpecialLocation
{
	private const string DumpBinPutFailureCounter = "DUMPBINPUTFAIL";

	internal readonly Location RotationLocation;

	private bool m_initialized;

	private int m_currentCount;

	private readonly Location m_putLocation;

	public static DumpBin Instance => Singleton<DumpBin>.Instance;

	public bool Active => ControllerConfiguration.Instance.IsVMZMachine;

	public Location PutLocation
	{
		get
		{
			if (Active)
			{
				return m_putLocation;
			}
			return null;
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

	public bool IsBin(ControllerLocation loc)
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
		return DumpBinEntry.GetBarcodesInBin();
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
		if (!DumpBinEntry.CodesRemoved(m_currentCount))
		{
			return false;
		}
		m_currentCount = 0;
		return true;
	}

	public void DumpContents(IFormattedLog log)
	{
		if (!ControllerConfiguration.Instance.DumpVMZState)
		{
			return;
		}
		List<DumpBinEntry> barcodesInBin = DumpBinEntry.GetBarcodesInBin();
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
		if (!DumpBinEntry.AddEntry(id))
		{
			return false;
		}
		m_currentCount++;
		return true;
	}

	internal void Initialize()
	{
		if (!m_initialized)
		{
			m_initialized = true;
			m_currentCount = DumpBinEntry.ItemsCount();
		}
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

	private bool IsFullInner()
	{
		if (Active)
		{
			return m_currentCount >= Capacity;
		}
		return true;
	}

	private DumpBin()
		: base("DumpBin", DecksManager.Instance.DeckCount, new Range(82, 84))
	{
		base.IsSecureLocation = true;
		RotationLocation = new Location(DecksManager.Instance.DeckCount, 1);
		m_putLocation = new Location(base.Deck.Value, base.Slots.Start + 1);
	}
}
