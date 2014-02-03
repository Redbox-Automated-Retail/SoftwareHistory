using System.Collections.Generic;
using System.Linq;

namespace Redbox.Messaging;

internal class ConnectionConfiguration
{
	private bool m_transactionalMessages = true;

	private DestinationType m_destinationType = DestinationType.Queue;

	private readonly Dictionary<string, string> m_properties = new Dictionary<string, string>();

	private int m_maxConnectionPoolSize = 100;

	private int m_minConnectionPoolSize = 20;

	public string Name { get; set; }

	public string ProviderName { get; set; }

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

	public DestinationType DestinationType
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
			value.ToList().ForEach(delegate(KeyValuePair<string, string> p)
			{
				m_properties.Add(p.Key, p.Value);
			});
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
