using System;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.Messaging;

namespace Redbox.IPC.Framework.Messaging;

internal class MessageQueueServiceHost : Redbox.IPC.Framework.IPCServiceHost
{
	private readonly List<Redbox.IPC.Framework.Messaging.QueueWorker> m_queueWorkerList = new List<Redbox.IPC.Framework.Messaging.QueueWorker>();

	public Redbox.Messaging.IMessageProvider Provider { get; set; }

	public Redbox.Messaging.IConnectionSettings ConfigIn { get; set; }

	public Redbox.Messaging.IConnectionSettings ConfigOut { get; set; }

	public override bool Start()
	{
		base.Alive = true;
		LogHelper.Instance.Log("Start Worker Pool threads.");
		base.WorkerPool.Start();
		LogHelper.Instance.Log("Get queue provider for protocol: {0}", base.Protocol);
		Provider = Redbox.IPC.Framework.Messaging.MessageQueueConfiguration.Instance.GetProvider(base.Protocol);
		Redbox.IPC.Framework.Messaging.MessageQueueConfiguration.Instance.GetConnectionSettings(base.Protocol, out var configIn, out var configOut);
		ConfigIn = configIn;
		ConfigOut = configOut;
		LogHelper.Instance.Log("Begin processing incoming requests.", LogEntryType.Info);
		Redbox.IPC.Framework.Statistics.Instance.ServerStartTime = DateTime.Now;
		int num = 0;
		while (num++ < base.WorkerPool.MaximumThreads)
		{
			Redbox.IPC.Framework.Messaging.QueueWorker item = new Redbox.IPC.Framework.Messaging.QueueWorker(Provider, ConfigIn, ConfigOut);
			m_queueWorkerList.Add(item);
		}
		foreach (Redbox.IPC.Framework.Messaging.QueueWorker queueWorker in m_queueWorkerList)
		{
			queueWorker.Open();
		}
		m_queueWorkerList.ForEach(delegate(Redbox.IPC.Framework.Messaging.QueueWorker w)
		{
			base.WorkerPool.QueueWorkItem(delegate
			{
				DoWork(w);
			});
		});
		return true;
	}

	public override void Stop()
	{
		if (!base.Alive)
		{
			return;
		}
		base.Alive = false;
		try
		{
			base.WorkerPool.Shutdown();
			m_queueWorkerList.ForEach(delegate(Redbox.IPC.Framework.Messaging.QueueWorker w)
			{
				w.Close();
			});
			m_queueWorkerList.Clear();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was rasied in MessageQueueServiceHost.Stop", e);
		}
		LogHelper.Instance.Log("End processing incoming requests.", LogEntryType.Info);
	}

	private void DoWork(Redbox.IPC.Framework.Messaging.QueueWorker worker)
	{
		if (!base.Alive)
		{
			return;
		}
		try
		{
			Redbox.Messaging.MessagingResult messagingResult;
			if (!worker.IsOpen)
			{
				messagingResult = worker.Open();
				if (!worker.IsOpen)
				{
					return;
				}
			}
			messagingResult = worker.ReceiveRequest(out var message);
			if (!messagingResult.Success)
			{
				worker.Close(destroyConnections: true);
			}
			else if (message != null)
			{
				Redbox.IPC.Framework.Messaging.MessageQueueServerSession messageQueueServerSession = new Redbox.IPC.Framework.Messaging.MessageQueueServerSession(this);
				RegiserSession(messageQueueServerSession);
				messagingResult = messageQueueServerSession.ProcessRequest(message, (Redbox.Messaging.IMessage reply) => worker.SendReply(message, reply));
				UnregisterSession(messageQueueServerSession);
				if (!messagingResult.Success)
				{
					worker.Close(destroyConnections: true);
				}
			}
		}
		finally
		{
			if (base.Alive)
			{
				base.WorkerPool.QueueWorkItem(delegate
				{
					DoWork(worker);
				});
			}
		}
	}
}
