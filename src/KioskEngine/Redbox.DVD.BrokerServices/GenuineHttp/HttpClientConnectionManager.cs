using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using Belikov.Common.ThreadProcessing;
using Belikov.GenuineChannels.Connection;
using Belikov.GenuineChannels.DotNetRemotingLayer;
using Belikov.GenuineChannels.Logbook;
using Belikov.GenuineChannels.Messaging;
using Belikov.GenuineChannels.Parameters;
using Belikov.GenuineChannels.Security;
using Belikov.GenuineChannels.TransportContext;
using Belikov.GenuineChannels.Utilities;

namespace Belikov.GenuineChannels.GenuineHttp;

internal class HttpClientConnectionManager : ConnectionManager, ITimerConsumer
{
	private class FailedHttpRequest
	{
		public HttpClientConnection HttpClientConnection;

		public Exception Exception;

		public bool Sender;
	}

	private Hashtable _invocation = Hashtable.Synchronized(new Hashtable());

	private Hashtable _persistent = Hashtable.Synchronized(new Hashtable());

	private int _hostRenewingSpan;

	private WaitCallback _pool_Sender_OnEndSending_ContinueExchange;

	private WaitCallback _internal_TimerCallback;

	private TimeSpan _webRequestInitiationTimeout;

	private int _httpAsynchronousRequestTimeout;

	private WaitCallback _listener_OnEndReceiving_ContinueExchange;

	public HttpClientConnectionManager(ITransportContext iTransportContext)
		: base(iTransportContext)
	{
		Local = new HostInformation("_ghttp://" + iTransportContext.HostIdentifier, iTransportContext);
		_hostRenewingSpan = GenuineUtility.ConvertToMilliseconds(iTransportContext.IParameterProvider[GenuineParameter.ClosePersistentConnectionAfterInactivity]) + GenuineUtility.ConvertToMilliseconds(iTransportContext.IParameterProvider[GenuineParameter.MaxTimeSpanToReconnect]);
		_internal_TimerCallback = Internal_TimerCallback;
		_pool_Sender_OnEndSending_ContinueExchange = Pool_Sender_OnEndSending_ContinueExchange;
		_listener_OnEndReceiving_ContinueExchange = Pool_Sender_OnEndSending_ContinueExchange;
		_webRequestInitiationTimeout = (TimeSpan)iTransportContext.IParameterProvider[GenuineParameter.HttpWebRequestInitiationTimeout];
		_httpAsynchronousRequestTimeout = GenuineUtility.ConvertToMilliseconds(iTransportContext.IParameterProvider[GenuineParameter.HttpAsynchronousRequestTimeout]);
		TimerProvider.Attach(this);
	}

	protected override void InternalSend(Message message)
	{
		switch (message.SecuritySessionParameters.GenuineConnectionType)
		{
		case GenuineConnectionType.Persistent:
			InternalSend(message, Pool_GetConnectionForSending(message));
			break;
		case GenuineConnectionType.Named:
			throw new NotSupportedException("Genuine HTTP client connection manager doesn't support named connections.");
		case GenuineConnectionType.Invocation:
		{
			HttpInvocationConnection httpInvocationConnection = FindInvocationConnection(message);
			httpInvocationConnection.SendMessage(message);
			break;
		}
		case GenuineConnectionType.Persistent | GenuineConnectionType.Named:
			break;
		}
	}

	private void InternalSend(Message message, HttpClientConnection httpClientConnection)
	{
		bool flag = false;
		lock (httpClientConnection.SendingLock)
		{
			if (!httpClientConnection.IsSent)
			{
				httpClientConnection.IsSent = true;
				flag = true;
			}
			else
			{
				httpClientConnection.MessageContainer.AddMessage(message, onlyCheckIn: false);
			}
		}
		if (flag)
		{
			LowLevel_Sender_Send(message, httpClientConnection, httpClientConnection.OnEndSending, repeatSending: false);
		}
	}

	private HttpInvocationConnection FindInvocationConnection(Message message)
	{
		lock (_invocation.SyncRoot)
		{
			HttpInvocationConnection httpInvocationConnection = _invocation[message.Recipient.Url] as HttpInvocationConnection;
			if (httpInvocationConnection == null)
			{
				httpInvocationConnection = (HttpInvocationConnection)(_invocation[message.Recipient.Url] = new HttpInvocationConnection(ITransportContext, message.Recipient));
			}
			return httpInvocationConnection;
		}
	}

	private HttpClientConnection Pool_GetConnectionForSending(Message message)
	{
		HttpClientConnection httpClientConnection = null;
		ReaderAutoLocker readerAutoLocker = new ReaderAutoLocker(_disposeLock);
		try
		{
			if (base._disposed)
			{
				throw OperationException.WrapException(_disposeReason);
			}
		}
		finally
		{
			readerAutoLocker.Dispose();
		}
		string text = message.ConnectionName;
		if (text == null || text.Length <= 0)
		{
			text = "$/__GC/" + message.Recipient.PrimaryUri;
		}
		if (!Monitor.TryEnter(message.Recipient.PersistentConnectionEstablishingLock, GenuineUtility.GetMillisecondsLeft(message.FinishTime)))
		{
			throw GenuineExceptions.Get_Send_Timeout();
		}
		try
		{
			lock (_persistent.SyncRoot)
			{
				httpClientConnection = _persistent[message.Recipient.Url] as HttpClientConnection;
				if (httpClientConnection != null && httpClientConnection._disposed)
				{
					_persistent.Remove(message.Recipient.Url);
					httpClientConnection = null;
				}
				if (httpClientConnection != null)
				{
					return httpClientConnection;
				}
			}
			httpClientConnection = LowLevel_OpenConnection(message.Recipient, message.SecuritySessionParameters.GenuineConnectionType, text);
			readerAutoLocker = new ReaderAutoLocker(_disposeLock);
			try
			{
				if (base._disposed)
				{
					httpClientConnection.Dispose(_disposeReason);
					throw OperationException.WrapException(_disposeReason);
				}
				_persistent[message.Recipient.Url] = httpClientConnection;
			}
			finally
			{
				readerAutoLocker.Dispose();
			}
			httpClientConnection.SignalState(GenuineEventType.GeneralConnectionEstablished, null, null);
			httpClientConnection.MessageContainer = new MessageContainer(ITransportContext);
		}
		finally
		{
			Monitor.Exit(message.Recipient.PersistentConnectionEstablishingLock);
		}
		return httpClientConnection;
	}

