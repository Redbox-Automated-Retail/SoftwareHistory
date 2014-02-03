using System.Collections.Generic;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Redbox.Core;
using Redbox.IPC.Framework.Messaging;
using Redbox.IPC.Framework.NamedPipes;
using Redbox.IPC.Framework.Remoting;
using Redbox.IPC.Framework.Sockets;

namespace Redbox.IPC.Framework;

internal abstract class IPCServiceHost
{
	private static Redbox.IPC.Framework.IPCServiceHost m_serviceHost;

	private readonly object m_syncObject = new object();

	private readonly IList<Redbox.IPC.Framework.ISession> m_sessions = new List<Redbox.IPC.Framework.ISession>();

	public bool Alive { get; set; }

	public string Name { get; protected set; }

	public string Version { get; protected set; }

	public string Copyright { get; protected set; }

	public IPCProtocol Protocol { get; protected set; }

	public WorkerPool WorkerPool { get; internal set; }

	public X509Certificate2 Certificate { get; set; }

	public SslProtocols EncryptionProtocol { get; set; }

	public IDictionary<string, object> Properties { get; private set; }

	public static Redbox.IPC.Framework.IPCServiceHost Create(IPCProtocol protocol, string product, string version, string copyright, int minThreads, int maxThreads, int bufferSize, int numberOfBuffers, int? backlogSize)
	{
		switch (protocol.Channel)
		{
		case Redbox.IPC.Framework.ChannelType.NamedPipe:
			m_serviceHost = new Redbox.IPC.Framework.NamedPipes.NamedPipeServiceHost();
			break;
		case Redbox.IPC.Framework.ChannelType.Socket:
			m_serviceHost = new Redbox.IPC.Framework.Sockets.TcpServiceHost(backlogSize);
			break;
		case Redbox.IPC.Framework.ChannelType.ActiveMQ:
		case Redbox.IPC.Framework.ChannelType.MSMQ:
		case Redbox.IPC.Framework.ChannelType.OracleMQ:
		case Redbox.IPC.Framework.ChannelType.TibcoEMS:
		case Redbox.IPC.Framework.ChannelType.WebSphereMQ:
			m_serviceHost = new Redbox.IPC.Framework.Messaging.MessageQueueServiceHost();
			break;
		case Redbox.IPC.Framework.ChannelType.Remoting:
			m_serviceHost = new Redbox.IPC.Framework.Remoting.RemotingServiceHost(maxThreads);
			break;
		default:
			LogHelper.Instance.Log($"Unsupported channel type '{protocol.Channel}' specified; try using sockets or pipes in your protocol uri.", LogEntryType.Error);
			return null;
		}
		BufferPool.Instance.Initialize(bufferSize, numberOfBuffers);
		m_serviceHost.WorkerPool = new WorkerPool(minThreads, maxThreads);
		m_serviceHost.Name = product;
		m_serviceHost.Version = version;
		m_serviceHost.Protocol = protocol;
		m_serviceHost.Copyright = copyright;
		return m_serviceHost;
	}

	public abstract bool Start();

	public abstract void Stop();

	public virtual void UnregisterSession(Redbox.IPC.Framework.ISession session)
	{
		UnregisterSession(session, cleanupAbandonedSessions: true);
	}

	public virtual void UnregisterSession(Redbox.IPC.Framework.ISession session, bool cleanupAbandonedSessions)
	{
		LogHelper.Instance.Log("Unregister server session.", LogEntryType.Debug);
		int count;
		lock (m_syncObject)
		{
			if (!m_sessions.Remove(session))
			{
				return;
			}
			if (cleanupAbandonedSessions)
			{
				for (int i = 0; i < m_sessions.Count; i++)
				{
					if (m_sessions[i] == null || !m_sessions[i].IsConnected())
					{
						m_sessions.RemoveAt(i);
						LogHelper.Instance.Log("Unregister abandon server session.", LogEntryType.Debug);
					}
				}
			}
			count = m_sessions.Count;
		}
		LogHelper.Instance.Log($"There are {count} server sessions active.", LogEntryType.Debug);
	}

	public virtual void RegiserSession(Redbox.IPC.Framework.ISession session)
	{
		int count;
		lock (m_syncObject)
		{
			foreach (KeyValuePair<string, object> property in Properties)
			{
				session.Properties.Add(property);
			}
			m_sessions.Add(session);
			count = m_sessions.Count;
		}
		LogHelper.Instance.Log($"Register server session - there are {count} active server sessions.", LogEntryType.Debug);
	}

	protected IPCServiceHost()
	{
		EncryptionProtocol = SslProtocols.None;
		Properties = new Dictionary<string, object>();
	}
}
