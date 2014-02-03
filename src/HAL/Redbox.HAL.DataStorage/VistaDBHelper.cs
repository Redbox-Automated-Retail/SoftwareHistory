using System;
using System.Collections.Generic;
using Redbox.HAL.Core;
using VistaDB.Provider;

namespace Redbox.HAL.DataStorage;

public sealed class VistaDBHelper
{
	public delegate void HandleSelectResult(IDataReader reader);

	private VistaDBConnectionManager ConnManager { get; set; }

	private string DataSource { get; set; }

	private string ConnectionString { get; set; }

	public bool Update(string update)
	{
		using VistaDBConnectionManager vistaDBConnectionManager = ConnManager.Connect(ConnectionString);
		using VistaDBCommand vistaDBCommand = new VistaDBCommand();
		vistaDBCommand.CommandText = update;
		vistaDBCommand.Connection = vistaDBConnectionManager.Connection;
		bool flag = 1 == vistaDBCommand.ExecuteNonQuery();
		if (!flag)
		{
			LogHelper.Instance.Log("WARNING: VistaDBHelper.Update returned false.");
		}
		return flag;
	}

	public int BulkUpdate(List<string> update)
	{
		int num = 0;
		using VistaDBConnectionManager vistaDBConnectionManager = ConnManager.Connect(ConnectionString);
		foreach (string item in update)
		{
			using VistaDBCommand vistaDBCommand = new VistaDBCommand();
			vistaDBCommand.CommandText = item;
			vistaDBCommand.Connection = vistaDBConnectionManager.Connection;
			if (1 == vistaDBCommand.ExecuteNonQuery())
			{
				num++;
			}
		}
		return num;
	}

	public bool Insert(string insert)
	{
		using VistaDBConnectionManager vistaDBConnectionManager = ConnManager.Connect(ConnectionString);
		using VistaDBCommand vistaDBCommand = new VistaDBCommand();
		vistaDBCommand.CommandText = insert;
		vistaDBCommand.Connection = vistaDBConnectionManager.Connection;
		return 1 == vistaDBCommand.ExecuteNonQuery();
	}

	public int BulkInsert(List<string> inserts)
	{
		int num = 0;
		using VistaDBConnectionManager vistaDBConnectionManager = ConnManager.Connect(ConnectionString);
		foreach (string insert in inserts)
		{
			using VistaDBCommand vistaDBCommand = new VistaDBCommand();
			vistaDBCommand.CommandText = insert;
			vistaDBCommand.Connection = vistaDBConnectionManager.Connection;
			if (1 == vistaDBCommand.ExecuteNonQuery())
			{
				num++;
			}
		}
		return num;
	}

	public int Delete(string deleteQuery)
	{
		using VistaDBConnectionManager vistaDBConnectionManager = ConnManager.Connect(ConnectionString);
		using VistaDBCommand vistaDBCommand = new VistaDBCommand();
		vistaDBCommand.CommandText = deleteQuery;
		vistaDBCommand.Connection = vistaDBConnectionManager.Connection;
		return vistaDBCommand.ExecuteNonQuery();
	}

