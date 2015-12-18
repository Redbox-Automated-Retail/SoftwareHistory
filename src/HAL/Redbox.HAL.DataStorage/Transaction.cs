using System;
using System.Collections.Generic;

namespace Redbox.HAL.DataStorage;

public struct Transaction : IDisposable
{
	private readonly TransactionType m_txType;

	private readonly List<string> m_statements;

	private bool m_disposed;

	public void Dispose()
	{
		if (!m_disposed)
		{
			m_disposed = true;
			m_statements.Clear();
			GC.SuppressFinalize(this);
		}
	}

	public void Enqueue(string statement)
	{
		m_statements.Add(statement);
	}

	public int Execute(IDataStore store)
	{
		if (store == null || m_statements.Count == 0)
		{
			return 0;
		}
		if (TransactionType.Update == m_txType)
		{
			return store.BulkUpdate(m_statements);
		}
		if (TransactionType.Insert == m_txType)
		{
			return store.BulkInsert(m_statements);
		}
		if (TransactionType.Delete == m_txType)
		{
			return store.BulkDelete(m_statements);
		}
		return 0;
	}

	public Transaction(TransactionType type)
	{
		m_disposed = false;
		m_txType = type;
		m_statements = new List<string>();
	}
}
