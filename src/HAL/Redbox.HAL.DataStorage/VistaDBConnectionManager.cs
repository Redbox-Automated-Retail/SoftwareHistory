using System;
using VistaDB.Provider;

namespace Redbox.HAL.DataStorage;

public sealed class VistaDBConnectionManager : IDisposable
{
	private object m_lock = new object();

	internal VistaDBConnection Connection { get; private set; }

	private bool CloseConnectionOnDispose { get; set; }

	public void Dispose()
	{
		lock (m_lock)
		{
			if (CloseConnectionOnDispose)
			{
				Connection.Dispose();
				Connection = null;
			}
		}
	}

	public VistaDBConnectionManager Connect(string connectionString)
	{
		lock (m_lock)
		{
			if (Connection == null)
			{
				Connection = new VistaDBConnection(connectionString);
				Connection.Open();
			}
			return this;
		}
	}

	public void Close(bool force)
	{
		lock (m_lock)
		{
			if (Connection != null)
			{
				Connection.Dispose();
			}
		}
	}

	public VistaDBConnectionManager(bool closeOnDispose)
	{
		CloseConnectionOnDispose = closeOnDispose;
	}
}
