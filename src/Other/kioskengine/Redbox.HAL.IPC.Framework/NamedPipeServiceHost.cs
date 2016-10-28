using System;
using System.Collections.Generic;
using System.Threading;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.IPC.Framework;

internal class NamedPipeServiceHost : IPCServiceHost
{
	private readonly List<ISession> m_sessions = new List<ISession>();

	private readonly object m_sessionsSyncObject = new object();

	public override void Start()
	{
		base.Alive = true;
		LogHelper.Instance.Log(string.Format("Pipe Server accepting connections on pipe {0}", "HALService7001"));
		NamedPipeServer namedPipeServer = NamedPipeServer.CreateServer("HALService7001");
		try
		{
			while (base.Alive)
			{
				AbstractNamedPipeServerSession session = namedPipeServer.WaitForClientConnect(PipeServerSessionFactory.Instance);
				if (session != null && session is NamedPipeServerSession namedPipeServerSession)
				{
					namedPipeServerSession.ServiceHost = this;
					RegiserSession(namedPipeServerSession);
					ThreadPool.QueueUserWorkItem(delegate
					{
						session.Start();
					});
				}
			}
		}
		catch (ThreadInterruptedException)
		{
			LogHelper.Instance.Log("Server thread interrupted");
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Caught exception in NamedPipeServiceListener.Start", e);
		}
	}

	public override void Stop()
	{
		LogHelper.Instance.Log("Attempting to Stop NamedPipeServiceHost on host {0} with pipe {1}", base.Protocol.Host, "HALService7001");
		base.Alive = false;
	}

	public override void UnregisterSession(ISession session)
	{
		if (session != null)
		{
			int num = 0;
			lock (m_sessionsSyncObject)
			{
				m_sessions.Remove(session);
				num = m_sessions.Count;
			}
			LogHelper.Instance.Log(LogEntryType.Debug, "[NamedPipeServer] Unregister session - currently {0} active sessions.", num);
		}
	}

	public override void RegiserSession(ISession session)
	{
		if (session != null)
		{
			int num = 0;
			lock (m_sessionsSyncObject)
			{
				m_sessions.Add(session);
				num = m_sessions.Count;
			}
			LogHelper.Instance.Log(LogEntryType.Debug, "[NamedPipeServer] Register session - currently {0} active sessions.", num);
		}
	}
}
