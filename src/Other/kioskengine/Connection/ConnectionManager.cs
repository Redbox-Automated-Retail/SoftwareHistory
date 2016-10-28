using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Belikov.Common.ThreadProcessing;
using Belikov.GenuineChannels.DotNetRemotingLayer;
using Belikov.GenuineChannels.Logbook;
using Belikov.GenuineChannels.Messaging;
using Belikov.GenuineChannels.Parameters;
using Belikov.GenuineChannels.Security;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.Connection;

[ComVisible(true)]
public abstract class ConnectionManager : MarshalByRefObject
{
	public ITransportContext ITransportContext;

	public HostInformation Local;

	private bool __disposed = false;

	private object __disposedLock = new object();

	internal Exception _disposeReason = null;

	internal ReaderWriterLock _disposeLock = new ReaderWriterLock();

	private long _totalBytesSent = 0L;

	private object _totalBytesSentLock = new object();

	private long _totalBytesReceived = 0L;

	private object _totalBytesReceivedLock = new object();

	private static int _ConnectionCounter = 0;

	private static int _ConnectionIdentifierCounter = 0;

	internal bool _disposed
	{
		get
		{
			lock (__disposedLock)
			{
				return __disposed;
			}
		}
		set
		{
			lock (__disposedLock)
			{
				__disposed = value;
			}
		}
	}

	public long BytesSent
	{
		get
		{
			lock (_totalBytesSentLock)
			{
				return _totalBytesSent;
			}
		}
	}

	public long BytesReceived
	{
		get
		{
			lock (_totalBytesReceivedLock)
			{
				return _totalBytesReceived;
			}
		}
	}

	public ConnectionManager(ITransportContext iTransportContext)
	{
		ITransportContext = iTransportContext;
	}