	public int BulkDelete(List<string> removes)
	{
		int num = 0;
		using VistaDBConnectionManager vistaDBConnectionManager = ConnManager.Connect(ConnectionString);
		foreach (string remove in removes)
		{
			using VistaDBCommand vistaDBCommand = new VistaDBCommand();
			vistaDBCommand.CommandText = remove;
			vistaDBCommand.Connection = vistaDBConnectionManager.Connection;
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
		using (VistaDBConnectionManager vistaDBConnectionManager = ConnManager.Connect(ConnectionString))
		{
			using VistaDBCommand vistaDBCommand = new VistaDBCommand();
			vistaDBCommand.CommandText = query;
			vistaDBCommand.Connection = vistaDBConnectionManager.Connection;
			num = (int)vistaDBCommand.ExecuteScalar();
		}
		return num == 1;
	}

	public int GetTableRowCount(string query)
	{
		int num = -1;
		using VistaDBConnection vistaDBConnection = new VistaDBConnection(ConnectionString);
		vistaDBConnection.Open();
		using VistaDBCommand vistaDBCommand = new VistaDBCommand();
		vistaDBCommand.CommandText = query;
		vistaDBCommand.Connection = vistaDBConnection;
		return (int)vistaDBCommand.ExecuteScalar();
	}

	public bool TableExists(string name)
	{
		return TableExistsInner(name);
	}

	public bool CreateTable(string createQuery)
	{
		return CreateTableInner(createQuery) == 0;
	}

	public bool CreateTable(string tableName, string createQuery, bool deleteIfExists)
	{
		if (deleteIfExists && TableExistsInner(tableName) && !DropTable(tableName))
		{
			LogHelper.Instance.Log($"Unable to delete table {tableName}");
		}
		return CreateTableInner(createQuery) == 0;
	}

	public void Pack()
	{
	}

	public bool DropTable(string tableName)
	{
		string query = $"DROP TABLE {tableName}";
		return ExecuteNonQuery(query);
	}

	public void ExectuteSelectQuery(HandleSelectResult callback, string query)
	{
		using VistaDBConnectionManager vistaDBConnectionManager = ConnManager.Connect(ConnectionString);
		using VistaDBCommand vistaDBCommand = new VistaDBCommand(query, vistaDBConnectionManager.Connection);
		using VistaDBDataReader vistaDBDataReader = vistaDBCommand.ExecuteReader();
		VistaDBReader reader = new VistaDBReader(vistaDBDataReader);
		while (vistaDBDataReader.Read())
		{
			callback?.Invoke(reader);
		}
	}

	public string FormatNullableTime(DateTime? time)
	{
		if (!time.HasValue)
		{
			return "NULL";
		}
		return $"'{time.Value}'";
	}

	public void CloseConnection(bool force)
	{
		if (ConnManager != null)
		{
			try
			{
				ConnManager.Close(force);
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("VistaDBHelper.CloseConnection: caught an exception.", e);
			}
		}
	}

	public VistaDBHelper(string source, VistaDBConnectionManager mgr)
	{
		DataSource = source;
		ConnectionString = $"Data Source={DataSource};Open Mode=ExclusiveReadWrite;Pooling=false";
		ConnManager = mgr;
	}

	public VistaDBHelper(string source, bool closeOnDispose)
	{
		DataSource = source;
		ConnectionString = $"Data Source={DataSource};Open Mode=ExclusiveReadWrite;Pooling=false";
		ConnManager = new VistaDBConnectionManager(closeOnDispose);
	}

	private int CreateTableInner(string createTable)
	{
		using VistaDBConnectionManager vistaDBConnectionManager = ConnManager.Connect(ConnectionString);
		using VistaDBCommand vistaDBCommand = new VistaDBCommand();
		vistaDBCommand.CommandText = createTable;
		vistaDBCommand.Connection = vistaDBConnectionManager.Connection;
		int num = -1;
		try
		{
			return vistaDBCommand.ExecuteNonQuery();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"CreateTable {createTable} caught an exception.", e);
			return -1;
		}
	}

	private bool TableExistsInner(string name)
	{
		bool result = false;
		using (VistaDBConnectionManager vistaDBConnectionManager = ConnManager.Connect(ConnectionString))
		{
			try
			{
				vistaDBConnectionManager.Connection.GetTableSchema(name);
				result = true;
			}
			catch (Exception)
			{
				result = false;
			}
		}
		return result;
	}

	private bool ExecuteNonQuery(string query)
	{
		int num = -1;
		using (VistaDBConnectionManager vistaDBConnectionManager = ConnManager.Connect(ConnectionString))
		{
			using VistaDBCommand vistaDBCommand = new VistaDBCommand();
			vistaDBCommand.CommandText = query;
			vistaDBCommand.Connection = vistaDBConnectionManager.Connection;
			try
			{
				num = vistaDBCommand.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				num = -1;
				LogHelper.Instance.Log($"ERROR: Failed to execute Non-Query: {query} - exception {ex.Message}", LogEntryType.Error);
			}
		}
		return num >= 0;
	}
}
