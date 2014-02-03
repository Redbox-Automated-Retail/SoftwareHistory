using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Redbox.Core;
using Redbox.TicketServices.Proxy.ComponentModel;
using VistaDB;
using VistaDB.DDA;
using VistaDB.Provider;

namespace Redbox.TicketServices.Proxy;

internal class DatabaseCache : IDisposable
{
	private string _path;

	private IVistaDBDDA _dda;

	private IVistaDBDatabase _database;

	public DatabaseCache(string path)
	{
		_path = path;
	}

	public bool Open()
	{
		VistaDBDatabaseOpenMode mode = VistaDBDatabaseOpenMode.NonexclusiveReadWrite;
		try
		{
			if (!File.Exists(_path))
			{
				LogHelper.Instance.Log("TicketsService database: {0} does not exist", _path);
				return false;
			}
			_dda = VistaDBEngine.Connections.OpenDDA();
			_database = _dda.OpenDatabase(_path, mode, null);
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Unhandled exception in TicketDataService.OpenDatabaseCache.", e);
			return false;
		}
	}

	public bool Close()
	{
		try
		{
			if (_database != null)
			{
				_database.Dispose();
				_database = null;
			}
			if (_dda != null)
			{
				_dda.Dispose();
				_dda = null;
			}
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Unhandled exception in TicketDataService.CloseDatabaseCache.", e);
			return false;
		}
	}

