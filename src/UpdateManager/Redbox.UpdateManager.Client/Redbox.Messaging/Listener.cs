#define DEBUG
using System;
using System.Diagnostics;
using System.Threading;

namespace Redbox.Messaging;

internal class Listener<T> where T : IMessageWorker
{
	private Thread m_workerThread;

	private ManualResetEvent m_manualResetEvent;

	private bool m_isWorking;

	private readonly Func<T, MessagingResult> m_listen;

	private readonly T m_worker;

	public Action<MessagingResult, T> ListenFailed { get; set; }

	public bool IsListening { get; private set; }

	public Listener(T worker, Func<T, MessagingResult> listen)
	{
		m_worker = worker;
		m_listen = listen;
	}

	public MessagingResult Start()
	{
		Debug.Assert(!IsListening, "Listener.Start is already listening.");
		MessagingResult messagingResult = new MessagingResult();
		try
		{
			m_workerThread = new Thread(Listen)
			{
				Name = "Redbox.Messaging.Listener"
			};
			m_workerThread.Start();
			messagingResult.Success = true;
			return messagingResult;
		}
		catch (Exception exception)
		{
			messagingResult.AddError(Error.NewError("L0020", "Listener<T>.Start failed.", exception));
			return messagingResult;
		}
	}

	public MessagingResult Stop()
	{
		return Stop(wait: true);
	}

	public MessagingResult Stop(bool wait)
	{
		MessagingResult messagingResult = new MessagingResult();
		try
		{
			if (wait)
			{
				m_manualResetEvent = new ManualResetEvent(initialState: false);
			}
			IsListening = false;
			if (!wait)
			{
				messagingResult.Success = true;
				return messagingResult;
			}
			while (m_isWorking)
			{
				m_manualResetEvent.WaitOne(10);
			}
			m_manualResetEvent.Close();
			m_manualResetEvent = null;
			if (m_workerThread != null)
			{
				m_workerThread = null;
			}
			messagingResult.Success = true;
			return messagingResult;
		}
		catch (Exception exception)
		{
			messagingResult.AddError(Error.NewError("L0030", "Listener<T>.Stop failed.", exception));
			return messagingResult;
		}
	}

	public void Listen()
	{
		Debug.Assert(!IsListening, "Listener.Listen is already listening.");
		IsListening = true;
		try
		{
			while (true)
			{
				bool flag = true;
				if (!IsListening)
				{
					break;
				}
				try
				{
					m_isWorking = true;
					MessagingResult messagingResult = m_listen(m_worker);
					if (!messagingResult.Success)
					{
						RaiseListenFailed(messagingResult);
						break;
					}
				}
				catch (Exception exception)
				{
					MessagingResult messagingResult = new MessagingResult();
					messagingResult.AddError(Error.NewError("L0040", "Listener<T>.Listen failed.", exception));
					RaiseListenFailed(messagingResult);
					break;
				}
				finally
				{
					m_isWorking = false;
				}
			}
		}
		finally
		{
			IsListening = false;
			m_isWorking = false;
			if (m_manualResetEvent != null)
			{
				m_manualResetEvent.Set();
			}
		}
	}

	private void RaiseListenFailed(MessagingResult messagingResult)
	{
		if (ListenFailed != null)
		{
			Thread thread = new Thread((ThreadStart)delegate
			{
				ListenFailed(messagingResult, m_worker);
			});
			thread.Name = "Listener.Listen failed";
			Thread thread2 = thread;
			thread2.Start();
		}
	}
}
