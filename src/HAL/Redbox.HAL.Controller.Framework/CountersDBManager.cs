using System;
using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Core;
using Redbox.HAL.DataStorage;

namespace Redbox.HAL.Controller.Framework;

public sealed class CountersDBManager
{
	internal delegate bool UpdateTableEvent(VistaDBHelper helper);

	private const string DatabaseName = "HALCounters.vdb3";

	public static CountersDBManager Instance => Singleton<CountersDBManager>.Instance;

	internal string CountersTableName => "Counters";

	internal string BinInventoryTableName => "BarcodesInBin";

	internal VistaDBHelper Store { get; private set; }

	public bool CreateTables(TextWriter writer)
	{
		if (!Store.TableExists(BinInventoryTableName))
		{
			string createQuery = $"CREATE TABLE [{BinInventoryTableName}] ( [Barcode] VarChar(100) NOT NULL, [PutTime] DateTime NOT NULL )";
			if (!Store.CreateTable(createQuery))
			{
				writer.WriteLine("Unable to create new database table '{0}'.", BinInventoryTableName);
			}
			else
			{
				writer.WriteLine("The table '{0}' was successfully created.", BinInventoryTableName);
			}
		}
		else
		{
			writer.WriteLine("The table '{0}' already exists - take no action.", BinInventoryTableName);
		}
		return SecretOptions.Instance.Upgrade(writer);
	}

	internal void Close()
	{
		try
		{
			if (Store != null)
			{
				Store.CloseConnection(force: true);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("CounterDBMgr.Close: caught an exception.", e);
		}
	}

	internal List<Counter> LoadCounters()
	{
		List<Counter> rv = new List<Counter>();
		lock (this)
		{
			string query = $"SELECT Name, Value FROM [{CountersTableName}]";
			Store.ExectuteSelectQuery(delegate(IDataReader reader)
			{
				Counter item = new Counter
				{
					Name = reader.ReadString(0),
					Value = reader.ReadInt(1)
				};
				rv.Add(item);
			}, query);
		}
		return rv;
	}

	internal Counter InsertCounter(string name, int defaultValue)
	{
		string insert = $"INSERT INTO [{CountersTableName}] (Name, Value) VALUES ('{name}', '{defaultValue}')";
		if (Store.Insert(insert))
		{
			Counter counter = new Counter();
			counter.Name = name;
			counter.Value = defaultValue;
			return counter;
		}
		LogHelper.Instance.Log($"Unable to create counter {name}", LogEntryType.Info);
		return null;
	}

	internal List<ProblemLocation> LoadLocations()
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return new List<ProblemLocation>();
		}
		List<ProblemLocation> rv = new List<ProblemLocation>();
		if (!Store.TableExists("ProblemLocations"))
		{
			return rv;
		}
		lock (this)
		{
			string query = "SELECT Deck, Slot, ProblemCount FROM [ProblemLocations]";
			Store.ExectuteSelectQuery(delegate(IDataReader reader)
			{
				ProblemLocation item = new ProblemLocation
				{
					Deck = reader.ReadInt(0),
					Slot = reader.ReadInt(1),
					ProblemCount = reader.ReadInt(2)
				};
				rv.Add(item);
			}, query);
			Store.DropTable("ProblemLocations");
			return rv;
		}
	}

	internal List<Location> ReadLegacyExcludes()
	{
		List<Location> rv = new List<Location>();
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return rv;
		}
		if (!Store.TableExists("ESP"))
		{
			return rv;
		}
		lock (this)
		{
			string query = "SELECT Deck, Slot, Excluded FROM [ESP]";
			Store.ExectuteSelectQuery(delegate(IDataReader reader)
			{
				string value = reader.ReadString(2);
				if (bool.TryParse(value, out var result) && result)
				{
					Location item = new Location(reader.ReadInt(0), reader.ReadInt(1));
					rv.Add(item);
				}
			}, query);
			Store.DropTable("ESP");
			return rv;
		}
	}

	internal List<DumpBinEntry> GetBarcodesInBin()
	{
		List<DumpBinEntry> rv = new List<DumpBinEntry>();
		lock (this)
		{
			string query = $"SELECT Barcode, PutTime FROM [{BinInventoryTableName}]";
			Store.ExectuteSelectQuery(delegate(IDataReader reader)
			{
				string id = reader.ReadString(0);
				DateTime? dateTime = reader.ReadDateTime(1);
				rv.Add(new DumpBinEntry(id, dateTime.Value));
			}, query);
			return rv;
		}
	}

	internal bool CodesRemoved(int expected)
	{
		try
		{
			string deleteQuery = $"DELETE FROM [{BinInventoryTableName}]";
			return Store.Delete(deleteQuery) == expected;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Failed to delete inventory from dump bin table.", e);
			return false;
		}
	}

	internal bool UpdateBinWithBarcode(string code)
	{
		string arg = Store.FormatNullableTime(DateTime.Now);
		string insert = $"INSERT INTO [{BinInventoryTableName}] ( Barcode, PutTime) VALUES ('{code}', {arg})";
		return Store.Insert(insert);
	}

	internal bool MakeUpdate(string query)
	{
		return Store.Update(query);
	}

	private CountersDBManager()
	{
		string source = Path.Combine(ControllerConfiguration.Instance.DataDirectory, "HALCounters.vdb3");
		Store = new VistaDBHelper(source, closeOnDispose: true);
	}
}
