using System;
using System.Collections.Generic;

namespace Redbox.HAL.DataStorage;

internal sealed class ThreadSafeVistaDBStore : IDataStore
{
	private readonly object Lock;

	private readonly IDataStore Impl;

	public bool Update(string update)
	{
		lock (Lock)
		{
			return Impl.Update(update);
		}
	}

	public int BulkUpdate(List<string> update)
	{
		lock (Lock)
		{
			return Impl.BulkUpdate(update);
		}
	}

	public bool Insert(string insert)
	{
		lock (Lock)
		{
			return Impl.Insert(insert);
		}
	}

	public int BulkInsert(List<string> inserts)
	{
		lock (Lock)
		{
			return Impl.BulkInsert(inserts);
		}
	}

	public int Delete(string deleteQuery)
	{
		lock (Lock)
		{
			return Impl.Delete(deleteQuery);
		}
	}

	public int BulkDelete(List<string> removes)
	{
		lock (Lock)
		{
			return Impl.BulkDelete(removes);
		}
	}

	public bool RowExists(string query)
	{
		lock (Lock)
		{
			return Impl.RowExists(query);
		}
	}

	public int GetTableRowCount(string query)
	{
		lock (Lock)
		{
			return Impl.GetTableRowCount(query);
		}
	}

	public bool TableExists(string name)
	{
		lock (Lock)
		{
			return Impl.TableExists(name);
		}
	}

	public bool CreateTable(string createTable)
	{
		lock (Lock)
		{
			return Impl.CreateTable(createTable);
		}
	}

	public bool DropTable(string tableName)
	{
		lock (Lock)
		{
			return Impl.DropTable(tableName);
		}
	}

	public void ExectuteSelectQuery(HandleSelectResult callback, string query)
	{
		lock (Lock)
		{
			Impl.ExectuteSelectQuery(callback, query);
		}
	}

	public void CloseConnection(bool force)
	{
	}

	public string FormatNullableTime(DateTime? time)
	{
		lock (Lock)
		{
			return Impl.FormatNullableTime(time);
		}
	}

	internal ThreadSafeVistaDBStore(string source)
	{
		Impl = new NoLockVistaDBStore(source);
		Lock = new object();
	}
}
