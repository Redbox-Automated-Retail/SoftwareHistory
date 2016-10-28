using System.Collections.Generic;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.IPC.Framework;

namespace Redbox.HAL.IPC.Framework;

internal abstract class IPCServiceHost
{
	private const int StateRunning = 1;

	private const int StateStopped = 0;

	private static IPCServiceHost m_serviceHost;

	private int m_aliveState;

	private readonly object m_sessionLockObject = new object();

	private readonly IList<Redbox.HAL.Component.Model.ISession> m_sessions = new List<Redbox.HAL.Component.Model.ISession>();

	public bool Alive
	{
		get
		{
			return Thread.VolatileRead(ref m_aliveState) == 1;
		}
		set
		{
			Interlocked.Exchange(ref m_aliveState, value ? 1 : 0);
		}
	}

	public IPCProtocol Protocol { get; protected set; }

	public string Name { get; protected set; }

	public string Version { get; protected set; }

	public string Copyright { get; protected set; }

	public X509Certificate2 Certificate { get; set; }

	public SslProtocols EncryptionProtocol { get; set; }

	public bool LogDetailedMessages { get; set; }

	public static IPCServiceHost Create(IPCProtocol protocol, string product, string version, string copyright, int minThreads, int maxThreads)
	{
		if (m_serviceHost != null)
		{
			return m_serviceHost;
		}
		if (protocol.Channel == ChannelType.NamedPipe)
		{
			m_serviceHost = new NamedPipeServiceHost();
		}
		else
		{
			if (protocol.Channel != ChannelType.Socket)
			{
				LogHelper.Instance.Log($"Unsupported channel type '{protocol.Channel}' specified; try using sockets or pipes in your protocol uri.", LogEntryType.Error);
				return null;
			}
			m_serviceHost = new TcpServiceHost();
		}
		m_serviceHost.Protocol = protocol;
		m_serviceHost.Name = product;
		m_serviceHost.Version = version;
		m_serviceHost.Copyright = copyright;
		return m_serviceHost;
	}

	public abstract void Start();

	public abstract void Stop();

	public virtual void UnregisterSession(Redbox.HAL.Component.Model.ISession session)
	{
		int num = 0;
		lock (m_sessionLockObject)
		{
			m_sessions.Remove(session);
			num = m_sessions.Count;
		}
		if (LogDetailedMessages)
		{
			LogHelper.Instance.Log($"Unregister server session - there are {num} active server sessions.", LogEntryType.Debug);
		}
	}

	public virtual void RegiserSession(Redbox.HAL.Component.Model.ISession session)
	{
		int num = 0;
		lock (m_sessionLockObject)
		{
			m_sessions.Add(session);
			num = m_sessions.Count;
		}
		if (LogDetailedMessages)
		{
			LogHelper.Instance.Log($"Register server session - there are {num} active server sessions.", LogEntryType.Debug);
		}
	}

	protected IPCServiceHost()
	{
		EncryptionProtocol = SslProtocols.None;
	}
}