	public void Send(Message message)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		try
		{
			ReaderAutoLocker readerAutoLocker = new ReaderAutoLocker(_disposeLock);
			try
			{
				if (_disposed)
				{
					throw OperationException.WrapException(_disposeReason);
				}
			}
			finally
			{
				readerAutoLocker.Dispose();
			}
			SecuritySession securitySession = null;
			if (_disposed)
			{
				throw OperationException.WrapException(_disposeReason);
			}
			if (message.SecuritySessionParameters == null)
			{
				SecuritySessionParameters securitySessionParameters = SecuritySessionServices.GetCurrentSecurityContext();
				if (securitySessionParameters == null)
				{
					securitySessionParameters = message.Recipient.SecuritySessionParameters;
				}
				if (securitySessionParameters == null)
				{
					securitySessionParameters = ITransportContext.SecuritySessionParameters;
				}
				if (securitySessionParameters == null)
				{
					securitySessionParameters = SecuritySessionServices.DefaultSecuritySession;
				}
				message.SecuritySessionParameters = securitySessionParameters;
			}
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.MessageProcessing] > 0)
			{
				binaryLogWriter.WriteSecuritySessionParametersEvent("ConnectionManager.Send", LogMessageType.SecuritySessionParametersAssembled, null, message, message.Recipient, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, message.SecuritySessionParameters, "Security Session Parameters have been assembled.");
			}
			message.IsSynchronous = (message.SecuritySessionParameters.Attributes & SecuritySessionAttributes.ForceSync) != SecuritySessionAttributes.None || (message.IsSynchronous && (message.SecuritySessionParameters.Attributes & SecuritySessionAttributes.ForceAsync) == 0);
			if (!message.FinishTime_Initialized)
			{
				TimeSpan timeSpan = message.SecuritySessionParameters.Timeout;
				if (timeSpan == TimeSpan.MinValue)
				{
					timeSpan = (TimeSpan)ITransportContext.IParameterProvider[GenuineParameter.InvocationTimeout];
				}
				message.FinishTime = GenuineUtility.GetTimeout(timeSpan);
				message.FinishTime_Initialized = true;
			}
			if (message.SerializedContent == null)
			{
				securitySession = message.Recipient.GetSecuritySession(message.SecuritySessionParameters.Name, ITransportContext.IKeyStore);
				if (!securitySession.IsEstablished)
				{
					if (binaryLogWriter != null && binaryLogWriter[LogCategory.Security] > 0)
					{
						binaryLogWriter.WriteEvent(LogCategory.Security, "ConnectionManager.Send", LogMessageType.SecuritySessionHasNotBeenEstablishedYet, null, message, message.Recipient, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, securitySession, securitySession.Name, -1, 0, 0, 0, null, null, null, null, "The requested Security Session is not established.");
					}
					securitySession.InitiateEstablishingSecuritySession(message.SecuritySessionParameters);
					if (message.IsSynchronous)
					{
						int millisecondsLeft = GenuineUtility.GetMillisecondsLeft(message.FinishTime);
						if (millisecondsLeft <= 0)
						{
							throw GenuineExceptions.Get_Send_ServerDidNotReply();
						}
						int num = 0;
						num = ((message.CancelSending == null) ? WaitHandle.WaitAny(new WaitHandle[2] { securitySession.IsEstablishedEvent, securitySession.Failed }, millisecondsLeft, exitContext: false) : WaitHandle.WaitAny(new WaitHandle[3] { securitySession.IsEstablishedEvent, securitySession.Failed, message.CancelSending }, millisecondsLeft, exitContext: false));
						if (num == 258)
						{
							throw GenuineExceptions.Get_Send_ServerDidNotReply();
						}
						Exception reasonOfFailure = securitySession.ReasonOfFailure;
						switch (num)
						{
						case 1:
							if (reasonOfFailure != null)
							{
								throw OperationException.WrapException(reasonOfFailure);
							}
							throw GenuineExceptions.Get_Security_ContextWasNotEstablished(securitySession.Name);
						case 2:
							return;
						}
					}
					else if (!securitySession.IsEstablished)
					{
						securitySession.PutMessageToAwaitingQueue(message);
						return;
					}
				}
			}
			if (message.SerializedContent == null)
			{
				GenuineChunkedStream genuineChunkedStream = new GenuineChunkedStream(releaseOnReadMode: false);
				MessageCoder.Serialize(genuineChunkedStream, message, (message.SecuritySessionParameters.Attributes & SecuritySessionAttributes.EnableCompression) != 0);
				GenuineChunkedStream genuineChunkedStream2 = new GenuineChunkedStream(releaseOnReadMode: false);
				BinaryWriter binaryWriter = new BinaryWriter(genuineChunkedStream2);
				binaryWriter.Write(message.SecuritySessionParameters.Name);
				securitySession.Encrypt(genuineChunkedStream, genuineChunkedStream2);
				message.SerializedContent = genuineChunkedStream2;
				if (binaryLogWriter != null && binaryLogWriter[LogCategory.Security] > 0)
				{
					binaryLogWriter.WriteEvent(LogCategory.Security, "ConnectionManager.Send", LogMessageType.SecuritySessionApplied, null, message, message.Recipient, (binaryLogWriter[LogCategory.Security] > 1) ? message.SerializedContent : null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, securitySession, securitySession.Name, -1, 0, 0, 0, null, null, null, null, "The message has been processed by the established Security Session.");
				}
			}
			message.Sender = Local;
			InternalSend(message);
		}
		catch (Exception exception)
		{
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Security] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.MessageProcessing, "ConnectionManager.Send", LogMessageType.Error, exception, message, message.Recipient, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "An exception occurred while processing the message.");
			}
			throw;
		}
	}

	protected abstract void InternalSend(Message message);

	public abstract void StartListening(object endPoint);

	public abstract void StopListening(object endPoint);

	public abstract void ReleaseConnections(HostInformation hostInformation, GenuineConnectionType genuineConnectionType, Exception reason);

	public virtual string[] GetConnectionNames(string uri)
	{
		return null;
	}

	public void Dispose(Exception reason)
	{
		if (_disposed)
		{
			return;
		}
		if (reason == null)
		{
			reason = GenuineExceptions.Get_Processing_TransportConnectionFailed();
		}
		WriterAutoLocker writerAutoLocker = new WriterAutoLocker(_disposeLock);
		try
		{
			if (_disposed)
			{
				return;
			}
			_disposed = true;
			_disposeReason = reason;
		}
		finally
		{
			writerAutoLocker.Dispose();
		}
		InternalDispose(reason);
	}

	public abstract void InternalDispose(Exception reason);

	internal void IncreaseBytesSent(int bytesSent)
	{
		lock (_totalBytesSentLock)
		{
			_totalBytesSent += bytesSent;
		}
	}

	internal void IncreaseBytesReceived(int bytesReceived)
	{
		lock (_totalBytesReceivedLock)
		{
			_totalBytesReceived += bytesReceived;
		}
	}

	internal static bool IsExceptionCritical(OperationException operationException)
	{
		if (operationException == null)
		{
			return false;
		}
		return operationException is GenuineExceptions.QueueIsOverloaded || operationException is GenuineExceptions.ChannelClosed || operationException is GenuineExceptions.ServerHasBeenRestarted || operationException is GenuineExceptions.ClientDidNotReconnectWithinTimeOut || operationException is GenuineExceptions.ConnectionClosed || operationException is GenuineExceptions.ChannelDesynchronization || operationException is GenuineExceptions.ConnectionShutDown || operationException is GenuineExceptions.ConnectionClosedAfterTimeout;
	}

	internal SecuritySession CreateConnectionLevelSecuritySession(GenuineConnectionType genuineConnectionType)
	{
		string text = null;
		switch (genuineConnectionType)
		{
		case GenuineConnectionType.Persistent:
			text = ITransportContext.IParameterProvider[GenuineParameter.SecuritySessionForPersistentConnections] as string;
			break;
		case GenuineConnectionType.Named:
			text = ITransportContext.IParameterProvider[GenuineParameter.SecuritySessionForNamedConnections] as string;
			break;
		case GenuineConnectionType.Invocation:
			text = ITransportContext.IParameterProvider[GenuineParameter.SecuritySessionForInvocationConnections] as string;
			break;
		}
		if (text != null)
		{
			return ITransportContext.IKeyStore.GetKey(text).CreateSecuritySession(text, null);
		}
		return null;
	}

	internal void ReleaseHostResources(HostInformation hostInformation, Exception exception)
	{
		ITransportContext.IGenuineEventProvider.Fire(new GenuineEventArgs(GenuineEventType.GeneralServerRestartDetected, exception, hostInformation, null));
	}

	internal string GetUniqueConnectionName()
	{
		return Interlocked.Increment(ref _ConnectionCounter).ToString();
	}

	internal static int GetUniqueConnectionId()
	{
		return Interlocked.Increment(ref _ConnectionIdentifierCounter);
	}
}
