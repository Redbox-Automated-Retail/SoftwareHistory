using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Redbox.Core;

namespace Redbox.Messaging;

internal class ConnectionPool<T>
{
	private class ConnectionLists
	{
		private readonly List<PoolConnection> m_list = new List<PoolConnection>();

		private readonly Queue<PoolConnection> m_queue = new Queue<PoolConnection>();

		public List<PoolConnection> List => m_list;

		public Redbox.Messaging.IConnectionSettings Configuration { get; set; }

		private Queue<PoolConnection> Queue => m_queue;

		public void Add(T connection, Action<T> close)
		{
			PoolConnection poolConnection = new PoolConnection();
			poolConnection.Connection = connection;
			poolConnection.Close = close;
			poolConnection.CheckedOut = true;
			PoolConnection item = poolConnection;
			List.Add(item);
		}

		public T CheckOut()
		{
			if (Queue.Count == 0)
			{
				return default(T);
			}
			PoolConnection poolConnection = Queue.Dequeue();
			poolConnection.CheckedOut = true;
			return poolConnection.Connection;
		}

		public Redbox.Messaging.MessagingResult CheckIn(T connection)
		{
			return CheckIn(connection, repool: true);
		}

		public Redbox.Messaging.MessagingResult CheckIn(T connection, bool repool)
		{
			Redbox.Messaging.MessagingResult messagingResult = new Redbox.Messaging.MessagingResult();
			PoolConnection poolConnection = List.Find((PoolConnection pc) => (object)pc.Connection == (object)connection);
			poolConnection.CheckedOut = false;
			if (!repool || List.Count > Configuration.MinConnectionPoolSize)
			{
				List.Remove(poolConnection);
				if (poolConnection.Close != null)
				{
					try
					{
						poolConnection.Close(connection);
					}
					catch (Exception exception)
					{
						messagingResult.AddError(Redbox.Messaging.Error.NewError("CP0010", "ConnectionPool.ReturnConnection failed closing a connection.", exception));
					}
				}
			}
			else
			{
				Queue.Enqueue(poolConnection);
			}
			if (messagingResult.Errors.Count == 0)
			{
				messagingResult.Success = true;
			}
			return messagingResult;
		}
	}

	private class PoolConnection
	{
		public T Connection { get; set; }

		public Action<T> Close { get; set; }

		public bool CheckedOut { get; set; }
	}

	private readonly Dictionary<string, ConnectionLists> m_poolList = new Dictionary<string, ConnectionLists>();

	private readonly object m_lock = new object();

	public static Redbox.Messaging.ConnectionPool<T> Instance => Singleton<Redbox.Messaging.ConnectionPool<T>>.Instance;

	private Dictionary<string, ConnectionLists> PoolList => m_poolList;

	public T GetConnection(Redbox.Messaging.IConnectionSettings configuration)
	{
		lock (m_lock)
		{
			string hash = GetHash(configuration);
			if (!PoolList.ContainsKey(hash))
			{
				return default(T);
			}
			ConnectionLists connectionLists = PoolList[hash];
			return connectionLists.CheckOut();
		}
	}

	public Redbox.Messaging.MessagingResult ReturnConnection(Redbox.Messaging.IConnectionSettings configuration, T connection, bool repool)
	{
		lock (m_lock)
		{
			string hash = GetHash(configuration);
			ConnectionLists connectionLists = PoolList[hash];
			return connectionLists.CheckIn(connection, repool);
		}
	}

	public void AddConnection(Redbox.Messaging.IConnectionSettings configuration, T connection, Action<T> close)
	{
		lock (m_lock)
		{
			string hash = GetHash(configuration);
			if (!PoolList.ContainsKey(hash))
			{
				PoolList.Add(hash, new ConnectionLists
				{
					Configuration = configuration
				});
			}
			ConnectionLists connectionLists = PoolList[hash];
			connectionLists.Add(connection, close);
		}
	}

	public bool IsMaxConnections(Redbox.Messaging.IConnectionSettings configuration)
	{
		lock (m_lock)
		{
			string hash = GetHash(configuration);
			if (!PoolList.ContainsKey(hash))
			{
				return false;
			}
			ConnectionLists connectionLists = PoolList[hash];
			return configuration.MaxConnectionPoolSize <= connectionLists.List.Count;
		}
	}

	public Redbox.Messaging.MessagingResult ClearPool()
	{
		lock (m_lock)
		{
			Redbox.Messaging.MessagingResult result = new Redbox.Messaging.MessagingResult();
			LinqExtensions.ForEach(PoolList, delegate(KeyValuePair<string, ConnectionLists> pair)
			{
				pair.Value.List.ForEach(delegate(PoolConnection pc)
				{
					if (pc.Close == null)
					{
						return;
					}
					try
					{
						pc.Close(pc.Connection);
					}
					catch (Exception exception)
					{
						result.AddError(Redbox.Messaging.Error.NewError("CP0020", "ConnectionPool.ClearPool failed closing a connection.", exception));
					}
				});
			});
			PoolList.Clear();
			if (result.Errors.Count == 0)
			{
				result.Success = true;
			}
			return result;
		}
	}

	public Redbox.Messaging.MessagingResult ClearPool(Redbox.Messaging.IConnectionSettings configuration)
	{
		Redbox.Messaging.MessagingResult result = new Redbox.Messaging.MessagingResult();
		lock (m_lock)
		{
			string hash = GetHash(configuration);
			if (!PoolList.ContainsKey(hash))
			{
				result.Success = true;
				return result;
			}
			ConnectionLists connectionLists = PoolList[hash];
			connectionLists.List.ForEach(delegate(PoolConnection pc)
			{
				if (pc.Close == null)
				{
					return;
				}
				try
				{
					pc.Close(pc.Connection);
				}
				catch (Exception exception)
				{
					result.AddError(Redbox.Messaging.Error.NewError("CP0020", "ConnectionPool.ClearPool failed closing a connection.", exception));
				}
			});
			PoolList.Remove(hash);
			if (result.Errors.Count == 0)
			{
				result.Success = true;
			}
			return result;
		}
	}

	private ConnectionPool()
	{
	}

	private string GetHash(Redbox.Messaging.IConnectionSettings configuration)
	{
		SHA256Managed sHA256Managed = new SHA256Managed();
		string result = Convert.ToBase64String(sHA256Managed.ComputeHash(sHA256Managed.ComputeHash(Encoding.UTF8.GetBytes(ObjectExtensions.ToJson(configuration)))));
		sHA256Managed.Clear();
		return result;
	}
}