	public ITicketDataTable GetData(string tableName, string filter1, long[] filterValues1, string filter2, Func<object, object, bool> filterExpression)
	{
		DateTime now = DateTime.Now;
		TicketDataTable ticketDataTable = new TicketDataTable();
		ticketDataTable.Data = new Dictionary<string, object[]>();
		ticketDataTable.DataList = new List<object[]>();
		ticketDataTable.TableName = tableName;
		LogHelper.Instance.Log("Tickets data service GetData({0})", tableName);
		try
		{
			if (!_database.ContainsTable(tableName))
			{
				LogHelper.Instance.Log("Tickets data service GetData({0}). Table does not exist.", tableName);
				return null;
			}
			ticketDataTable.ColumnNames = GetColumns(tableName);
			using (IVistaDBTable vistaDBTable = _database.OpenTable(tableName, exclusive: false, readOnly: true))
			{
				vistaDBTable.Next();
				IVistaDBTableSchema vistaDBTableSchema = _database.TableSchema(tableName);
				int columnCount = vistaDBTableSchema.ColumnCount;
				if (columnCount > 1)
				{
					int columnPos = GetColumnPos(filter1, ticketDataTable.ColumnNames.ToList());
					int columnPos2 = GetColumnPos(filter2, ticketDataTable.ColumnNames.ToList());
					int columnPos3 = GetColumnPos("Key", ticketDataTable.ColumnNames.ToList());
					if (columnPos3 == -1)
					{
						return null;
					}
					while (!vistaDBTable.EndOfTable)
					{
						string text = vistaDBTable.CurrentRow[columnPos3].Value as string;
						if (string.IsNullOrEmpty(text))
						{
							vistaDBTable.Next();
							continue;
						}
						if (filterExpression != null)
						{
							if (!filterExpression(vistaDBTable.CurrentRow[columnPos].Value, vistaDBTable.CurrentRow[columnPos2].Value))
							{
								vistaDBTable.Next();
								continue;
							}
						}
						else if (columnPos > -1)
						{
							long value = (long)vistaDBTable.CurrentRow[columnPos].Value;
							if (!filterValues1.Contains(value))
							{
								vistaDBTable.Next();
								continue;
							}
						}
						object[] array = new object[columnCount - 1];
						for (int i = 1; i < columnCount; i++)
						{
							array[i - 1] = vistaDBTable.CurrentRow[i].Value;
						}
						ticketDataTable.Data[text] = array;
						ticketDataTable.DataList.Add(array);
						vistaDBTable.Next();
					}
				}
			}
			TimeSpan timeSpan = DateTime.Now - now;
			LogHelper.Instance.Log("Tickets data service GetData({0}) complete.  Rows: {1} Milliseconds: {2}", tableName, ticketDataTable.DataList.Count, timeSpan.TotalMilliseconds);
			return ticketDataTable;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"Exception in TicketDataService.GetData({tableName})", e);
			throw;
		}
	}

	public ITicketDataTable GetSqlData(string tableName, string sql)
	{
		DateTime now = DateTime.Now;
		TicketDataTable ticketDataTable = new TicketDataTable();
		ticketDataTable.Data = new Dictionary<string, object[]>();
		ticketDataTable.DataList = new List<object[]>();
		ticketDataTable.TableName = tableName;
		LogHelper.Instance.Log("Tickets data service GetSqlData({0})", tableName);
		try
		{
			if (!_database.ContainsTable(tableName))
			{
				LogHelper.Instance.Log("Tickets data service GetSqlData({0}). Table does not exist.", tableName);
				return null;
			}
			ticketDataTable.ColumnNames = GetColumns(tableName);
			DataTable dataTable = new DataTable();
			VistaDBConnection conn = new VistaDBConnection("Open Mode=NonExclusiveReadWrite;Data Source=" + _path);
			using (VistaDBDataAdapter vistaDBDataAdapter = new VistaDBDataAdapter("SET OPTIMIZATION ON; " + sql, conn))
			{
				vistaDBDataAdapter.Fill(dataTable);
			}
			int count = dataTable.Columns.Count;
			if (count > 1)
			{
				int columnPos = GetColumnPos("Key", ticketDataTable.ColumnNames.ToList());
				if (columnPos == -1)
				{
					return null;
				}
				foreach (DataRow row in dataTable.Rows)
				{
					string key = row[columnPos] as string;
					object[] array = new object[count - 1];
					for (int i = 1; i < count; i++)
					{
						if (DBNull.Value.Equals(row[i]))
						{
							array[i - 1] = null;
						}
						else
						{
							array[i - 1] = row[i];
						}
					}
					ticketDataTable.Data[key] = array;
					ticketDataTable.DataList.Add(array);
				}
			}
			TimeSpan timeSpan = DateTime.Now - now;
			LogHelper.Instance.Log("Tickets data service GetSqlData({0}) complete.  Rows: {1} Milliseconds: {2}", tableName, ticketDataTable.DataList.Count, timeSpan.TotalMilliseconds);
			return ticketDataTable;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"Exception in TicketDataService.GetSqlData({tableName})", e);
			throw;
		}
	}

	private int GetColumnPos(string columnName, List<string> columnNames)
	{
		if (string.IsNullOrEmpty(columnName))
		{
			return -1;
		}
		return columnNames.FindIndex((string s) => s.Equals(columnName, StringComparison.CurrentCultureIgnoreCase));
	}

	public List<string> GetTables()
	{
		List<string> list = new List<string>();
		if (_database == null)
		{
			LogHelper.Instance.Log("TicketDataService.GetTables failed. Database is null.");
			return null;
		}
		try
		{
			foreach (string item in _database.EnumTables())
			{
				list.Add(item);
			}
			return list;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"Exception in TicketsDataService.GetTables()", e);
			return null;
		}
	}

	public List<string> GetColumns(string tableName)
	{
		List<string> list = new List<string>();
		if (_database == null)
		{
			LogHelper.Instance.Log("TicketDataService.GetColumns failed. Database is null.");
			return null;
		}
		try
		{
			using IVistaDBTableSchema vistaDBTableSchema = _database.TableSchema(tableName);
			foreach (IVistaDBColumnAttributes item in vistaDBTableSchema)
			{
				list.Add(item.Name);
			}
			return list;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"Exception in TicketsDataService.GetColumns()", e);
			return null;
		}
	}

	public void Dispose()
	{
		Close();
	}
}