	public void Pool_Sender_OnEndSending(HttpClientConnection httpClientConnection, HttpWebResponse httpWebResponse)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		Stream stream = null;
		try
		{
			httpClientConnection.Remote.Renew(_hostRenewingSpan, canMakeShorter: false);
			httpClientConnection.LastMessageWasReceviedAt = GenuineUtility.TickCount;
			httpClientConnection.SignalState(GenuineEventType.GeneralConnectionEstablished, null, null);
			httpClientConnection.SentContent.Close();
			httpClientConnection.SentContent = null;
			httpClientConnection.MessagesBeingSent.Clear();
			stream = httpWebResponse.GetResponseStream();
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.LowLevelTransport] > 0)
			{
				GenuineChunkedStream genuineChunkedStream = null;
				if (binaryLogWriter[LogCategory.Transport] > 1)
				{
					genuineChunkedStream = new GenuineChunkedStream(releaseOnReadMode: false);
					GenuineUtility.CopyStreamToStream(stream, genuineChunkedStream, (int)httpWebResponse.ContentLength);
				}
				binaryLogWriter.WriteTransportContentEvent(LogCategory.LowLevelTransport, "HttpClientConnectionManager.Pool_Sender_OnEndSending", LogMessageType.AsynchronousSendingFinished, null, null, httpClientConnection.Remote, (binaryLogWriter[LogCategory.LowLevelTransport] > 1) ? genuineChunkedStream : null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, httpClientConnection.DbgConnectionId, (int)httpWebResponse.ContentLength, null, null, null, "The content of the response received by the sender connection.");
				if (binaryLogWriter[LogCategory.Transport] > 1)
				{
					stream = genuineChunkedStream;
				}
			}
			BinaryReader binaryReader = new BinaryReader(stream);
			HttpMessageCoder.ReadResponseHeader(binaryReader, out var serverUri, out var sequenceNo, out var httpPacketType, out var remoteHostUniqueIdentifier);
			if (sequenceNo != httpClientConnection.SendSequenceNo)
			{
				throw GenuineExceptions.Get_Receive_IncorrectData();
			}
			if (httpClientConnection.GenuineConnectionType == GenuineConnectionType.Persistent)
			{
				httpClientConnection.Remote.UpdateUri(serverUri, remoteHostUniqueIdentifier);
			}
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.LowLevelTransport] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.LowLevelTransport, "HttpClientConnectionManager.Pool_Sender_OnEndSending", LogMessageType.LowLevelTransport_AsyncSendingCompleted, null, null, httpClientConnection.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, httpClientConnection.Sender_ConnectionLevelSecurity, (httpClientConnection.Sender_ConnectionLevelSecurity == null) ? null : httpClientConnection.Sender_ConnectionLevelSecurity.Name, httpClientConnection.DbgConnectionId, (int)httpPacketType, sequenceNo, 0, null, null, null, null, "SENDER invocation completed. Type of the packet: {0}. Server Uri: {1}. Seq No: {2}", Enum.Format(typeof(HttpPacketType), httpPacketType, "g"), serverUri, sequenceNo);
			}
			switch (httpPacketType)
			{
			case HttpPacketType.Desynchronization:
				ConnectionFailed(httpClientConnection, sender: true, GenuineExceptions.Get_Channel_Desynchronization(), tryToReestablish: false);
				return;
			case HttpPacketType.SenderError:
			{
				Exception ex = null;
				try
				{
					BinaryFormatter binaryFormatter = new BinaryFormatter(new RemotingSurrogateSelector(), new StreamingContext(StreamingContextStates.Other));
					ex = binaryFormatter.Deserialize(stream) as Exception;
				}
				catch
				{
				}
				if (ex != null)
				{
					ConnectionFailed(httpClientConnection, sender: true, GenuineExceptions.Get_Receive_ConnectionClosed(ex.Message), tryToReestablish: false);
				}
				else
				{
					ConnectionFailed(httpClientConnection, sender: true, GenuineExceptions.Get_Receive_ConnectionClosed("Remote host was not able to parse the request. Probably due to the security reasons."), tryToReestablish: false);
				}
				return;
			}
			default:
				LowLevel_ParseLabelledStream(stream, httpClientConnection.Sender_ReceiveBuffer, httpClientConnection, httpClientConnection.Sender_ConnectionLevelSecurity);
				break;
			}
		}
		finally
		{
			stream?.Close();
			httpWebResponse.Close();
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Debugging] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Debugging, "HttpClientConnectionManager.Pool_Sender_OnEndSending", LogMessageType.DebuggingSuccess, null, null, httpClientConnection.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, httpClientConnection.Sender_ConnectionLevelSecurity, null, httpClientConnection.DbgConnectionId, -1, -1, -1, null, null, null, null, "HttpClientConnectionManager.Pool_Sender_OnEndSending is completed.");
			}
		}
		GenuineThreadPool.QueueUserWorkItem(_pool_Sender_OnEndSending_ContinueExchange, httpClientConnection, longDuration: false);
	}

	private void Pool_Sender_OnEndSending_ContinueExchange(object httpClientConnectionAsObject)
	{
		Message message = null;
		HttpClientConnection httpClientConnection = (HttpClientConnection)httpClientConnectionAsObject;
		lock (httpClientConnection.SendingLock)
		{
			message = httpClientConnection.MessageContainer.GetMessage();
			if (message == null)
			{
				httpClientConnection.IsSent = false;
				return;
			}
		}
		LowLevel_Sender_Send(message, httpClientConnection, httpClientConnection.OnEndSending, repeatSending: false);
	}

	public void ConnectionFailed(HttpClientConnection httpClientConnection, bool sender, Exception exception, bool tryToReestablish)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		try
		{
			if (tryToReestablish)
			{
				tryToReestablish = !ConnectionManager.IsExceptionCritical(exception as OperationException);
			}
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Connection, "HttpClientConnectionManager.ConnectionFailed", LogMessageType.ConnectionFailed, exception, null, httpClientConnection.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, httpClientConnection.DbgConnectionId, 0, 0, 0, null, null, null, null, "Connection failure. Sender: {0}. Connection type: {1}. Try to reestablish: {2}.", sender.ToString(), Enum.GetName(typeof(GenuineConnectionType), httpClientConnection.GenuineConnectionType), tryToReestablish.ToString());
			}
			ReaderAutoLocker readerAutoLocker = new ReaderAutoLocker(_disposeLock);
			try
			{
				if (base._disposed)
				{
					tryToReestablish = false;
				}
			}
			finally
			{
				readerAutoLocker.Dispose();
			}
			lock (httpClientConnection.Remote.PersistentConnectionEstablishingLock)
			{
				if (!tryToReestablish || httpClientConnection._disposed)
				{
					readerAutoLocker = new ReaderAutoLocker(_disposeLock);
					try
					{
						_persistent.Remove(httpClientConnection.Remote.Url);
					}
					finally
					{
						readerAutoLocker.Dispose();
					}
					if (httpClientConnection.SentContent != null)
					{
						httpClientConnection.SentContent.Close();
						httpClientConnection.SentContent = null;
					}
					ITransportContext.KnownHosts.ReleaseHostResources(httpClientConnection.Remote, exception);
					foreach (Message item in httpClientConnection.MessagesBeingSent)
					{
						ITransportContext.IIncomingStreamHandler.DispatchException(item, exception);
					}
					httpClientConnection.MessagesBeingSent.Clear();
					httpClientConnection.Dispose(exception);
					httpClientConnection.SignalState(GenuineEventType.GeneralConnectionClosed, exception, null);
					if (exception is OperationException && ((OperationException)exception).OperationErrorMessage.ErrorIdentifier == "GenuineChannels.Exception.Receive.ServerHasBeenRestarted")
					{
						httpClientConnection.SignalState(GenuineEventType.GeneralServerRestartDetected, exception, null);
					}
					if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
					{
						binaryLogWriter.WriteEvent(LogCategory.Connection, "HttpClientConnectionManager.ConnectionFailed", LogMessageType.ConnectionFailed, exception, null, httpClientConnection.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, httpClientConnection.DbgConnectionId, 0, 0, 0, null, null, null, null, "The connection has been completely terminated and removed from all collection. Sender: {0}. Connection type: {1}. Try to reestablish: {2}.", sender.ToString(), Enum.GetName(typeof(GenuineConnectionType), httpClientConnection.GenuineConnectionType), tryToReestablish.ToString());
					}
					return;
				}
				httpClientConnection.SignalState(GenuineEventType.GeneralConnectionReestablishing, exception, null);
				HttpClientConnection.ReconnectionContainer reconnectionContainer = (sender ? httpClientConnection.SenderReconnectionLock : httpClientConnection.ListenerReconnectionLock);
				lock (reconnectionContainer.SyncLock)
				{
					if (GenuineUtility.IsTimeoutExpired(reconnectionContainer.ReconnectionStartedAt + GenuineUtility.ConvertToMilliseconds(ITransportContext.IParameterProvider[GenuineParameter.MaxTimeSpanToReconnect])))
					{
						if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
						{
							binaryLogWriter.WriteEvent(LogCategory.Connection, "HttpClientConnectionManager.ConnectionFailed", LogMessageType.ConnectionReestablishing, GenuineExceptions.Get_Debugging_GeneralWarning("Connection was not reestablished within the specified time boundaries."), null, httpClientConnection.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, httpClientConnection.DbgConnectionId, 0, 0, 0, null, null, null, null, "The connection has not been reestablished within the specified time boundaries.");
						}
						readerAutoLocker = new ReaderAutoLocker(_disposeLock);
						try
						{
							_persistent.Remove(httpClientConnection.Remote.Url);
						}
						finally
						{
							readerAutoLocker.Dispose();
						}
						ITransportContext.KnownHosts.ReleaseHostResources(httpClientConnection.Remote, exception);
						httpClientConnection.Dispose(exception);
						httpClientConnection.MessageContainer.Dispose(exception);
						httpClientConnection.SignalState(GenuineEventType.GeneralConnectionClosed, exception, null);
					}
					else if (sender)
					{
						if (httpClientConnection.SentContent != null)
						{
							LowLevel_Sender_Send(null, httpClientConnection, httpClientConnection.OnEndSending, repeatSending: true);
						}
					}
					else
					{
						LowLevel_InitiateListening(httpClientConnection, requestNextPacket: false);
					}
				}
			}
		}
		catch (Exception exception2)
		{
			binaryLogWriter?.WriteImplementationWarningEvent("HttpClientConnectionManager.ConnectionFailed", LogMessageType.Error, exception2, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, "Unexpected exception occurred in the HttpClientConnectionManager.ConnectionFailed method. Most likely, something must be fixed.");
		}
	}

	public void TimerCallback()
	{
		GenuineThreadPool.QueueUserWorkItem(_internal_TimerCallback, null, longDuration: false);
	}

	private void Internal_TimerCallback(object ignored)
	{
		int tickCount = GenuineUtility.TickCount;
		int num = GenuineUtility.ConvertToMilliseconds(ITransportContext.IParameterProvider[GenuineParameter.PersistentConnectionSendPingAfterInactivity]);
		int num2 = GenuineUtility.ConvertToMilliseconds(ITransportContext.IParameterProvider[GenuineParameter.ClosePersistentConnectionAfterInactivity]);
		int num3 = GenuineUtility.ConvertToMilliseconds(ITransportContext.IParameterProvider[GenuineParameter.CloseInvocationConnectionAfterInactivity]);
		int num4 = GenuineUtility.ConvertToMilliseconds(ITransportContext.IParameterProvider[GenuineParameter.CloseOneWayConnectionAfterInactivity]);
		int num5 = GenuineUtility.ConvertToMilliseconds(ITransportContext.IParameterProvider[GenuineParameter.MaxTimeSpanToReconnect]);
		ArrayList arrayList = new ArrayList();
		DictionaryEntry[] array = null;
		lock (_persistent.SyncRoot)
		{
			array = new DictionaryEntry[_persistent.Count];
			_persistent.CopyTo(array, 0);
		}
		DictionaryEntry[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			DictionaryEntry dictionaryEntry = array2[i];
			HttpClientConnection httpClientConnection = (HttpClientConnection)dictionaryEntry.Value;
			if (httpClientConnection._disposed)
			{
				arrayList.Add(dictionaryEntry.Key);
				continue;
			}
			bool flag = false;
			int num6 = 0;
			lock (httpClientConnection.ListenerReconnectionLock.SyncLock)
			{
				flag |= httpClientConnection.ListenerReconnectionLock.IsReconnectionStarted;
				if (httpClientConnection.ListenerReconnectionLock.IsReconnectionStarted)
				{
					num6 = httpClientConnection.ListenerReconnectionLock.ReconnectionStartedAt;
				}
			}
			lock (httpClientConnection.SenderReconnectionLock.SyncLock)
			{
				flag |= httpClientConnection.SenderReconnectionLock.IsReconnectionStarted;
				if (httpClientConnection.SenderReconnectionLock.IsReconnectionStarted)
				{
					num6 = httpClientConnection.SenderReconnectionLock.ReconnectionStartedAt;
				}
			}
			if (GenuineUtility.IsTimeoutExpired(httpClientConnection.LastMessageWasReceviedAt + num2, tickCount) && !flag)
			{
				GenuineThreadPool.QueueUserWorkItem(PerformConnectionFailure, httpClientConnection, longDuration: false);
			}
			if (GenuineUtility.IsTimeoutExpired(httpClientConnection.LastMessageWasSentAt + num, tickCount))
			{
				GenuineThreadPool.QueueUserWorkItem(SendPing, httpClientConnection, longDuration: false);
			}
			if (flag && GenuineUtility.IsTimeoutExpired(num6 + num5, tickCount))
			{
				GenuineThreadPool.QueueUserWorkItem(PerformConnectionFailure, httpClientConnection, longDuration: false);
			}
		}
		foreach (object item in arrayList)
		{
			_persistent.Remove(item);
		}
	}

	private void PerformConnectionFailure(object httpClientConnectionAsObject)
	{
		ConnectionFailed(httpClientConnectionAsObject as HttpClientConnection, sender: true, GenuineExceptions.Get_Channel_ConnectionClosedAfterTimeout(), tryToReestablish: false);
	}

	private void SendPing(object httpClientConnectionAsObject)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		HttpClientConnection httpClientConnection = null;
		try
		{
			httpClientConnection = (HttpClientConnection)httpClientConnectionAsObject;
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Connection, "HttpClientConnectionManager.SendPing", LogMessageType.ConnectionPingSending, null, null, httpClientConnection.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, httpClientConnection.DbgConnectionId, 0, 0, 0, null, null, null, null, "Sending a ping.");
			}
			Message message = new Message(ITransportContext, httpClientConnection.Remote, -3, new TransportHeaders(), Stream.Null);
			message.IsSynchronous = false;
			Send(message);
		}
		catch (Exception exception)
		{
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Connection, "HttpClientConnectionManager.SendPing", LogMessageType.ConnectionPingSending, exception, null, httpClientConnection?.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, httpClientConnection?.DbgConnectionId ?? (-1), 0, 0, 0, null, null, null, null, "Exception occurred while sending a ping.");
			}
		}
	}

	private void ReestablishFailedConnection(object failedHttpRequestAsObject)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		ReaderAutoLocker readerAutoLocker = new ReaderAutoLocker(_disposeLock);
		try
		{
			if (base._disposed)
			{
				return;
			}
		}
		finally
		{
			readerAutoLocker.Dispose();
		}
		FailedHttpRequest failedHttpRequest = (FailedHttpRequest)failedHttpRequestAsObject;
		HttpClientConnection httpClientConnection = failedHttpRequest.HttpClientConnection;
		HttpClientConnection.ReconnectionContainer reconnectionContainer = (failedHttpRequest.Sender ? httpClientConnection.SenderReconnectionLock : httpClientConnection.ListenerReconnectionLock);
		bool flag = false;
		try
		{
			Exception exception = failedHttpRequest.Exception;
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Connection, "HttpClientConnectionManager.ReestablishFailedConnection", LogMessageType.ConnectionReestablishing, null, null, httpClientConnection.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, httpClientConnection.DbgConnectionId, 0, 0, 0, null, null, null, null, "Reestablishing the {0} HTTP connection. System.Net.ServicePointManager.DefaultConnectionLimit = {1}.", failedHttpRequest.Sender ? "SENDER" : "LISTENER", ServicePointManager.DefaultConnectionLimit);
			}
			int timeout = GenuineUtility.GetTimeout((TimeSpan)httpClientConnection.ITransportContext.IParameterProvider[GenuineParameter.MaxTimeSpanToReconnect]);
			for (int i = 0; i < (int)httpClientConnection.ITransportContext.IParameterProvider[GenuineParameter.ReconnectionTries]; i++)
			{
				if (GenuineUtility.IsTimeoutExpired(timeout))
				{
					break;
				}
				if (httpClientConnection._disposed)
				{
					break;
				}
				Thread.Sleep((TimeSpan)httpClientConnection.ITransportContext.IParameterProvider[GenuineParameter.SleepBetweenReconnections]);
				readerAutoLocker = new ReaderAutoLocker(_disposeLock);
				try
				{
					if (base._disposed)
					{
						return;
					}
				}
				finally
				{
					readerAutoLocker.Dispose();
				}
				lock (reconnectionContainer.SyncLock)
				{
					reconnectionContainer.RequestFailed = false;
				}
				if (httpClientConnection._disposed)
				{
					return;
				}
				try
				{
					binaryLogWriter?.WriteEvent(LogCategory.Debugging, "HttpClientConnectionManager.ReestablishFailedConnection", LogMessageType.DebuggingWarning, null, null, httpClientConnection.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, httpClientConnection.DbgConnectionId, 0, 0, 0, null, null, null, null, "Reestablishing the {0} attempt #{1}", failedHttpRequest.Sender ? "SENDER" : "LISTENER", i);
					ConnectionFailed(httpClientConnection, failedHttpRequest.Sender, exception, tryToReestablish: true);
					lock (reconnectionContainer.SyncLock)
					{
						binaryLogWriter?.WriteEvent(LogCategory.Debugging, "HttpClientConnectionManager.ReestablishFailedConnection", LogMessageType.DebuggingWarning, null, null, httpClientConnection.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, httpClientConnection.DbgConnectionId, 0, 0, 0, null, null, null, null, "Reestablishing the {0} attempt N{1} RESULT: {2}", failedHttpRequest.Sender ? "SENDER" : "LISTENER", i, reconnectionContainer.RequestFailed ? "FAILURE" : "SUCCESS");
						if (reconnectionContainer.RequestFailed)
						{
							continue;
						}
						flag = true;
						reconnectionContainer.IsReconnectionStarted = false;
						return;
					}
				}
				catch (Exception ex)
				{
					if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
					{
						binaryLogWriter.WriteEvent(LogCategory.Connection, "HttpClientConnectionManager.ReestablishFailedConnection", LogMessageType.ConnectionReestablishing, ex, null, httpClientConnection.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, httpClientConnection.DbgConnectionId, 0, 0, 0, null, null, null, null, "Reestablishing attempt failed. Reestablishing the {0} HTTP connection will be continued. Try: {1}. Milliseconds left: {2}", failedHttpRequest.Sender ? "SENDER" : "LISTENER", i, GenuineUtility.CompareTickCounts(timeout, GenuineUtility.TickCount));
					}
					httpClientConnection.SignalState(GenuineEventType.GeneralConnectionReestablishing, ex, null);
				}
			}
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Connection, "HttpClientConnectionManager.ReestablishFailedConnection", LogMessageType.ConnectionReestablished, GenuineExceptions.Get_Debugging_GeneralWarning("Reestablishing failed."), null, httpClientConnection.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, httpClientConnection.DbgConnectionId, 0, 0, 0, null, null, null, null, "Reestablishing the {0} HTTP connection has failed.", failedHttpRequest.Sender ? "SENDER" : "LISTENER");
			}
			ConnectionFailed(httpClientConnection, failedHttpRequest.Sender, exception, tryToReestablish: false);
		}
		finally
		{
			if (!flag)
			{
				reconnectionContainer.IsReconnectionStarted = false;
			}
		}
	}

	private void LowLevel_Sender_Send(Message message, HttpClientConnection httpClientConnection, AsyncCallback asyncCallback, bool repeatSending)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		try
		{
			if (!repeatSending)
			{
				httpClientConnection.SendSequenceNo++;
				httpClientConnection.LastMessageWasSentAt = GenuineUtility.TickCount;
				httpClientConnection.SentContent = new GenuineChunkedStream(releaseOnReadMode: false);
				MessageCoder.FillInLabelledStream(message, httpClientConnection.MessageContainer, httpClientConnection.MessagesBeingSent, httpClientConnection.SentContent, httpClientConnection.Sender_SendBuffer, (int)ITransportContext.IParameterProvider[GenuineParameter.HttpRecommendedPacketSize]);
				if (binaryLogWriter != null && binaryLogWriter[LogCategory.Transport] > 0)
				{
					for (int i = 0; i < httpClientConnection.MessagesBeingSent.Count; i++)
					{
						Message message2 = (Message)httpClientConnection.MessagesBeingSent[i];
						binaryLogWriter.WriteEvent(LogCategory.Transport, "HttpClientConnectionManager.LowLevel_Sender_Send", LogMessageType.MessageIsSentAsynchronously, null, message2, httpClientConnection.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, httpClientConnection.Sender_ConnectionLevelSecurity, null, httpClientConnection.DbgConnectionId, httpClientConnection.SendSequenceNo, 0, 0, null, null, null, null, "The message will be sent in the SENDER stream N: {0}.", httpClientConnection.SendSequenceNo);
					}
				}
				if (httpClientConnection.Sender_ConnectionLevelSecurity != null)
				{
					GenuineChunkedStream genuineChunkedStream = new GenuineChunkedStream(releaseOnReadMode: false);
					httpClientConnection.Sender_ConnectionLevelSecurity.Encrypt(httpClientConnection.SentContent, genuineChunkedStream);
					httpClientConnection.SentContent = genuineChunkedStream;
				}
				GenuineChunkedStream genuineChunkedStream2 = new GenuineChunkedStream(releaseOnReadMode: false);
				BinaryWriter binaryWriter = new BinaryWriter(genuineChunkedStream2);
				HttpMessageCoder.WriteRequestHeader(binaryWriter, 3, httpClientConnection.GenuineConnectionType, httpClientConnection.HostId, HttpPacketType.Usual, httpClientConnection.SendSequenceNo, httpClientConnection.ConnectionName, httpClientConnection.Remote.LocalHostUniqueIdentifier);
				if (httpClientConnection.SentContent.CanSeek)
				{
					genuineChunkedStream2.WriteStream(httpClientConnection.SentContent);
				}
				else
				{
					GenuineUtility.CopyStreamToStream(httpClientConnection.SentContent, genuineChunkedStream2);
				}
				httpClientConnection.SentContent = genuineChunkedStream2;
			}
			else
			{
				httpClientConnection.SentContent.Position = 0L;
			}
			httpClientConnection.Sender = httpClientConnection.InitializeRequest(sender: true, httpClientConnection._keepalive);
			httpClientConnection.Sender.ContentLength = httpClientConnection.SentContent.Length;
			LowLevel_InitiateHttpWebRequest(httpClientConnection.Sender, httpClientConnection.SentContent, httpClientConnection.SenderClosed, sender: true, asyncCallback, httpClientConnection);
		}
		catch (Exception exception)
		{
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Connection, "HttpClientConnectionManager.LowLevel_Sender_Send", LogMessageType.AsynchronousSendingFinished, exception, null, httpClientConnection.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, httpClientConnection.DbgConnectionId, 0, 0, 0, null, null, null, null, "Error occurred while sending a message to the remote host.");
			}
			StartReestablishingIfNecessary(httpClientConnection, exception, sender: true);
		}
	}

	private void LowLevel_InitiateHttpWebRequest(HttpWebRequest httpWebRequest, Stream inputStream, ManualResetEvent closed, bool sender, AsyncCallback asyncCallback, HttpClientConnection dbgHttpClientConnection)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		ReaderAutoLocker readerAutoLocker = new ReaderAutoLocker(_disposeLock);
		try
		{
			if (base._disposed)
			{
				throw OperationException.WrapException(_disposeReason);
			}
		}
		finally
		{
			readerAutoLocker.Dispose();
		}
		Stream stream = null;
		try
		{
			if (!closed.WaitOne(_webRequestInitiationTimeout, exitContext: false))
			{
				throw GenuineExceptions.Get_Processing_LogicError("[0] Deadlock or a heavy load.");
			}
			closed.Reset();
			stream = httpWebRequest.GetRequestStream();
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.LowLevelTransport] > 0)
			{
				binaryLogWriter.WriteTransportContentEvent(LogCategory.LowLevelTransport, "HttpClientConnectionManager.LowLevel_InitiateHttpWebRequest", LogMessageType.LowLevelTransport_AsyncSendingInitiating, null, null, dbgHttpClientConnection?.Remote, (binaryLogWriter[LogCategory.LowLevelTransport] > 1) ? inputStream : null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, dbgHttpClientConnection?.DbgConnectionId ?? (-1), (int)inputStream.Length, null, null, null, "HTTP {0} Request is being sent. Size: {1}.", sender ? "SENDER" : "LISTENER", (int)inputStream.Length);
			}
			GenuineUtility.CopyStreamToStream(inputStream, stream, (int)inputStream.Length);
			IncreaseBytesSent((int)inputStream.Length);
			HttpWebRequestCop httpWebRequestCop = new HttpWebRequestCop(httpWebRequest, asyncCallback, httpWebRequest, _httpAsynchronousRequestTimeout);
		}
		catch (Exception exception)
		{
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.LowLevelTransport] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.LowLevelTransport, "HttpClientConnectionManager.LowLevel_InitiateHttpWebRequest", LogMessageType.LowLevelTransport_AsyncSendingInitiating, exception, null, dbgHttpClientConnection?.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, dbgHttpClientConnection?.DbgConnectionId ?? (-1), 0, 0, 0, null, null, null, null, "Exception occurred while initiating HTTP request.");
			}
			try
			{
				httpWebRequest.Abort();
			}
			catch (Exception)
			{
			}
			throw;
		}
		finally
		{
			try
			{
				stream?.Close();
			}
			catch (Exception exception2)
			{
				if (binaryLogWriter != null && binaryLogWriter[LogCategory.LowLevelTransport] > 0)
				{
					binaryLogWriter.WriteEvent(LogCategory.LowLevelTransport, "HttpClientConnectionManager.LowLevel_InitiateHttpWebRequest", LogMessageType.LowLevelTransport_AsyncSendingInitiating, exception2, null, dbgHttpClientConnection?.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, dbgHttpClientConnection?.DbgConnectionId ?? (-1), 0, 0, 0, null, null, null, null, "Can't close the stream!");
				}
			}
			closed.Set();
		}
	}

	private HttpClientConnection LowLevel_OpenConnection(HostInformation remote, GenuineConnectionType genuineConnectionType, string connectionName)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		ReaderAutoLocker readerAutoLocker = new ReaderAutoLocker(_disposeLock);
		try
		{
			if (base._disposed)
			{
				throw OperationException.WrapException(_disposeReason);
			}
		}
		finally
		{
			readerAutoLocker.Dispose();
		}
		int timeout = GenuineUtility.GetTimeout((TimeSpan)ITransportContext.IParameterProvider[GenuineParameter.ConnectTimeout]);
		if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
		{
			binaryLogWriter.WriteEvent(LogCategory.Connection, "HttpClientConnectionManager.LowLevel_OpenConnection", LogMessageType.ConnectionEstablishing, null, null, remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "The connection is being established to \"{0}\".", (remote == null) ? "?" : ((remote.Url == null) ? "?" : remote.Url));
		}
		HttpClientConnection httpClientConnection = new HttpClientConnection(ITransportContext, connectionName);
		httpClientConnection.Remote = remote;
		httpClientConnection.GenuineConnectionType = genuineConnectionType;
		httpClientConnection.Sender_ConnectionLevelSecurity = CreateConnectionLevelSecuritySession(genuineConnectionType);
		httpClientConnection.Listener_ConnectionLevelSecurity = CreateConnectionLevelSecuritySession(genuineConnectionType);
		if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
		{
			binaryLogWriter.WriteConnectionParameterEvent(LogCategory.Connection, "HttpClientConnectionManager.LowLevel_OpenConnection", LogMessageType.ConnectionParameters, null, remote, ITransportContext.IParameterProvider, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, httpClientConnection.DbgConnectionId, "An HTTP connection is being established.");
		}
		Stream stream = Stream.Null;
		Stream stream2 = Stream.Null;
		Stream stream3 = null;
		GenuineChunkedStream genuineChunkedStream = null;
		bool flag = false;
		bool flag2 = false;
		int num = 0;
		while (true)
		{
			bool flag3 = true;
			if (!flag)
			{
				genuineChunkedStream = new GenuineChunkedStream(releaseOnReadMode: false);
				BinaryWriter binaryWriter = new BinaryWriter(genuineChunkedStream);
				HttpMessageCoder.WriteRequestHeader(binaryWriter, 3, genuineConnectionType, httpClientConnection.HostId, (num == 0) ? HttpPacketType.Establishing_ResetConnection : HttpPacketType.Establishing, ++httpClientConnection.SendSequenceNo, httpClientConnection.ConnectionName, remote.LocalHostUniqueIdentifier);
				GenuineChunkedStreamSizeLabel genuineChunkedStreamSizeLabel = new GenuineChunkedStreamSizeLabel(genuineChunkedStream);
				try
				{
					if (httpClientConnection.Sender_ConnectionLevelSecurity != null && !httpClientConnection.Sender_ConnectionLevelSecurity.IsEstablished)
					{
						stream3 = httpClientConnection.Sender_ConnectionLevelSecurity.EstablishSession(stream, connectionLevel: true);
						if (stream3 != null)
						{
							flag3 = false;
							GenuineUtility.CopyStreamToStream(stream3, genuineChunkedStream);
						}
					}
				}
				finally
				{
					genuineChunkedStreamSizeLabel.Dispose();
				}
				genuineChunkedStreamSizeLabel = new GenuineChunkedStreamSizeLabel(genuineChunkedStream);
				try
				{
					if (httpClientConnection.Listener_ConnectionLevelSecurity != null && !httpClientConnection.Listener_ConnectionLevelSecurity.IsEstablished)
					{
						stream3 = httpClientConnection.Listener_ConnectionLevelSecurity.EstablishSession(stream2, connectionLevel: true);
						if (stream3 != null)
						{
							flag3 = false;
							GenuineUtility.CopyStreamToStream(stream3, genuineChunkedStream);
						}
					}
				}
				finally
				{
					genuineChunkedStreamSizeLabel.Dispose();
				}
			}
			else
			{
				genuineChunkedStream.Position = 0L;
			}
			HttpWebRequest httpWebRequest = httpClientConnection.InitializeRequest(sender: true, httpClientConnection._keepalive);
			int num2 = GenuineUtility.CompareTickCounts(timeout, GenuineUtility.TickCount);
			if (num2 <= 0)
			{
				throw GenuineExceptions.Get_Channel_ConnectionClosedAfterTimeout();
			}
			httpWebRequest.Timeout = num2;
			httpWebRequest.ContentLength = genuineChunkedStream.Length;
			Stream stream4 = null;
			try
			{
				stream4 = httpWebRequest.GetRequestStream();
				GenuineUtility.CopyStreamToStream(genuineChunkedStream, stream4, (int)genuineChunkedStream.Length);
				using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
				{
					if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
					{
						StringBuilder stringBuilder = new StringBuilder(1024);
						foreach (string key in httpWebResponse.Headers.Keys)
						{
							stringBuilder.AppendFormat("{0}=\"{1}\"; ", key, httpWebResponse.Headers.Get(key));
						}
						binaryLogWriter.WriteEvent(LogCategory.Connection, "HttpClientConnectionManager.LowLevel_OpenConnection", LogMessageType.ConnectionEstablishing, null, null, remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, httpClientConnection.DbgConnectionId, 0, 0, 0, null, null, null, null, "The trial connection has been successfully completed. Content-Length: {0}; Content encoding: {1}; Content type: {2}; Protocol version: {3}; Respose uri: {4}; Status code: {5}; Status description: {6}; HTTP headers: {7}.", httpWebResponse.ContentLength, httpWebResponse.ContentEncoding, httpWebResponse.ContentType, httpWebResponse.ProtocolVersion, httpWebResponse.ResponseUri, httpWebResponse.StatusCode, httpWebResponse.StatusDescription, stringBuilder.ToString());
					}
					using Stream stream5 = httpWebResponse.GetResponseStream();
					BinaryReader binaryReader = new BinaryReader(stream5);
					HttpMessageCoder.ReadResponseHeader(binaryReader, out var serverUri, out var _, out var httpPacketType, out var remoteHostUniqueIdentifier);
					if (httpPacketType != HttpPacketType.Establishing && httpPacketType != HttpPacketType.Establishing_ResetConnection)
					{
						throw GenuineExceptions.Get_Connect_CanNotConnectToRemoteHost(remote.ToString(), "Wrong response received from the remote host.");
					}
					if (genuineConnectionType == GenuineConnectionType.Persistent || httpClientConnection.Sender_ConnectionLevelSecurity != null)
					{
						remote.UpdateUri(serverUri, remoteHostUniqueIdentifier);
					}
					int size = binaryReader.ReadInt32();
					stream = new GenuineChunkedStream(releaseOnReadMode: true);
					GenuineUtility.CopyStreamToStream(stream5, stream, size);
					size = binaryReader.ReadInt32();
					stream2 = new GenuineChunkedStream(releaseOnReadMode: true);
					GenuineUtility.CopyStreamToStream(stream5, stream2, size);
				}
				genuineChunkedStream.Close();
			}
			catch (Exception exception)
			{
				if (flag2)
				{
					throw;
				}
				if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
				{
					binaryLogWriter.WriteEvent(LogCategory.Connection, "HttpClientConnectionManager.LowLevel_OpenConnection", LogMessageType.ConnectionEstablishing, exception, null, remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "Exception occurred while establishing the connection to \"{0}\". The connection establishing will be continued.", (remote == null) ? "?" : ((remote.Url == null) ? "?" : remote.Url));
				}
				flag = true;
				goto IL_05f1;
			}
			finally
			{
				stream4?.Close();
			}
			flag2 = false;
			flag = false;
			if ((httpClientConnection.Sender_ConnectionLevelSecurity != null && !httpClientConnection.Sender_ConnectionLevelSecurity.IsEstablished) || (httpClientConnection.Listener_ConnectionLevelSecurity != null && !httpClientConnection.Listener_ConnectionLevelSecurity.IsEstablished) || !flag3)
			{
				goto IL_05f1;
			}
			break;
			IL_05f1:
			num++;
		}
		remote.PhysicalAddress = remote.Url;
		if (binaryLogWriter != null && binaryLogWriter[LogCategory.HostInformation] > 0)
		{
			binaryLogWriter.WriteHostInformationEvent("HttpClientConnectionManager.LowLevel_OpenConnection", LogMessageType.HostInformationCreated, null, remote, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, httpClientConnection.Sender_ConnectionLevelSecurity, (httpClientConnection.Sender_ConnectionLevelSecurity == null) ? null : httpClientConnection.Sender_ConnectionLevelSecurity.Name, httpClientConnection.DbgConnectionId, "HostInformation is ready for actions.");
		}
		if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
		{
			binaryLogWriter.WriteEvent(LogCategory.Connection, "HttpClientConnectionManager.LowLevel_OpenConnection", LogMessageType.ConnectionEstablished, null, null, remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, httpClientConnection.Sender_ConnectionLevelSecurity, (httpClientConnection.Sender_ConnectionLevelSecurity == null) ? null : httpClientConnection.Sender_ConnectionLevelSecurity.Name, httpClientConnection.DbgConnectionId, (int)genuineConnectionType, 0, 0, GetType().Name, remote.Url, remote.Url, null, "The connection to the remote host is established.");
		}
		if (genuineConnectionType == GenuineConnectionType.Persistent)
		{
			LowLevel_InitiateListening(httpClientConnection, requestNextPacket: true);
		}
		return httpClientConnection;
	}

	public void LowLevel_ParseLabelledStream(Stream stream, byte[] intermediateBuffer, HttpClientConnection httpClientConnection, SecuritySession connectionLevelSecuritySession)
	{
		bool flag = httpClientConnection.GenuineConnectionType == GenuineConnectionType.Invocation;
		BinaryReader binaryReader = new BinaryReader(stream);
		while (binaryReader.ReadByte() == 0)
		{
			using LabelledStream labelledStream = new LabelledStream(ITransportContext, stream, intermediateBuffer);
			if (flag)
			{
				ITransportContext.IIncomingStreamHandler.HandleMessage(labelledStream, httpClientConnection.Remote, httpClientConnection.GenuineConnectionType, httpClientConnection.ConnectionName, httpClientConnection.DbgConnectionId, useThisThread: true, null, connectionLevelSecuritySession, null);
				continue;
			}
			GenuineChunkedStream genuineChunkedStream = new GenuineChunkedStream(releaseOnReadMode: true);
			GenuineUtility.CopyStreamToStream(labelledStream, genuineChunkedStream);
			ITransportContext.IIncomingStreamHandler.HandleMessage(genuineChunkedStream, httpClientConnection.Remote, httpClientConnection.GenuineConnectionType, httpClientConnection.ConnectionName, httpClientConnection.DbgConnectionId, useThisThread: false, null, connectionLevelSecuritySession, null);
		}
	}

	private void LowLevel_InitiateListening(HttpClientConnection httpClientConnection, bool requestNextPacket)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		try
		{
			if (requestNextPacket)
			{
				httpClientConnection.ListenerSequenceNo++;
			}
			using GenuineChunkedStream genuineChunkedStream = new GenuineChunkedStream(releaseOnReadMode: false);
			BinaryWriter binaryWriter = new BinaryWriter(genuineChunkedStream);
			HttpMessageCoder.WriteRequestHeader(binaryWriter, 3, httpClientConnection.GenuineConnectionType, httpClientConnection.HostId, HttpPacketType.Listening, httpClientConnection.ListenerSequenceNo, httpClientConnection.ConnectionName, httpClientConnection.Remote.LocalHostUniqueIdentifier);
			httpClientConnection.Listener = httpClientConnection.InitializeRequest(sender: false, httpClientConnection._keepalive);
			httpClientConnection.Listener.ContentLength = genuineChunkedStream.Length;
			LowLevel_InitiateHttpWebRequest(httpClientConnection.Listener, genuineChunkedStream, httpClientConnection.ListenerClosed, sender: false, httpClientConnection.OnEndReceiving, httpClientConnection);
		}
		catch (Exception exception)
		{
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Connection, "HttpClientConnectionManager.LowLevel_InitiateListening", LogMessageType.ConnectionFailed, exception, null, httpClientConnection.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, httpClientConnection.DbgConnectionId, 0, 0, 0, null, null, null, null, "The LISTENER request Seq No={0} has failed. The connection will be reestablished, if possible.", httpClientConnection.ListenerSequenceNo);
			}
			StartReestablishingIfNecessary(httpClientConnection, exception, sender: false);
		}
	}

	public void Listener_OnEndReceiving(HttpClientConnection httpClientConnection, HttpWebResponse httpWebResponse)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		Stream stream = null;
		try
		{
			httpClientConnection.Remote.Renew(_hostRenewingSpan, canMakeShorter: false);
			httpClientConnection.LastMessageWasReceviedAt = GenuineUtility.TickCount;
			httpClientConnection.SignalState(GenuineEventType.GeneralConnectionEstablished, null, null);
			stream = httpWebResponse.GetResponseStream();
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.LowLevelTransport] > 0)
			{
				bool flag = binaryLogWriter[LogCategory.LowLevelTransport] > 1;
				GenuineChunkedStream genuineChunkedStream = null;
				if (flag)
				{
					genuineChunkedStream = new GenuineChunkedStream(releaseOnReadMode: false);
					GenuineUtility.CopyStreamToStream(stream, genuineChunkedStream, (int)httpWebResponse.ContentLength);
				}
				binaryLogWriter.WriteTransportContentEvent(LogCategory.LowLevelTransport, "HttpClientConnectionManager.Listener_OnEndReceiving", LogMessageType.LowLevelTransport_AsyncReceivingCompleted, null, null, httpClientConnection.Remote, flag ? genuineChunkedStream : null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, httpClientConnection.DbgConnectionId, (int)httpWebResponse.ContentLength, null, null, null, "The content has been received.");
				if (flag)
				{
					stream = genuineChunkedStream;
				}
			}
			if (httpClientConnection.Listener_ConnectionLevelSecurity != null)
			{
				stream = httpClientConnection.Listener_ConnectionLevelSecurity.Decrypt(stream);
			}
			BinaryReader binaryReader = new BinaryReader(stream);
			HttpMessageCoder.ReadResponseHeader(binaryReader, out var serverUri, out var sequenceNo, out var httpPacketType, out var remoteHostUniqueIdentifier);
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Connection, "HttpClientConnectionManager.Listener_OnEndReceiving", LogMessageType.ReceivingFinished, null, null, httpClientConnection.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, httpClientConnection.Listener_ConnectionLevelSecurity, null, httpClientConnection.DbgConnectionId, 0, 0, 0, null, null, null, null, "A response to the listener request has been received. Server uri: {0}. Sequence no: {1}. Packet type: {2}. Content-encoding: {3}. Content-length: {4}. Protocol version: {5}. Response uri: \"{6}\". Server: \"{7}\". Status code: {8}. Status description: \"{9}\".", serverUri, sequenceNo, Enum.Format(typeof(HttpPacketType), httpPacketType, "g"), httpWebResponse.ContentEncoding, httpWebResponse.ContentLength, httpWebResponse.ProtocolVersion, httpWebResponse.ResponseUri, httpWebResponse.Server, httpWebResponse.StatusCode, httpWebResponse.StatusDescription);
			}
			if (httpPacketType == HttpPacketType.Desynchronization)
			{
				throw GenuineExceptions.Get_Channel_Desynchronization();
			}
			if (sequenceNo != httpClientConnection.ListenerSequenceNo)
			{
				throw GenuineExceptions.Get_Processing_LogicError($"Impossible situation. The request stream number: {httpClientConnection.ListenerSequenceNo}. The received stream number: {sequenceNo}.");
			}
			if (httpClientConnection.GenuineConnectionType == GenuineConnectionType.Persistent)
			{
				httpClientConnection.Remote.UpdateUri(serverUri, remoteHostUniqueIdentifier);
			}
			switch (httpPacketType)
			{
			case HttpPacketType.Desynchronization:
			case HttpPacketType.ClosedManually:
				throw GenuineExceptions.Get_Receive_ConnectionClosed();
			default:
				LowLevel_ParseLabelledStream(stream, httpClientConnection.Listener_ReceiveBuffer, httpClientConnection, httpClientConnection.Listener_ConnectionLevelSecurity);
				break;
			case HttpPacketType.ListenerTimedOut:
				break;
			}
			GenuineUtility.CopyStreamToStream(stream, Stream.Null);
		}
		catch (Exception exception)
		{
			ConnectionFailed(httpClientConnection, sender: false, exception, tryToReestablish: true);
			return;
		}
		finally
		{
			stream?.Close();
			httpWebResponse.Close();
		}
		GenuineThreadPool.QueueUserWorkItem(Listener_OnEndReceiving_ContinueExchange, httpClientConnection, longDuration: false);
	}

	private void Listener_OnEndReceiving_ContinueExchange(object httpClientConnectionAsObject)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		HttpClientConnection httpClientConnection = (HttpClientConnection)httpClientConnectionAsObject;
		LowLevel_InitiateListening(httpClientConnection, requestNextPacket: true);
	}

	public override void StartListening(object endPoint)
	{
		throw new NotSupportedException();
	}

	public override void StopListening(object endPoint)
	{
		throw new NotSupportedException();
	}

	public override void ReleaseConnections(HostInformation hostInformation, GenuineConnectionType genuineConnectionType, Exception reason)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		HttpClientConnection httpClientConnection = null;
		ArrayList arrayList = new ArrayList();
		if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
		{
			binaryLogWriter.WriteEvent(LogCategory.Connection, "HttpClientConnectionManager.ReleaseConnections", LogMessageType.ReleaseConnections, reason, null, hostInformation, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, Enum.Format(typeof(GenuineConnectionType), genuineConnectionType, "g"), null, null, null, "\"{0}\" connections will be terminated.", Enum.Format(typeof(GenuineConnectionType), genuineConnectionType, "g"), null);
		}
		if ((genuineConnectionType & GenuineConnectionType.Persistent) != GenuineConnectionType.None)
		{
			lock (_persistent.SyncRoot)
			{
				foreach (object item in _persistent)
				{
					DictionaryEntry dictionaryEntry = (DictionaryEntry)item;
					httpClientConnection = (HttpClientConnection)dictionaryEntry.Value;
					if (hostInformation == null || httpClientConnection.Remote == hostInformation)
					{
						arrayList.Add(httpClientConnection);
					}
				}
			}
		}
		foreach (HttpClientConnection item2 in arrayList)
		{
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Connection, "HttpClientConnectionManager.ReleaseConnections", LogMessageType.ConnectionShuttingDown, reason, null, item2.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, item2.DbgConnectionId, 0, 0, 0, null, null, null, null, "The connection is being shut down manually.");
			}
			ConnectionFailed(item2, sender: true, GenuineExceptions.Get_Channel_ConnectionShutDown(reason), tryToReestablish: false);
			if (item2.GenuineConnectionType == GenuineConnectionType.Persistent)
			{
				item2.SignalState(GenuineEventType.GeneralConnectionClosed, reason, null);
			}
		}
	}

	public override string[] GetConnectionNames(string uri)
	{
		lock (_persistent.SyncRoot)
		{
			if (_persistent.ContainsKey(uri))
			{
				return new string[1] { uri };
			}
		}
		return null;
	}

	public override void InternalDispose(Exception reason)
	{
		ReleaseConnections(null, GenuineConnectionType.All, reason);
	}

	public void StartReestablishingIfNecessary(HttpClientConnection httpClientConnection, Exception exception, bool sender)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		HttpClientConnection.ReconnectionContainer reconnectionContainer = (sender ? httpClientConnection.SenderReconnectionLock : httpClientConnection.ListenerReconnectionLock);
		lock (reconnectionContainer.SyncLock)
		{
			if (!reconnectionContainer.IsReconnectionStarted)
			{
				binaryLogWriter?.WriteEvent(LogCategory.Debugging, "HttpClientConnectionManager.StartReestablishingIfNecessary", LogMessageType.ConnectionReestablishing, exception, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "Reconnection is being started.");
				reconnectionContainer.IsReconnectionStarted = true;
				reconnectionContainer.ReconnectionStartedAt = GenuineUtility.TickCount;
				FailedHttpRequest failedHttpRequest = new FailedHttpRequest();
				failedHttpRequest.HttpClientConnection = httpClientConnection;
				failedHttpRequest.Exception = exception;
				failedHttpRequest.Sender = sender;
				GenuineThreadPool.QueueUserWorkItem(ReestablishFailedConnection, failedHttpRequest, longDuration: true);
			}
			else
			{
				binaryLogWriter?.WriteEvent(LogCategory.Debugging, "HttpClientConnectionManager.StartReestablishingIfNecessary", LogMessageType.ConnectionReestablishing, exception, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "Reconnection is already in progress.");
				reconnectionContainer.RequestFailed = true;
			}
		}
	}

	public void ReestablishingCompleted(HttpClientConnection httpClientConnection, bool sender)
	{
		HttpClientConnection.ReconnectionContainer reconnectionContainer = (sender ? httpClientConnection.SenderReconnectionLock : httpClientConnection.ListenerReconnectionLock);
		reconnectionContainer.IsReconnectionStarted = false;
	}
}
