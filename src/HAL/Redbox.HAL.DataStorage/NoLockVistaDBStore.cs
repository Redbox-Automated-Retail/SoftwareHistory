using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;
using VistaDB.Provider;

namespace Redbox.HAL.DataStorage;

public sealed class NoLockVistaDBStore : IDataStore
{
	private readonly string ConnectionString;

	public bool Update(string update)
	{
		using VistaDBConnection connection = Connect();
		using VistaDBCommand vistaDBCommand = new VistaDBCommand();
		vistaDBCommand.CommandText = update;
		vistaDBCommand.Connection = connection;
		bool flag = 1 == vistaDBCommand.ExecuteNonQuery();
		if (!flag)
		{
			LogHelper.Instance.Log("** Database Update returned false.", LogEntryType.Error);
		}
		return flag;
	}

	public int BulkUpdate(List<string> update)
	{
		int num = 0;
		using (VistaDBConnection connection = Connect())
		{
			foreach (string item in update)
			{
				using VistaDBCommand vistaDBCommand = new VistaDBCommand();
				vistaDBCommand.CommandText = item;
				vistaDBCommand.Connection = connection;
				num += vistaDBCommand.ExecuteNonQuery();
			}
		}
		LogHelper.Instance.Log(LogEntryType.Debug, "[Bulk Update] successes = {0}", num);
		return num;
	}

	public bool Insert(string insert)
	{
		using VistaDBConnection connection = Connect();
		using VistaDBCommand vistaDBCommand = new VistaDBCommand();
		vistaDBCommand.CommandText = insert;
		vistaDBCommand.Connection = connection;
		return 1 == vistaDBCommand.ExecuteNonQuery();
	}

	public int BulkInsert(List<string> inserts)
	{
		int num = 0;
		using VistaDBConnection connection = Connect();
		foreach (string insert in inserts)
		{
			using VistaDBCommand vistaDBCommand = new VistaDBCommand();
			vistaDBCommand.CommandText = insert;
			vistaDBCommand.Connection = connection;
			if (1 == vistaDBCommand.ExecuteNonQuery())
			{
				num++;
			}
		}
		return num;
	}

	public int Delete(string deleteQuery)
	{
		using VistaDBConnection connection = Connect();
		using VistaDBCommand vistaDBCommand = new VistaDBCommand();
		vistaDBCommand.CommandText = deleteQuery;
		vistaDBCommand.Connection = connection;
		return vistaDBCommand.ExecuteNonQuery();
	}

	public int BulkDelete(List<string> removes)
	{
		int num = 0;
		using VistaDBConnection connection = Connect();
		foreach (string remove in removes)
		{
			using VistaDBCommand vistaDBCommand = new VistaDBCommand();
			vistaDBCommand.CommandText = remove;
			vistaDBCommand.Connection = connection;
			if (1 == vistaDBCommand.ExecuteNonQuery())
			{
				num++;
			}
		}
		return num;
	}

	public bool RowExists(string query)
	{
		int num = 0;
		using (VistaDBConnection connection = Connect())
		{
			using VistaDBCommand vistaDBCommand = new VistaDBCommand();
			vistaDBCommand.CommandText = query;
			vistaDBCommand.Connection = connection;
			num = (int)vistaDBCommand.ExecuteScalar();
		}
		return num == 1;
	}

	public int GetTableRowCount(string query)
	{
		int num = -1;
		using VistaDBConnection connection = Connect();
		using VistaDBCommand vistaDBCommand = new VistaDBCommand();
		vistaDBCommand.CommandText = query;
		vistaDBCommand.Connection = connection;
		return (int)vistaDBCommand.ExecuteScalar();
	}

	public bool TableExists(string name)
	{
		bool result = false;
		using (VistaDBConnection vistaDBConnection = Connect())
		{
			try
			{
				vistaDBConnection.GetTableSchema(name);
				result = true;
			}
			catch (Exception)
			{
				result = false;
			}
		}
		return result;
	}

	public bool CreateTable(string createTable)
	{
		using VistaDBConnection connection = Connect();
		using VistaDBCommand vistaDBCommand = new VistaDBCommand();
		vistaDBCommand.CommandText = createTable;
		vistaDBCommand.Connection = connection;
		try
		{
			int num = vistaDBCommand.ExecuteNonQuery();
			return num == 0;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"CreateTable {createTable} caught an exception.", e);
			return false;
		}
	}

	public bool DropTable(string tableName)
	{
		string text = $"DROP TABLE {tableName}";
		int num = -1;
		using (VistaDBConnection connection = Connect())
		{
			using VistaDBCommand vistaDBCommand = new VistaDBCommand();
			vistaDBCommand.CommandText = text;
			vistaDBCommand.Connection = connection;
			try
			{
				num = vistaDBCommand.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				num = -1;
				LogHelper.Instance.Log($"ERROR: Failed to execute Non-Query: {text} - exception {ex.Message}", LogEntryType.Error);
			}
		}
		return num >= 0;
	}

	public void ExectuteSelectQuery(HandleSelectResult callback, string query)
	{
		using VistaDBConnection connection = Connect();
		using VistaDBCommand vistaDBCommand = new VistaDBCommand(query, connection);
		using VistaDBDataReader vistaDBDataReader = vistaDBCommand.ExecuteReader();
		VistaDBReader reader = new VistaDBReader(vistaDBDataReader);
		while (vistaDBDataReader.Read())
		{
			callback?.Invoke(reader);
		}
	}

	public string FormatNullableTime(DateTime? time)
	{
		if (time.HasValue)
		{
			return $"'{time.Value}'";
		}
		return "NULL";
	}

	public void CloseConnection(bool force)
	{
	}

	internal NoLockVistaDBStore(string source)
	{
		ConnectionString = $"Data Source={source};Open Mode=ExclusiveReadWrite;Pooling=false";
	}

	private VistaDBConnection Connect()
	{
		VistaDBConnection vistaDBConnection = new VistaDBConnection(ConnectionString);
		vistaDBConnection.Open();
		return vistaDBConnection;
	}
}
