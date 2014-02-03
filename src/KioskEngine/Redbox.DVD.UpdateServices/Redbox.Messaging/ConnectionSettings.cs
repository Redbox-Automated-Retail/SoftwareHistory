using System.Collections.Generic;
using Redbox.Core;

namespace Redbox.Messaging;

internal class ConnectionSettings : Redbox.Messaging.IConnectionSettings
{
	private bool m_transactionalMessages = true;

	private Redbox.Messaging.DestinationType m_destinationType = Redbox.Messaging.DestinationType.Queue;

	private readonly Dictionary<string, string> m_properties = new Dictionary<string, string>();

	private int m_maxConnectionPoolSize = 100;

	private int m_minConnectionPoolSize = 20;

	public string Name { get; set; }

	public string ConnectionString { get; set; }

	public string DestinationName { get; set; }

	public bool TransactionalMessages
	{
		get
		{
			return m_transactionalMessages;
		}
		set
		{
			m_transactionalMessages = value;
		}
	}

	public Redbox.Messaging.DestinationType DestinationType
	{
		get
		{
			return m_destinationType;
		}
		set
		{
			m_destinationType = value;
		}
	}

	public string UserName { get; set; }

	public string Password { get; set; }

	public Dictionary<string, string> Properties
	{
		get
		{
			return m_properties;
		}
		set
		{
			m_properties.Clear();
			if (value != null)
			{
				LinqExtensions.ForEach(value, delegate(KeyValuePair<string, string> pair)
				{
					m_properties.Add(pair.Key, pair.Value);
				});
			}
		}
	}

	public int MaxConnectionPoolSize
	{
		get
		{
			return m_maxConnectionPoolSize;
		}
		set
		{
			m_maxConnectionPoolSize = value;
		}
	}

	public int MinConnectionPoolSize
	{
		get
		{
			return m_minConnectionPoolSize;
		}
		set
		{
			m_minConnectionPoolSize = value;
		}
	}
}
