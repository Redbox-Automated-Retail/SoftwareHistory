using System;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using Redbox.Core;

namespace Redbox.IPC.Framework.Remoting;

internal class RemotingHost : MarshalByRefObject, Redbox.IPC.Framework.Remoting.IRemotingHost
{
	private static long m_connectionCount;

	public string ExecuteCommand(string commandString)
	{
		Redbox.IPC.Framework.Remoting.RemotingServiceHost remotingServiceHost = null;
		Redbox.IPC.Framework.Remoting.RemotingServerSession remotingServerSession = null;
		using ExecutionTimer timer = new ExecutionTimer();
		try
		{
			Interlocked.Add(ref Redbox.IPC.Framework.Statistics.Instance.NumberOfBytesReceived, commandString.Length);
			remotingServiceHost = (Redbox.IPC.Framework.Remoting.RemotingServiceHost)CallContext.GetData("RemotingHost");
			if (Interlocked.Read(ref m_connectionCount) >= remotingServiceHost.MaxThreads)
			{
				return Redbox.IPC.Framework.CommandService.Instance.GetServerBusyResult(timer, remotingServiceHost.MaxThreads);
			}
			Interlocked.Increment(ref m_connectionCount);
			LogHelper.Instance.Log($"There are {Interlocked.Read(ref m_connectionCount)} connections active", LogEntryType.Debug);
			remotingServerSession = new Redbox.IPC.Framework.Remoting.RemotingServerSession();
			string text = remotingServerSession.ProcessRequest(commandString);
			Interlocked.Add(ref Redbox.IPC.Framework.Statistics.Instance.NumberOfBytesSent, text.Length);
			Interlocked.Decrement(ref m_connectionCount);
			return text;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("RemotingHost execution error", e);
			return null;
		}
	}
}
