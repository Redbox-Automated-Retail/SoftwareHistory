using System;
using System.Collections.Generic;

namespace Redbox.HAL.DataStorage;

public sealed class Transaction : IDisposable
{
	private TransactionType m_txType;

	private VistaDBHelper m_helper;

	private List<string> m_statements = new List<string>();

	public void Dispose()
	{
		m_statements.Clear();
		GC.SuppressFinalize(this);
	}

	public void Enqueue(string statement)
	{
		m_statements.Add(statement);
	}

	public bool Execute()
	{
		return ExecuteInner(m_helper);
	}

	public bool Execute(VistaDBHelper helper)
	{
		return ExecuteInner(helper);
	}

	public Transaction(VistaDBHelper helper, TransactionType type)
	{
		m_txType = type;
		m_helper = helper;
	}

	public Transaction(TransactionType type)
	{
		m_txType = type;
	}

	private bool ExecuteInner(VistaDBHelper helper)
	{
		if (TransactionType.Update == m_txType)
		{
			return helper.BulkUpdate(m_statements) == m_statements.Count;
		}
		if (TransactionType.Insert == m_txType)
		{
			return helper.BulkInsert(m_statements) == m_statements.Count;
		}
		if (TransactionType.Delete == m_txType)
		{
			return helper.BulkDelete(m_statements) == m_statements.Count;
		}
		return false;
	}
}
