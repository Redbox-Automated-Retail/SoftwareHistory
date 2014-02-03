using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Channels;
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

namespace Belikov.GenuineChannels.GenuineTcp;

internal class TcpConnectionManager : ConnectionManager, ITimerConsumer, IAcceptConnectionConsumer
{
	private class FindExpiredConnections_CollectedConnections
	{
		public ArrayList SendPingTo = new ArrayList();

		public ArrayList ExpiredConnections = new ArrayList();

		public int SendPingAfterInactivity;

		public int Now;
	}

	private class ReleaseConnections_Parameters
	{
		public ArrayList FailedConnections;

		public HostInformation HostInformation;

		public GenuineConnectionType GenuineConnectionType;
	}

	internal const int HEADER_SIZE = 6;

	internal const int SOCKET_BUFFER_SIZE = 5000;

	private AsyncCallback _onEndSending;

	private WaitCallback _lowLevel_Client_PreventDelayedAck;

	private AsyncCallback _HalfSync_onEndReceiving;

	private WaitCallback _HalfSync_onContinueReceiving;

	private PersistentConnectionStorage _persistent = new PersistentConnectionStorage();

	private Hashtable _named = new Hashtable();

	private Hashtable _invocation = Hashtable.Synchronized(new Hashtable());

	private Hashtable _knownInvocationConnections = Hashtable.Synchronized(new Hashtable());

	private object _lowLevel_OpenConnectionLock = new object();

	private WaitCallback _internal_TimerCallback;

	private bool _isInProgress = false;

	private PersistentConnectionStorage.ProcessConnectionEventHandler _processObjectLateBoundHandler;

	private bool _tcpReadRequestBeforeProcessing;

	public int LocalPort = -1;

	private Hashtable _listeningSockets = Hashtable.Synchronized(new Hashtable());

	private PersistentConnectionStorage.ProcessConnectionEventHandler _releaseConnections_InspectPersistentConnections;

	public TcpConnectionManager(ITransportContext iTransportContext)
		: base(iTransportContext)
	{
		_onEndSending = LowLevel_EndSending;
		_internal_TimerCallback = Internal_TimerCallback;
		_HalfSync_onContinueReceiving = Pool_ContinueHalfSyncReceiving;
		_HalfSync_onEndReceiving = LowLevel_HalfSync_EndReceiving;
		_lowLevel_Client_PreventDelayedAck = LowLevel_Client_PreventDelayedAck;
		_processObjectLateBoundHandler = FindExpiredConnections;
		_releaseConnections_InspectPersistentConnections = ReleaseConnections_InspectPersistentConnections;
		_tcpReadRequestBeforeProcessing = (bool)iTransportContext.IParameterProvider[GenuineParameter.TcpReadRequestBeforeProcessing];
		Local = new HostInformation("_gtcp://" + iTransportContext.HostIdentifier, iTransportContext);
		TimerProvider.Attach(this);
	}

	protected override void InternalSend(Message message)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		TcpSocketInfo tcpSocketInfo = null;
		if (message.IsSynchronous)
		{
			while (true)
			{
				tcpSocketInfo = GetConnectionForSending(message);
				if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
				{
					binaryLogWriter.WriteEvent(LogCategory.Connection, "TcpConnectionManager.InternalSend", LogMessageType.ConnectionSelected, null, null, tcpSocketInfo.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, tcpSocketInfo.ConnectionLevelSecurity, (tcpSocketInfo.ConnectionLevelSecurity == null) ? null : tcpSocketInfo.ConnectionLevelSecurity.Name, tcpSocketInfo.DbgConnectionId, 0, 0, 0, null, null, null, null, "TCP connection has been selected. Stack trace: {0}", Environment.StackTrace);
				}
				try
				{
					if (tcpSocketInfo.MessageContainer != null)
					{
						tcpSocketInfo.MessageContainer.AddMessage(message, onlyCheckIn: true);
					}
					SendSync(message, tcpSocketInfo);
					break;
				}
				catch (Exception ex)
				{
					SocketFailed(ex, tcpSocketInfo);
					if (ex is OperationException ex2 && ex2.OperationErrorMessage.ErrorIdentifier == "GenuineChannels.Exception.Send.TransportProblem")
					{
						message.SerializedContent.Position = 0L;
						continue;
					}
					throw;
				}
				finally
				{
					if (tcpSocketInfo.MessageContainer != null)
					{
						tcpSocketInfo.MessageContainer.UnregisterSyncMessage(message);
					}
				}
			}
		}
		else
		{
			tcpSocketInfo = GetConnectionForSending(message);
			SendAsync(message, tcpSocketInfo);
		}
	}

	private void SendSync(Message message, TcpSocketInfo tcpSocketInfo)
	{
		SyncMessageSlot syncMessageSlot = null;
		bool flag = true;
		lock (tcpSocketInfo)
		{
			if (tcpSocketInfo.LockForSending)
			{
				syncMessageSlot = new SyncMessageSlot();
				tcpSocketInfo.MessageContainer.AddMessage(syncMessageSlot, onlyCheckIn: false);
				flag = false;
			}
			else
			{
				tcpSocketInfo.LockForSending = true;
			}
		}
		if (!flag)
		{
			WaitHandle[] array = null;
			array = ((message.CancelSending == null) ? new WaitHandle[1] : new WaitHandle[2] { null, message.CancelSending });
			array[0] = syncMessageSlot.ConnectionAvailable;
			int millisecondsLeft = GenuineUtility.GetMillisecondsLeft(message.FinishTime);
			if (millisecondsLeft <= 0)
			{
				throw GenuineExceptions.Get_Send_ServerDidNotReply();
			}
			switch (WaitHandle.WaitAny(array, millisecondsLeft, exitContext: false))
			{
			case 258:
				lock (syncMessageSlot)
				{
					syncMessageSlot.IsValid = false;
					if (syncMessageSlot.TcpSocketInfo != null)
					{
						Pool_StartSending(syncMessageSlot.TcpSocketInfo);
					}
				}
				throw GenuineExceptions.Get_Send_ServerDidNotReply();
			case 1:
				throw GenuineExceptions.Get_Send_Timeout();
			}
			if (syncMessageSlot.SyncWaitException != null)
			{
				throw OperationException.WrapException(syncMessageSlot.SyncWaitException);
			}
			tcpSocketInfo = syncMessageSlot.TcpSocketInfo;
		}
		LowLevel_SendSync(message, tcpSocketInfo);
		switch (tcpSocketInfo.GenuineConnectionType)
		{
		case GenuineConnectionType.Persistent:
		case GenuineConnectionType.Named:
			message.IsResendAfterFail = false;
			Pool_StartSending(tcpSocketInfo);
			break;
		case GenuineConnectionType.Invocation:
			lock (tcpSocketInfo)
			{
				tcpSocketInfo.TcpInvocationFiniteAutomatonState = TcpInvocationFiniteAutomatonState.ClientReceiving;
				tcpSocketInfo.Renew();
			}
			lock (tcpSocketInfo)
			{
				tcpSocketInfo.TcpInvocationFiniteAutomatonState = TcpInvocationFiniteAutomatonState.ClientAvailable;
				tcpSocketInfo.Renew();
				break;
			}
		case GenuineConnectionType.Persistent | GenuineConnectionType.Named:
			break;
		}
	}

	private void SendAsync(Message message, TcpSocketInfo tcpSocketInfo)
	{
		try
		{
			bool flag = true;
			lock (tcpSocketInfo)
			{
				if (tcpSocketInfo.LockForSending)
				{
					flag = false;
					tcpSocketInfo.MessageContainer.AddMessage(message, onlyCheckIn: false);
				}
				else
				{
					tcpSocketInfo.LockForSending = true;
				}
			}
			if (flag)
			{
				tcpSocketInfo.Message = message;
				tcpSocketInfo.AsyncSendStream = null;
				BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
				if (binaryLogWriter != null && binaryLogWriter[LogCategory.Transport] > 0)
				{
					binaryLogWriter.WriteEvent(LogCategory.Transport, "TcpConnectionManager.SendAsync", LogMessageType.MessageIsSentAsynchronously, null, message, message.Recipient, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, (tcpSocketInfo.ConnectionLevelSecurity == null) ? null : tcpSocketInfo.ConnectionLevelSecurity, (tcpSocketInfo.ConnectionLevelSecurity == null) ? null : tcpSocketInfo.ConnectionLevelSecurity.Name, tcpSocketInfo.DbgConnectionId, 0, 0, 0, null, null, null, null, "The connection has been obtained and the message is being sent asynchronously.");
				}
				LowLevel_StartSending(message, tcpSocketInfo);
			}
		}
		catch (Exception exception)
		{
			SocketFailed(exception, tcpSocketInfo);
		}
	}

	private void SendPing(object tcpSocketInfoAsObject)
	{
		TcpSocketInfo tcpSocketInfo = null;
		try
		{
			tcpSocketInfo = (TcpSocketInfo)tcpSocketInfoAsObject;
			lock (tcpSocketInfo)
			{
				if (tcpSocketInfo.MessageContainer.IsMessageAvailable)
				{
					return;
				}
			}
			Message message = new Message(ITransportContext, tcpSocketInfo.Remote, -3, new TransportHeaders(), Stream.Null);
			message.IsSynchronous = false;
			message.IsOneWay = true;
			message.SerializedContent = Stream.Null;
			BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.MessageProcessing] > 0)
			{
				binaryLogWriter.WriteMessageCreatedEvent("TcpConnectionManager.SendPing", LogMessageType.MessageCreated, null, message, true, tcpSocketInfo.Remote, null, "TCP PING", "TCP PING", GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, tcpSocketInfo.DbgConnectionId, -1, null, -1, null, "TCP ping is created and sent.");
			}
			SendAsync(message, tcpSocketInfo);
		}
		catch (Exception exception)
		{
			BinaryLogWriter binaryLogWriter2 = ITransportContext.BinaryLogWriter;
			if (binaryLogWriter2 != null && binaryLogWriter2[LogCategory.Connection] > 0)
			{
				binaryLogWriter2.WriteEvent(LogCategory.Connection, "TcpConnectionManager.SendPing", LogMessageType.ConnectionPingSending, exception, null, tcpSocketInfo?.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "Cannot send a ping.");
			}
		}
	}

	public void LowLevel_SendSync(Message message, TcpSocketInfo tcpSocketInfo)
	{
		Stream stream = null;
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		if (binaryLogWriter != null && binaryLogWriter[LogCategory.Transport] > 0)
		{
			binaryLogWriter.WriteEvent(LogCategory.Transport, "TcpConnectionManager.LowLevel_SendSync", LogMessageType.MessageIsSentSynchronously, null, message, message.Recipient, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, (tcpSocketInfo.ConnectionLevelSecurity == null) ? null : tcpSocketInfo.ConnectionLevelSecurity, (tcpSocketInfo.ConnectionLevelSecurity == null) ? null : tcpSocketInfo.ConnectionLevelSecurity.Name, tcpSocketInfo.DbgConnectionId, 0, 0, 0, null, null, null, null, "The connection has been obtained and the message is being sent synchronously.");
		}
		try
		{
			if (tcpSocketInfo.GenuineConnectionType == GenuineConnectionType.Persistent)
			{
				tcpSocketInfo.LastTimeContentWasSent = GenuineUtility.TickCount;
			}
			tcpSocketInfo.SetupWriting(message.FinishTime);
			bool flag = false;
			stream = message.SerializedContent;
			if (tcpSocketInfo.ConnectionLevelSecurity != null)
			{
				GenuineChunkedStream genuineChunkedStream = new GenuineChunkedStream(releaseOnReadMode: true);
				tcpSocketInfo.ConnectionLevelSecurity.Encrypt(stream, genuineChunkedStream);
				stream = genuineChunkedStream;
			}
			while (!flag)
			{
				int i = 6;
				int num = 0;
				for (; tcpSocketInfo._sendBuffer.Length > i; i += num)
				{
					if ((num = stream.Read(tcpSocketInfo._sendBuffer, i, tcpSocketInfo._sendBuffer.Length - i)) <= 0)
					{
						break;
					}
					if (num <= 0)
					{
						break;
					}
				}
				flag = i < tcpSocketInfo._sendBuffer.Length;
				int num2 = i;
				tcpSocketInfo._sendBuffer[0] = 55;
				MessageCoder.WriteInt32(tcpSocketInfo._sendBuffer, 1, num2 - 6);
				tcpSocketInfo._sendBuffer[5] = (byte)(flag ? 1 : 0);
				i = 0;
				tcpSocketInfo.Write(tcpSocketInfo._sendBuffer, i, num2);
				IncreaseBytesSent(num2);
			}
			if (tcpSocketInfo.GenuineConnectionType == GenuineConnectionType.Persistent && !(bool)ITransportContext.IParameterProvider[GenuineParameter.TcpDoNotResendMessages])
			{
				tcpSocketInfo.MessagesSentSynchronously.PutMessage(message);
			}
		}
		catch (Exception exception)
		{
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Transport] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Transport, "TcpConnectionManager.LowLevel_SendSync", LogMessageType.MessageIsSentSynchronously, exception, message, message.Recipient, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, (tcpSocketInfo.ConnectionLevelSecurity == null) ? null : tcpSocketInfo.ConnectionLevelSecurity, (tcpSocketInfo.ConnectionLevelSecurity == null) ? null : tcpSocketInfo.ConnectionLevelSecurity.Name, tcpSocketInfo.DbgConnectionId, 0, 0, 0, null, null, null, null, "The exception occurred while synchronous sending.");
			}
			throw;
		}
		finally
		{
			lock (tcpSocketInfo)
			{
				tcpSocketInfo.LockForSending = false;
			}
		}
		if (tcpSocketInfo.GenuineConnectionType == GenuineConnectionType.Persistent)
		{
			tcpSocketInfo.LastTimeContentWasSent = GenuineUtility.TickCount;
		}
	}

	private Stream LowLevel_ReadSync(TcpSocketInfo tcpSocketInfo, int timeoutInMilliseconds, bool automaticallyContinueReading)
	{
		Stream stream = new SyncSocketReadingStream(this, tcpSocketInfo, timeoutInMilliseconds, automaticallyContinueReading);
		if (tcpSocketInfo.ConnectionLevelSecurity != null)
		{
			return tcpSocketInfo.ConnectionLevelSecurity.Decrypt(stream);
		}
		return stream;
	}

	private void LowLevel_StartSending(Message message, TcpSocketInfo tcpSocketInfo)
	{
		if (tcpSocketInfo.GenuineConnectionType == GenuineConnectionType.Persistent)
		{
			tcpSocketInfo.LastTimeContentWasSent = GenuineUtility.TickCount;
		}
		if (tcpSocketInfo.AsyncSendStream == null)
		{
			tcpSocketInfo.AsyncSendStream = message.SerializedContent;
			if (tcpSocketInfo.ConnectionLevelSecurity != null && message.SerializedContent != Stream.Null)
			{
				GenuineChunkedStream genuineChunkedStream = new GenuineChunkedStream(releaseOnReadMode: true);
				tcpSocketInfo.ConnectionLevelSecurity.Encrypt(tcpSocketInfo.AsyncSendStream, genuineChunkedStream);
				tcpSocketInfo.AsyncSendStream = genuineChunkedStream;
			}
		}
		tcpSocketInfo.Message = message;
		if (tcpSocketInfo.AsyncSendBuffer == null)
		{
			tcpSocketInfo.AsyncSendBuffer = new byte[tcpSocketInfo.MaxSendSize];
		}
		tcpSocketInfo.AsyncSendBufferCurrentPosition = 0;
		tcpSocketInfo.AsyncSendBufferSizeOfValidContent = 6 + GenuineUtility.TryToReadFromStream(tcpSocketInfo.AsyncSendStream, tcpSocketInfo.AsyncSendBuffer, 6, tcpSocketInfo.AsyncSendBuffer.Length - 6);
		tcpSocketInfo.AsyncSendBuffer[0] = 55;
		MessageCoder.WriteInt32(tcpSocketInfo.AsyncSendBuffer, 1, tcpSocketInfo.AsyncSendBufferSizeOfValidContent - 6);
		tcpSocketInfo.AsyncSendBufferIsLastPacket = tcpSocketInfo.AsyncSendBufferSizeOfValidContent < tcpSocketInfo.AsyncSendBuffer.Length - 6;
		tcpSocketInfo.AsyncSendBuffer[5] = (byte)(tcpSocketInfo.AsyncSendBufferIsLastPacket ? 1 : 0);
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		if (binaryLogWriter != null && binaryLogWriter[LogCategory.Transport] > 0)
		{
			binaryLogWriter.WriteTransportContentEvent(LogCategory.Transport, "TcpConnectionManager.LowLevel_StartSending", LogMessageType.AsynchronousSendingStarted, null, tcpSocketInfo.Message, tcpSocketInfo.Remote, (binaryLogWriter[LogCategory.Transport] > 1) ? new MemoryStream(GenuineUtility.CutOutBuffer(tcpSocketInfo.AsyncSendBuffer, tcpSocketInfo.AsyncSendBufferCurrentPosition, tcpSocketInfo.AsyncSendBufferSizeOfValidContent - tcpSocketInfo.AsyncSendBufferCurrentPosition)) : null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, tcpSocketInfo.DbgConnectionId, tcpSocketInfo.AsyncSendBufferSizeOfValidContent - tcpSocketInfo.AsyncSendBufferCurrentPosition, tcpSocketInfo.Remote.PhysicalAddress.ToString(), null, null, "The chunk is being sent asynchronously.");
		}
		AsyncThreadStarter.QueueTask(new Async_InitiateSocketSending(tcpSocketInfo.Socket, tcpSocketInfo.AsyncSendBuffer, tcpSocketInfo.AsyncSendBufferCurrentPosition, tcpSocketInfo.AsyncSendBufferSizeOfValidContent - tcpSocketInfo.AsyncSendBufferCurrentPosition, _onEndSending, tcpSocketInfo));
	}

	private void LowLevel_EndSending(IAsyncResult ar)
	{
		TcpSocketInfo tcpSocketInfo = null;
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		try
		{
			tcpSocketInfo = ar.AsyncState as TcpSocketInfo;
			if (tcpSocketInfo == null)
			{
				binaryLogWriter?.WriteImplementationWarningEvent("TcpConnectionManager.LowLevel_EndSending", LogMessageType.AsynchronousSendingFinished, GenuineExceptions.Get_Debugging_GeneralWarning("TcpSocketInfo is NULL!"), GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, "Unexpected situation: the TcpSocketInfo is null.");
				return;
			}
			int num = tcpSocketInfo.Socket.EndSend(ar);
			IncreaseBytesSent(num);
			tcpSocketInfo.AsyncSendBufferCurrentPosition += num;
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.LowLevelTransport] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.LowLevelTransport, "TcpConnectionManager.LowLevel_EndSending", LogMessageType.LowLevelTransport_AsyncSendingCompleted, null, tcpSocketInfo.Message, tcpSocketInfo.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, tcpSocketInfo.ConnectionLevelSecurity, (tcpSocketInfo.ConnectionLevelSecurity == null) ? null : tcpSocketInfo.ConnectionLevelSecurity.Name, tcpSocketInfo.DbgConnectionId, tcpSocketInfo.AsyncSendBuffer.GetHashCode(), num, 0, null, null, null, null, "The asynchronous sending completed.");
			}
			tcpSocketInfo.MessagesSentSynchronously.ReleaseAllMessages();
			if (tcpSocketInfo.AsyncSendBufferCurrentPosition >= tcpSocketInfo.AsyncSendBufferSizeOfValidContent && !tcpSocketInfo.AsyncSendBufferIsLastPacket)
			{
				LowLevel_StartSending(tcpSocketInfo.Message, tcpSocketInfo);
				return;
			}
			if (tcpSocketInfo.AsyncSendBufferCurrentPosition < tcpSocketInfo.AsyncSendBufferSizeOfValidContent)
			{
				AsyncThreadStarter.QueueTask(new Async_InitiateSocketSending(tcpSocketInfo.Socket, tcpSocketInfo.AsyncSendBuffer, tcpSocketInfo.AsyncSendBufferCurrentPosition, tcpSocketInfo.AsyncSendBufferSizeOfValidContent - tcpSocketInfo.AsyncSendBufferCurrentPosition, _onEndSending, tcpSocketInfo));
				return;
			}
			lock (tcpSocketInfo)
			{
				tcpSocketInfo.AsyncSendStream.Close();
				if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
				{
					binaryLogWriter.WriteEvent(LogCategory.Connection, "TcpConnectionManager.SendAsync", LogMessageType.MessageHasBeenSentAsynchronously, null, tcpSocketInfo.Message, tcpSocketInfo.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, tcpSocketInfo.ConnectionLevelSecurity, (tcpSocketInfo.ConnectionLevelSecurity == null) ? null : tcpSocketInfo.ConnectionLevelSecurity.Name, tcpSocketInfo.DbgConnectionId, 0, 0, 0, null, null, null, null, "The message has been sent asynchronously.");
				}
				if (tcpSocketInfo.Message != null)
				{
					tcpSocketInfo.Message.Dispose();
					tcpSocketInfo.Message = null;
				}
				if (tcpSocketInfo.GenuineConnectionType == GenuineConnectionType.Persistent)
				{
					tcpSocketInfo.LastTimeContentWasSent = GenuineUtility.TickCount;
				}
				tcpSocketInfo.Message = null;
				tcpSocketInfo.LockForSending = false;
			}
			Pool_StartSending(tcpSocketInfo);
		}
		catch (Exception exception)
		{
			if (tcpSocketInfo.Message != null)
			{
				tcpSocketInfo.Message.SerializedContent.Position = 0L;
			}
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Transport] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Connection, "TcpConnectionManager.LowLevel_EndSending", LogMessageType.AsynchronousSendingFinished, exception, tcpSocketInfo.Message, tcpSocketInfo.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, tcpSocketInfo.ConnectionLevelSecurity, (tcpSocketInfo.ConnectionLevelSecurity == null) ? null : tcpSocketInfo.ConnectionLevelSecurity.Name, tcpSocketInfo.DbgConnectionId, 0, 0, 0, null, null, null, null, "The exception occurred while sending an asynchronous message.");
			}
			SocketFailed(exception, tcpSocketInfo);
		}
	}

	internal void LowLevel_Client_PreventDelayedAck(object tcpSocketInfoAsObject)
	{
		TcpSocketInfo tcpSocketInfo = (TcpSocketInfo)tcpSocketInfoAsObject;
		try
		{
			Message message = new Message(ITransportContext, tcpSocketInfo.Remote, -3, new TransportHeaders(), Stream.Null);
			message.IsSynchronous = true;
			message.SerializedContent = Stream.Null;
			SendSync(message, tcpSocketInfo);
		}
		catch (Exception exception)
		{
			BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Transport] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Transport, "TcpConnectionManager.LowLevel_Client_PreventDelayedAck", LogMessageType.SynchronousSendingFinished, exception, null, tcpSocketInfo.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, tcpSocketInfo.ConnectionLevelSecurity, (tcpSocketInfo.ConnectionLevelSecurity == null) ? null : tcpSocketInfo.ConnectionLevelSecurity.Name, tcpSocketInfo.DbgConnectionId, 0, 0, 0, null, null, null, null, "The exception occurred while sending a synchronous message.");
			}
		}
	}

	internal void LowLevel_HalfSync_StartReceiving(TcpSocketInfo tcpSocketInfo)
	{
		if (tcpSocketInfo.ReceivingHeaderBuffer == null)
		{
			tcpSocketInfo.ReceivingHeaderBuffer = new byte[6];
		}
		tcpSocketInfo.ReceivingBufferExpectedSize = 6;
		tcpSocketInfo.ReceivingBufferCurrentPosition = 0;
		tcpSocketInfo.IsHeaderIsBeingReceived = true;
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		if (binaryLogWriter != null && binaryLogWriter[LogCategory.Transport] > 0)
		{
			binaryLogWriter.WriteEvent(LogCategory.Transport, "TcpConnectionManager.LowLevel_HalfSync_StartReceiving", LogMessageType.ReceivingStarted, null, null, tcpSocketInfo.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, tcpSocketInfo.ConnectionLevelSecurity, (tcpSocketInfo.ConnectionLevelSecurity == null) ? null : tcpSocketInfo.ConnectionLevelSecurity.Name, tcpSocketInfo.DbgConnectionId, 0, 0, 0, null, null, null, null, "The asynchronous receiving is initiated. Requested size: {0}.", tcpSocketInfo.ReceivingBufferExpectedSize - tcpSocketInfo.ReceivingBufferCurrentPosition);
		}
		AsyncThreadStarter.QueueTask(new Async_InitiateSocketReceiving(tcpSocketInfo.Socket, tcpSocketInfo.ReceivingHeaderBuffer, tcpSocketInfo.ReceivingBufferCurrentPosition, tcpSocketInfo.ReceivingBufferExpectedSize - tcpSocketInfo.ReceivingBufferCurrentPosition, _HalfSync_onEndReceiving, tcpSocketInfo));
		if (!tcpSocketInfo.IsServer && (bool)ITransportContext.IParameterProvider[GenuineParameter.TcpPreventDelayedAck])
		{
			GenuineThreadPool.QueueUserWorkItem(_lowLevel_Client_PreventDelayedAck, tcpSocketInfo, longDuration: true);
		}
	}

	private void LowLevel_HalfSync_EndReceiving(IAsyncResult ar)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		TcpSocketInfo tcpSocketInfo = (TcpSocketInfo)ar.AsyncState;
		try
		{
			int num = tcpSocketInfo.Socket.EndReceive(ar);
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.LowLevelTransport] > 0)
			{
				binaryLogWriter.WriteTransportContentEvent(LogCategory.LowLevelTransport, "TcpConnectionManager.LowLevel_HalfSync_EndReceiving", LogMessageType.LowLevelTransport_AsyncReceivingCompleted, null, null, tcpSocketInfo.Remote, (binaryLogWriter[LogCategory.LowLevelTransport] > 1) ? new MemoryStream(tcpSocketInfo.ReceivingHeaderBuffer, tcpSocketInfo.ReceivingBufferCurrentPosition, num) : null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, tcpSocketInfo.DbgConnectionId, num, tcpSocketInfo.Socket.RemoteEndPoint.ToString(), null, null, "Socket.EndReceive(). Bytes received: {0}. Socket: {1}.", num, (long)tcpSocketInfo.Socket.Handle);
			}
			tcpSocketInfo.ReceivingBufferCurrentPosition += num;
			if (tcpSocketInfo.ReceivingBufferCurrentPosition == 0)
			{
				throw GenuineExceptions.Get_Receive_Portion();
			}
			if (tcpSocketInfo.ReceivingBufferCurrentPosition < tcpSocketInfo.ReceivingBufferExpectedSize)
			{
				AsyncThreadStarter.QueueTask(new Async_InitiateSocketReceiving(tcpSocketInfo.Socket, tcpSocketInfo.ReceivingHeaderBuffer, tcpSocketInfo.ReceivingBufferCurrentPosition, tcpSocketInfo.ReceivingBufferExpectedSize - tcpSocketInfo.ReceivingBufferCurrentPosition, _HalfSync_onEndReceiving, tcpSocketInfo));
			}
			else
			{
				GenuineThreadPool.QueueUserWorkItem(_HalfSync_onContinueReceiving, tcpSocketInfo, longDuration: true);
			}
		}
		catch (Exception exception)
		{
			SocketFailed(exception, tcpSocketInfo);
		}
	}

	private void LowLevel_CloseConnection(object tcpSocketInfoAsObject)
	{
		TcpSocketInfo tcpSocketInfo = (TcpSocketInfo)tcpSocketInfoAsObject;
		if (tcpSocketInfo.Socket != null)
		{
			SocketUtility.CloseSocket(tcpSocketInfo.Socket);
		}
	}

	internal TcpSocketInfo LowLevel_OpenConnection(HostInformation remote, GenuineConnectionType genuineConnectionType, string localUri, int localPort, string connectionName, out string remoteUri, out int remoteHostUniqueIdentifier)
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
		remoteUri = null;
		Stream stream = null;
		Stream stream2 = null;
		remoteHostUniqueIdentifier = 0;
		string url = remote.Url;
		string text = GenuineUtility.SplitToHostAndPort(url, out var port);
		if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
		{
			binaryLogWriter.WriteEvent(LogCategory.Connection, "TcpConnectionManager.LowLevel_OpenConnection", LogMessageType.ConnectionEstablishing, null, null, remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "The connection is being established to {0}.", text);
		}
		Socket socket = null;
		int timeout = GenuineUtility.GetTimeout((TimeSpan)ITransportContext.IParameterProvider[GenuineParameter.ConnectTimeout]);
		using (ConnectionEstablishingClosure connectionEstablishingClosure = new ConnectionEstablishingClosure(text, port, ITransportContext.IParameterProvider))
		{
			connectionEstablishingClosure.StartOperation();
			if (!connectionEstablishingClosure.Completed.WaitOne(GenuineUtility.GetMillisecondsLeft(timeout), exitContext: false))
			{
				throw GenuineExceptions.Get_Connect_CanNotConnectToRemoteHost(url, "Timeout expired.");
			}
			if (connectionEstablishingClosure.Exception != null)
			{
				throw GenuineExceptions.Get_Connect_CanNotConnectToRemoteHost(url, connectionEstablishingClosure.Exception.Message);
			}
			socket = connectionEstablishingClosure.CompleteOperation();
		}
		if (!socket.Connected)
		{
			throw GenuineExceptions.Get_Connect_CanNotConnectToRemoteHost(url, "Socket.Connected property is false after connecting.");
		}
		if (connectionName == null)
		{
			connectionName = "$/__GC/" + text;
		}
		TcpSocketInfo tcpSocketInfo = new TcpSocketInfo(socket, ITransportContext, connectionName);
		tcpSocketInfo.GenuineConnectionType = genuineConnectionType;
		if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
		{
			binaryLogWriter.WriteConnectionParameterEvent(LogCategory.Connection, "TcpConnectionManager.LowLevel_OpenConnection", LogMessageType.ConnectionParameters, null, remote, ITransportContext.IParameterProvider, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, tcpSocketInfo.DbgConnectionId, "The connection is being established to \"{0}\".", text);
		}
		Message message = new Message(null, null, -1, new TransportHeaders(), Stream.Null);
		message.FinishTime = timeout;
		message.SerializedContent = MessageCoder.SerializeConnectionHeader(3, genuineConnectionType, tcpSocketInfo.ConnectionName).BaseStream;
		LowLevel_SendSync(message, tcpSocketInfo);
		tcpSocketInfo.MessagesSentSynchronously.ReleaseAllMessages();
		string text2 = null;
		switch (genuineConnectionType)
		{
		case GenuineConnectionType.Persistent:
			text2 = ITransportContext.IParameterProvider[GenuineParameter.SecuritySessionForPersistentConnections] as string;
			break;
		case GenuineConnectionType.Named:
			text2 = ITransportContext.IParameterProvider[GenuineParameter.SecuritySessionForNamedConnections] as string;
			break;
		case GenuineConnectionType.Invocation:
			text2 = ITransportContext.IParameterProvider[GenuineParameter.SecuritySessionForInvocationConnections] as string;
			break;
		}
		SecuritySession securitySession = null;
		if (text2 != null)
		{
			securitySession = ITransportContext.IKeyStore.GetKey(text2).CreateSecuritySession(text2, null);
		}
		if (securitySession != null && !securitySession.IsEstablished)
		{
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Connection, "TcpConnectionManager.LowLevel_OpenConnection", LogMessageType.ConnectionSecurityIsEstablished, null, null, tcpSocketInfo.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, securitySession, text2, tcpSocketInfo.DbgConnectionId, 0, 0, 0, null, null, null, null, "The Connection Level Security Session is being established.");
			}
			bool flag = true;
			while (true)
			{
				stream = Stream.Null;
				try
				{
					if (!flag)
					{
						stream = LowLevel_ReadSync(tcpSocketInfo, timeout, automaticallyContinueReading: false);
					}
					else
					{
						flag = false;
					}
					stream2 = securitySession.EstablishSession(stream, connectionLevel: true);
					if (stream2 != null)
					{
						message = new Message(null, null, -1, new TransportHeaders(), Stream.Null);
						message.FinishTime = timeout;
						message.SerializedContent = stream2;
						LowLevel_SendSync(message, tcpSocketInfo);
						tcpSocketInfo.MessagesSentSynchronously.ReleaseAllMessages();
						if (!securitySession.IsEstablished)
						{
							continue;
						}
					}
				}
				finally
				{
					stream?.Close();
					stream2?.Close();
				}
				break;
			}
		}
		tcpSocketInfo.ConnectionLevelSecurity = securitySession;
		using (GenuineChunkedStream genuineChunkedStream = new GenuineChunkedStream(releaseOnReadMode: false))
		{
			BinaryWriter binaryWriter = new BinaryWriter(genuineChunkedStream);
			binaryWriter.Write(localUri);
			binaryWriter.Write(-1);
			binaryWriter.Write((byte)genuineConnectionType);
			binaryWriter.Write(remote.LocalHostUniqueIdentifier);
			message = new Message(null, null, -1, new TransportHeaders(), Stream.Null);
			message.FinishTime = timeout;
			message.SerializedContent = genuineChunkedStream;
			LowLevel_SendSync(message, tcpSocketInfo);
			tcpSocketInfo.MessagesSentSynchronously.ReleaseAllMessages();
			using Stream input = LowLevel_ReadSync(tcpSocketInfo, timeout, automaticallyContinueReading: false);
			BinaryReader binaryReader = new BinaryReader(input);
			remoteUri = binaryReader.ReadString();
			remoteHostUniqueIdentifier = binaryReader.ReadInt32();
		}
		tcpSocketInfo.IsServer = false;
		return tcpSocketInfo;
	}

	internal TcpSocketInfo LowLevel_AcceptConnection(Socket socket, string localUri, out string remoteUri, out string remoteAddress, out string connectionName, out int remoteHostUniqueIdentifier, out Exception reasonOfStateLost)
	{
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
		ThreadDataSlotKeeper threadDataSlotKeeper = new ThreadDataSlotKeeper("__GC__EstablishingSocket", socket);
		try
		{
			remoteUri = null;
			remoteAddress = null;
			remoteHostUniqueIdentifier = 0;
			Stream stream = null;
			Stream stream2 = null;
			int timeout = GenuineUtility.GetTimeout((TimeSpan)ITransportContext.IParameterProvider[GenuineParameter.ConnectTimeout]);
			LingerOption optionValue = new LingerOption(enable: true, 3);
			socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, optionValue);
			if ((bool)ITransportContext.IParameterProvider[GenuineParameter.TcpDisableNagling])
			{
				socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Debug, 1);
			}
			TcpSocketInfo tcpSocketInfo = new TcpSocketInfo(socket, ITransportContext, null);
			BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.AcceptingConnection] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.AcceptingConnection, "TcpConnectionManager.LowLevel_AcceptConnection", LogMessageType.ConnectionAccepting, null, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, tcpSocketInfo.DbgConnectionId, 0, 0, 0, socket.RemoteEndPoint.ToString(), socket.LocalEndPoint.ToString(), null, null, "An inbound connection is being accepted. Remote end point: {0}.", socket.RemoteEndPoint.ToString());
			}
			byte protocolVersion;
			GenuineConnectionType genuineConnectionType;
			using (Stream input = LowLevel_ReadSync(tcpSocketInfo, timeout, automaticallyContinueReading: false))
			{
				BinaryReader binaryReader = new BinaryReader(input);
				MessageCoder.DeserializeConnectionHeader(binaryReader, out protocolVersion, out genuineConnectionType, out connectionName);
			}
			tcpSocketInfo.GenuineConnectionType = genuineConnectionType;
			tcpSocketInfo.ConnectionName = connectionName;
			string text = null;
			switch (tcpSocketInfo.GenuineConnectionType)
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
			SecuritySession securitySession = null;
			if (text != null)
			{
				securitySession = ITransportContext.IKeyStore.GetKey(text).CreateSecuritySession(text, null);
			}
			if (securitySession != null && !securitySession.IsEstablished)
			{
				if (binaryLogWriter != null && binaryLogWriter[LogCategory.AcceptingConnection] > 0)
				{
					binaryLogWriter.WriteEvent(LogCategory.AcceptingConnection, "TcpConnectionManager.LowLevel_AcceptConnection", LogMessageType.ConnectionSecurityIsEstablished, null, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, securitySession, text, tcpSocketInfo.DbgConnectionId, 0, 0, 0, null, null, null, null, "The Connection Level Security Session must be established prior to connection is accepted.");
				}
				while (true)
				{
					stream = null;
					try
					{
						stream = LowLevel_ReadSync(tcpSocketInfo, timeout, automaticallyContinueReading: false);
						stream2 = securitySession.EstablishSession(stream, connectionLevel: true);
						if (stream2 != null)
						{
							Message message = new Message(null, null, -1, new TransportHeaders(), Stream.Null);
							message.FinishTime = timeout;
							message.SerializedContent = stream2;
							LowLevel_SendSync(message, tcpSocketInfo);
							tcpSocketInfo.MessagesSentSynchronously.ReleaseAllMessages();
							if (!securitySession.IsEstablished)
							{
								continue;
							}
						}
					}
					finally
					{
						stream?.Close();
						stream2?.Close();
					}
					break;
				}
			}
			tcpSocketInfo.ConnectionLevelSecurity = securitySession;
			int num = 0;
			using (Stream input2 = LowLevel_ReadSync(tcpSocketInfo, timeout, automaticallyContinueReading: false))
			{
				BinaryReader binaryReader2 = new BinaryReader(input2);
				remoteUri = binaryReader2.ReadString();
				num = binaryReader2.ReadInt32();
				tcpSocketInfo.GenuineConnectionType = (GenuineConnectionType)binaryReader2.ReadByte();
				if (protocolVersion >= 1)
				{
					remoteHostUniqueIdentifier = binaryReader2.ReadInt32();
				}
			}
			HostInformation hostInformation = ITransportContext.KnownHosts[remoteUri];
			hostInformation.ProtocolVersion = protocolVersion;
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
			{
				binaryLogWriter.WriteConnectionParameterEvent(LogCategory.Connection, "TcpConnectionManager.LowLevel_AcceptConnection", LogMessageType.ConnectionParameters, null, hostInformation, ITransportContext.IParameterProvider, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, tcpSocketInfo.DbgConnectionId, "The connection is accepted. Protocol version: \"{0}\".", protocolVersion);
			}
			reasonOfStateLost = hostInformation.UpdateUri(remoteUri, remoteHostUniqueIdentifier, tcpSocketInfo.GenuineConnectionType == GenuineConnectionType.Persistent);
			using (GenuineChunkedStream genuineChunkedStream = new GenuineChunkedStream(releaseOnReadMode: false))
			{
				BinaryWriter binaryWriter = new BinaryWriter(genuineChunkedStream);
				binaryWriter.Write(localUri);
				if (protocolVersion >= 1)
				{
					binaryWriter.Write(hostInformation.LocalHostUniqueIdentifier);
				}
				Message message2 = new Message(null, null, -1, new TransportHeaders(), Stream.Null);
				message2.FinishTime = timeout;
				message2.SerializedContent = genuineChunkedStream;
				LowLevel_SendSync(message2, tcpSocketInfo);
				tcpSocketInfo.MessagesSentSynchronously.ReleaseAllMessages();
			}
			if (num > 0)
			{
				IPEndPoint iPEndPoint = (IPEndPoint)socket.RemoteEndPoint;
				remoteAddress = "gtcp://" + iPEndPoint.Address.ToString() + ":" + num;
			}
			tcpSocketInfo.IsServer = true;
			return tcpSocketInfo;
		}
		finally
		{
			threadDataSlotKeeper.Dispose();
		}
	}

	private void ReestablishConnection(object stubSocketInfoAsObject)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		TcpSocketInfo tcpSocketInfo = (TcpSocketInfo)stubSocketInfoAsObject;
		HostInformation remote = tcpSocketInfo.Remote;
		TcpSocketInfo tcpSocketInfo2 = null;
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
		try
		{
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Connection, "TcpConnectionManager.ReestablishConnection", LogMessageType.ConnectionReestablishing, null, null, remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "The connection is being reestablished.");
			}
			remote.Renew(GenuineUtility.ConvertToMilliseconds(ITransportContext.IParameterProvider[GenuineParameter.MaxTimeSpanToReconnect]), canMakeShorter: false);
			int timeout = GenuineUtility.GetTimeout((TimeSpan)ITransportContext.IParameterProvider[GenuineParameter.MaxTimeSpanToReconnect]);
			int num = 0;
			int timeout2 = GenuineUtility.GetTimeout((TimeSpan)ITransportContext.IParameterProvider[GenuineParameter.MaxTimeSpanToReconnect]);
			string remoteUri = null;
			int remoteHostUniqueIdentifier;
			while (true)
			{
				tcpSocketInfo2 = null;
				Thread.Sleep((TimeSpan)ITransportContext.IParameterProvider[GenuineParameter.SleepBetweenReconnections]);
				if (num++ > (int)ITransportContext.IParameterProvider[GenuineParameter.ReconnectionTries] || GenuineUtility.IsTimeoutExpired(timeout2))
				{
					throw GenuineExceptions.Get_Channel_ReconnectionFailed();
				}
				if (GenuineUtility.IsTimeoutExpired(timeout))
				{
					throw GenuineExceptions.Get_Channel_ReconnectionFailed();
				}
				readerAutoLocker = new ReaderAutoLocker(_disposeLock);
				try
				{
					if (base._disposed)
					{
						throw GenuineExceptions.Get_Channel_ReconnectionFailed();
					}
				}
				finally
				{
					readerAutoLocker.Dispose();
				}
				lock (tcpSocketInfo)
				{
					if (!tcpSocketInfo.IsValid)
					{
						return;
					}
				}
				try
				{
					tcpSocketInfo2 = LowLevel_OpenConnection(remote, GenuineConnectionType.Persistent, Local.Uri, LocalPort, tcpSocketInfo.ConnectionName, out remoteUri, out remoteHostUniqueIdentifier);
					remote.PhysicalAddress = tcpSocketInfo2.Socket.RemoteEndPoint;
					remote.LocalPhysicalAddress = tcpSocketInfo2.Socket.LocalEndPoint;
				}
				catch (Exception exception)
				{
					if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
					{
						binaryLogWriter.WriteEvent(LogCategory.Connection, "TcpConnectionManager.ReestablishConnection", LogMessageType.ConnectionReestablishing, exception, null, remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "Reconnection failed. The current try: {0}. Milliseconds remained: {1}.", num, GenuineUtility.CompareTickCounts(timeout, GenuineUtility.TickCount));
					}
					continue;
				}
				break;
			}
			remote.UpdateUri(remoteUri, remoteHostUniqueIdentifier, checkHostInfoVersion: true);
			readerAutoLocker = new ReaderAutoLocker(_disposeLock);
			try
			{
				lock (_persistent.SyncRoot)
				{
					if (!(_persistent.Get(remote.Url, tcpSocketInfo.ConnectionName) is TcpSocketInfo tcpSocketInfo3))
					{
						GenuineThreadPool.QueueUserWorkItem(LowLevel_CloseConnection, tcpSocketInfo2, longDuration: false);
						return;
					}
					lock (tcpSocketInfo3)
					{
						if (!tcpSocketInfo3.IsValid)
						{
							GenuineThreadPool.QueueUserWorkItem(LowLevel_CloseConnection, tcpSocketInfo2, longDuration: false);
							return;
						}
						tcpSocketInfo2.MessageContainer = tcpSocketInfo3.MessageContainer;
						tcpSocketInfo2.Remote = tcpSocketInfo3.Remote;
						tcpSocketInfo2._iMessageRegistrator = tcpSocketInfo3._iMessageRegistrator;
						tcpSocketInfo2.CloseConnectionAfterInactivity = GenuineUtility.ConvertToMilliseconds(ITransportContext.IParameterProvider[GenuineParameter.ClosePersistentConnectionAfterInactivity]);
						tcpSocketInfo2.Renew();
						_persistent.Set(remote.Url, tcpSocketInfo.ConnectionName, tcpSocketInfo2);
					}
					Pool_StartSending(tcpSocketInfo2);
					Pool_InitiateReceiving(tcpSocketInfo2, tcpSocketInfo2.ITransportContext.IParameterProvider);
					tcpSocketInfo2.SignalState(GenuineEventType.GeneralConnectionEstablished, null, null);
					if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
					{
						binaryLogWriter.WriteEvent(LogCategory.Connection, "TcpConnectionManager.ReestablishConnection", LogMessageType.ConnectionReestablished, null, null, remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, tcpSocketInfo2.ConnectionLevelSecurity, (tcpSocketInfo2.ConnectionLevelSecurity == null) ? null : tcpSocketInfo2.ConnectionLevelSecurity.Name, tcpSocketInfo2?.DbgConnectionId ?? (-1), (int)tcpSocketInfo2.GenuineConnectionType, 0, 0, GetType().Name, remote.LocalPhysicalAddress.ToString(), remote.PhysicalAddress.ToString(), null, "The connection has been reestablished.");
					}
				}
			}
			finally
			{
				readerAutoLocker.Dispose();
			}
		}
		catch (Exception ex)
		{
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Connection, "TcpConnectionManager.ReestablishConnection", LogMessageType.ConnectionReestablished, ex, null, remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, tcpSocketInfo2?.DbgConnectionId ?? (-1), 0, 0, 0, null, null, null, null, "The connection cannot be reestablished.");
			}
			if (tcpSocketInfo2 != null)
			{
				LowLevel_CloseConnection(tcpSocketInfo2);
			}
			bool flag = false;
			lock (_persistent.SyncRoot)
			{
				tcpSocketInfo2 = _persistent.Get(remote.Url, tcpSocketInfo.ConnectionName) as TcpSocketInfo;
				if (object.ReferenceEquals(tcpSocketInfo, tcpSocketInfo2))
				{
					_persistent.Remove(remote.Url, tcpSocketInfo.ConnectionName);
					flag = true;
				}
			}
			if (flag)
			{
				ITransportContext.KnownHosts.ReleaseHostResources(remote, ex);
			}
			tcpSocketInfo.SignalState(GenuineEventType.GeneralConnectionClosed, ex, null);
			if (ex is OperationException ex2 && ex2.OperationErrorMessage.ErrorIdentifier == "GenuineChannels.Exception.Receive.ServerHasBeenRestarted")
			{
				ITransportContext.IGenuineEventProvider.Fire(new GenuineEventArgs(GenuineEventType.GeneralServerRestartDetected, GenuineExceptions.Get_Receive_ServerHasBeenRestared(), remote, null));
			}
		}
		finally
		{
		}
	}

	public void TimerCallback()
	{
		lock (this)
		{
			if (_isInProgress)
			{
				return;
			}
			_isInProgress = true;
		}
		GenuineThreadPool.QueueUserWorkItem(_internal_TimerCallback, null, longDuration: false);
	}

	private void FindExpiredConnections(object tcpSocketInfoAsObject, object findExpiredConnections_CollectedConnectionsAsObject)
	{
		TcpSocketInfo tcpSocketInfo = (TcpSocketInfo)tcpSocketInfoAsObject;
		FindExpiredConnections_CollectedConnections findExpiredConnections_CollectedConnections = (FindExpiredConnections_CollectedConnections)findExpiredConnections_CollectedConnectionsAsObject;
		if (GenuineUtility.IsTimeoutExpired(tcpSocketInfo.ShutdownTime, findExpiredConnections_CollectedConnections.Now))
		{
			findExpiredConnections_CollectedConnections.ExpiredConnections.Add(tcpSocketInfo);
		}
		if (GenuineUtility.IsTimeoutExpired(tcpSocketInfo.LastTimeContentWasSent + findExpiredConnections_CollectedConnections.SendPingAfterInactivity, findExpiredConnections_CollectedConnections.Now))
		{
			findExpiredConnections_CollectedConnections.SendPingTo.Add(tcpSocketInfo);
		}
	}

	private void Internal_TimerCallback(object ignored)
	{
		try
		{
			int tickCount = GenuineUtility.TickCount;
			int now = GenuineUtility.TickCount - GenuineUtility.ConvertToMilliseconds(ITransportContext.IParameterProvider[GenuineParameter.PersistentConnectionSendPingAfterInactivity]);
			ArrayList arrayList = new ArrayList();
			ArrayList arrayList2 = new ArrayList();
			FindExpiredConnections_CollectedConnections findExpiredConnections_CollectedConnections = new FindExpiredConnections_CollectedConnections();
			findExpiredConnections_CollectedConnections.ExpiredConnections = arrayList;
			findExpiredConnections_CollectedConnections.SendPingTo = arrayList2;
			findExpiredConnections_CollectedConnections.Now = tickCount;
			findExpiredConnections_CollectedConnections.SendPingAfterInactivity = GenuineUtility.ConvertToMilliseconds(ITransportContext.IParameterProvider[GenuineParameter.PersistentConnectionSendPingAfterInactivity]);
			_persistent.InspectAllConnections(_processObjectLateBoundHandler, findExpiredConnections_CollectedConnections);
			foreach (TcpSocketInfo item in arrayList)
			{
				SocketFailed(GenuineExceptions.Get_Channel_ConnectionClosedAfterTimeout(), item);
			}
			foreach (TcpSocketInfo item2 in arrayList2)
			{
				GenuineThreadPool.QueueUserWorkItem(SendPing, item2, longDuration: false);
			}
			arrayList.Clear();
			arrayList2.Clear();
			lock (_named)
			{
				foreach (object item3 in _named)
				{
					DictionaryEntry dictionaryEntry = (DictionaryEntry)item3;
					TcpSocketInfo tcpSocketInfo2 = (TcpSocketInfo)dictionaryEntry.Value;
					if (GenuineUtility.IsTimeoutExpired(tcpSocketInfo2.ShutdownTime, tickCount))
					{
						arrayList.Add(tcpSocketInfo2);
					}
					if (GenuineUtility.IsTimeoutExpired(tcpSocketInfo2.LastTimeContentWasSent, now))
					{
						arrayList2.Add(tcpSocketInfo2);
					}
				}
			}
			foreach (TcpSocketInfo item4 in arrayList)
			{
				SocketFailed(GenuineExceptions.Get_Channel_ConnectionClosedAfterTimeout(), item4);
			}
			foreach (TcpSocketInfo item5 in arrayList2)
			{
				GenuineThreadPool.QueueUserWorkItem(SendPing, item5, longDuration: false);
			}
			arrayList.Clear();
			lock (_invocation.SyncRoot)
			{
				foreach (object item6 in _invocation)
				{
					DictionaryEntry dictionaryEntry2 = (DictionaryEntry)item6;
					ArrayList arrayList3 = (ArrayList)dictionaryEntry2.Value;
					foreach (TcpSocketInfo item7 in arrayList3)
					{
						lock (item7)
						{
							if (GenuineUtility.IsTimeoutExpired(item7.ShutdownTime, tickCount))
							{
								arrayList.Add(item7);
							}
						}
					}
				}
			}
			foreach (TcpSocketInfo item8 in arrayList)
			{
				SocketFailed(GenuineExceptions.Get_Channel_ConnectionClosedAfterTimeout(), item8);
			}
		}
		catch
		{
		}
		finally
		{
			lock (this)
			{
				_isInProgress = false;
			}
		}
	}

	private TcpSocketInfo GetConnectionForSending(Message message)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		TcpSocketInfo tcpSocketInfo = null;
		string text = null;
		bool flag = false;
		string remoteUri = null;
		ReaderAutoLocker readerAutoLocker = new ReaderAutoLocker(_disposeLock);
		try
		{
			if (base._disposed)
			{
				throw OperationException.WrapException(_disposeReason);
			}
			int remoteHostUniqueIdentifier;
			switch (message.SecuritySessionParameters.GenuineConnectionType)
			{
			case GenuineConnectionType.Persistent:
				text = message.Recipient.Url;
				if (message.ConnectionName == null)
				{
					message.ConnectionName = message.SecuritySessionParameters.ConnectionName;
				}
				lock (message.Recipient.PersistentConnectionEstablishingLock)
				{
					switch (message.Recipient.GenuinePersistentConnectionState)
					{
					case GenuinePersistentConnectionState.NotEstablished:
						if (message.Recipient.Url == null)
						{
							throw GenuineExceptions.Get_Send_DestinationIsUnreachable(message.Recipient.Uri);
						}
						message.Recipient.GenuinePersistentConnectionState = GenuinePersistentConnectionState.Opened;
						break;
					case GenuinePersistentConnectionState.Accepted:
						flag = true;
						text = message.Recipient.Uri;
						break;
					}
					if (!flag)
					{
						tcpSocketInfo = _persistent.Get(text, message.ConnectionName) as TcpSocketInfo;
						if (tcpSocketInfo != null && !tcpSocketInfo.IsValid)
						{
							_persistent.Remove(text, message.ConnectionName);
							tcpSocketInfo = null;
						}
						if (tcpSocketInfo == null)
						{
							tcpSocketInfo = LowLevel_OpenConnection(message.Recipient, GenuineConnectionType.Persistent, Local.Uri, LocalPort, message.ConnectionName, out remoteUri, out remoteHostUniqueIdentifier);
							message.Recipient.UpdateUri(remoteUri, remoteHostUniqueIdentifier);
							message.Recipient.PhysicalAddress = tcpSocketInfo.Socket.RemoteEndPoint;
							message.Recipient.LocalPhysicalAddress = tcpSocketInfo.Socket.LocalEndPoint;
							tcpSocketInfo.Remote = message.Recipient;
							if (binaryLogWriter != null && binaryLogWriter[LogCategory.HostInformation] > 0)
							{
								binaryLogWriter.WriteHostInformationEvent("TcpConnectionManager.LowLevel_OpenConnection", LogMessageType.HostInformationCreated, null, message.Recipient, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, tcpSocketInfo.ConnectionLevelSecurity, (tcpSocketInfo.ConnectionLevelSecurity == null) ? null : tcpSocketInfo.ConnectionLevelSecurity.Name, tcpSocketInfo.DbgConnectionId, "HostInformation is ready for actions.");
							}
							tcpSocketInfo.MessageContainer = new MessageContainer(ITransportContext);
							_persistent.Set(text, tcpSocketInfo.ConnectionName, tcpSocketInfo);
							tcpSocketInfo.CloseConnectionAfterInactivity = GenuineUtility.ConvertToMilliseconds(ITransportContext.IParameterProvider[GenuineParameter.ClosePersistentConnectionAfterInactivity]);
							tcpSocketInfo.Renew();
							tcpSocketInfo.SignalState(GenuineEventType.GeneralConnectionEstablished, null, null);
							if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
							{
								binaryLogWriter.WriteEvent(LogCategory.Connection, "TcpConnectionManager.GetConnectionForSending", LogMessageType.ConnectionEstablished, null, null, tcpSocketInfo.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, tcpSocketInfo.ConnectionLevelSecurity, (tcpSocketInfo.ConnectionLevelSecurity == null) ? null : tcpSocketInfo.ConnectionLevelSecurity.Name, tcpSocketInfo.DbgConnectionId, (int)message.SecuritySessionParameters.GenuineConnectionType, 0, 0, GetType().Name, message.Recipient.LocalPhysicalAddress.ToString(), message.Recipient.PhysicalAddress.ToString(), null, "The connection to the remote host is established.");
							}
							Pool_StartSending(tcpSocketInfo);
							Pool_InitiateReceiving(tcpSocketInfo, ITransportContext.IParameterProvider);
						}
					}
					else
					{
						tcpSocketInfo = _persistent.Get(text, message.ConnectionName) as TcpSocketInfo;
						if (tcpSocketInfo == null)
						{
							throw GenuineExceptions.Get_Send_DestinationIsUnreachable(message.Recipient.Uri);
						}
					}
				}
				break;
			case GenuineConnectionType.Named:
			{
				if (message.ConnectionName == null)
				{
					message.ConnectionName = message.SecuritySessionParameters.ConnectionName;
				}
				flag = message.Recipient.Url == null;
				string namedConnectionName = GetNamedConnectionName(flag, message.Recipient, message.ConnectionName);
				try
				{
					if (!flag)
					{
						bool flag2 = false;
						lock (_named)
						{
							tcpSocketInfo = _named[namedConnectionName] as TcpSocketInfo;
							if (tcpSocketInfo == null)
							{
								flag2 = true;
								tcpSocketInfo = new TcpSocketInfo(null, ITransportContext, message.ConnectionName);
								tcpSocketInfo.Remote = message.Recipient;
								tcpSocketInfo.GenuineConnectionType = GenuineConnectionType.Named;
								tcpSocketInfo.LockForSending = true;
								tcpSocketInfo.CloseConnectionAfterInactivity = GenuineUtility.ConvertToMilliseconds(ITransportContext.IParameterProvider[GenuineParameter.ConnectTimeout]);
								tcpSocketInfo.MessageContainer = new MessageContainer(ITransportContext);
								tcpSocketInfo.Renew();
								_named[namedConnectionName] = tcpSocketInfo;
							}
						}
						if (flag2)
						{
							tcpSocketInfo = LowLevel_OpenConnection(message.Recipient, GenuineConnectionType.Named, Local.Uri, LocalPort, message.ConnectionName, out remoteUri, out remoteHostUniqueIdentifier);
							message.Recipient.UpdateUri(remoteUri, remoteHostUniqueIdentifier);
							message.Recipient.PhysicalAddress = tcpSocketInfo.Socket.RemoteEndPoint;
							message.Recipient.LocalPhysicalAddress = tcpSocketInfo.Socket.LocalEndPoint;
							tcpSocketInfo.Remote = message.Recipient;
							if (message.SecuritySessionParameters.CloseAfterInactivity == TimeSpan.MinValue)
							{
								tcpSocketInfo.CloseConnectionAfterInactivity = GenuineUtility.ConvertToMilliseconds(ITransportContext.IParameterProvider[GenuineParameter.CloseNamedConnectionAfterInactivity]);
							}
							else
							{
								tcpSocketInfo.CloseConnectionAfterInactivity = GenuineUtility.ConvertToMilliseconds(message.SecuritySessionParameters.CloseAfterInactivity);
							}
							tcpSocketInfo.Renew();
							lock (_named)
							{
								TcpSocketInfo tcpSocketInfo2 = (TcpSocketInfo)_named[namedConnectionName];
								_named[namedConnectionName] = tcpSocketInfo;
								tcpSocketInfo.MessageContainer = tcpSocketInfo2.MessageContainer;
							}
							if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
							{
								binaryLogWriter.WriteEvent(LogCategory.Connection, "TcpConnectionManager.GetConnectionForSending", LogMessageType.ConnectionEstablished, null, null, tcpSocketInfo.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, tcpSocketInfo.ConnectionLevelSecurity, (tcpSocketInfo.ConnectionLevelSecurity == null) ? null : tcpSocketInfo.ConnectionLevelSecurity.Name, tcpSocketInfo.DbgConnectionId, (int)message.SecuritySessionParameters.GenuineConnectionType, 0, 0, GetType().Name, message.Recipient.LocalPhysicalAddress.ToString(), message.Recipient.PhysicalAddress.ToString(), null, "The connection to the remote host is established.");
							}
							Pool_StartSending(tcpSocketInfo);
							Pool_InitiateReceiving(tcpSocketInfo, ITransportContext.IParameterProvider);
						}
						break;
					}
					lock (_named)
					{
						tcpSocketInfo = _named[namedConnectionName] as TcpSocketInfo;
					}
					if (tcpSocketInfo == null)
					{
						Exception ex = GenuineExceptions.Get_Send_DestinationIsUnreachable(message.Recipient.Uri);
						if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
						{
							binaryLogWriter.WriteEvent(LogCategory.Connection, "TcpConnectionManager.GetConnectionForSending", LogMessageType.ConnectionEstablished, ex, message, message.Recipient, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, (int)message.SecuritySessionParameters.GenuineConnectionType, 0, 0, GetType().Name, null, null, null, "The connection to the remote host cannot be established.");
						}
						throw ex;
					}
				}
				catch (Exception exception)
				{
					tcpSocketInfo.Remote = message.Recipient;
					SocketFailed(exception, tcpSocketInfo);
					TcpSocketInfo tcpSocketInfo3 = null;
					lock (_named)
					{
						tcpSocketInfo3 = _named[namedConnectionName] as TcpSocketInfo;
					}
					SocketFailed(exception, tcpSocketInfo3);
					throw;
				}
				break;
			}
			case GenuineConnectionType.Invocation:
				throw new ApplicationException("This version of Genuine Channels does not support the Invocation Connection Pattern.");
			default:
				throw GenuineExceptions.Get_Connect_CanNotConnectToRemoteHost(message.Recipient.PrimaryUri, "Invalid type of connection requested.");
			}
		}
		finally
		{
			readerAutoLocker.Dispose();
		}
		return tcpSocketInfo;
	}

	private void Pool_StartSending(TcpSocketInfo tcpSocketInfo)
	{
		Message message = null;
		switch (tcpSocketInfo.GenuineConnectionType)
		{
		case GenuineConnectionType.Persistent:
		case GenuineConnectionType.Named:
			lock (tcpSocketInfo)
			{
				if (tcpSocketInfo.LockForSending)
				{
					break;
				}
				tcpSocketInfo.LockForSending = true;
				while (true)
				{
					message = tcpSocketInfo.MessageContainer.GetMessage();
					if (message == null)
					{
						tcpSocketInfo.LockForSending = false;
						return;
					}
					if (!(message is SyncMessageSlot))
					{
						break;
					}
					SyncMessageSlot syncMessageSlot = (SyncMessageSlot)message;
					lock (syncMessageSlot)
					{
						if (!syncMessageSlot.IsValid)
						{
							continue;
						}
						syncMessageSlot.TcpSocketInfo = tcpSocketInfo;
						syncMessageSlot.ConnectionAvailable.Set();
						return;
					}
				}
				BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
				if (binaryLogWriter != null && binaryLogWriter[LogCategory.Transport] > 0)
				{
					binaryLogWriter.WriteEvent(LogCategory.Transport, "TcpConnectionManager.SendAsync", LogMessageType.MessageIsSentAsynchronously, null, message, message.Recipient, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, (tcpSocketInfo.ConnectionLevelSecurity == null) ? null : tcpSocketInfo.ConnectionLevelSecurity, (tcpSocketInfo.ConnectionLevelSecurity == null) ? null : tcpSocketInfo.ConnectionLevelSecurity.Name, tcpSocketInfo.DbgConnectionId, 0, 0, 0, null, null, null, null, "The connection has been obtained and the message is being sent asynchronously.");
				}
				tcpSocketInfo.Message = message;
				tcpSocketInfo.AsyncSendStream = null;
				LowLevel_StartSending(message, tcpSocketInfo);
				break;
			}
		case GenuineConnectionType.Invocation:
			Pool_HandleAvailableInvocationOrOneWayConnection(tcpSocketInfo);
			break;
		case GenuineConnectionType.Persistent | GenuineConnectionType.Named:
			break;
		}
	}

	private void Pool_InitiateReceiving(TcpSocketInfo tcpSocketInfo, IParameterProvider iParameterProvider)
	{
		LowLevel_HalfSync_StartReceiving(tcpSocketInfo);
	}

	private void Pool_HandleAvailableInvocationOrOneWayConnection(TcpSocketInfo tcpSocketInfo)
	{
		if (tcpSocketInfo.CloseConnectionAfterInactivity <= 0)
		{
			SocketFailed(GenuineExceptions.Get_Connection_LifetimeCycleEnded(), tcpSocketInfo);
		}
		else
		{
			tcpSocketInfo.Renew();
		}
	}

	private void Pool_DestroyPersistentConnection(TcpSocketInfo tcpSocketInfo, Exception reason)
	{
		bool flag = false;
		bool flag2 = true;
		lock (_persistent.SyncRoot)
		{
			TcpSocketInfo objA = _persistent.Get(tcpSocketInfo.PrimaryRemoteUri, tcpSocketInfo.ConnectionName) as TcpSocketInfo;
			if (object.ReferenceEquals(objA, tcpSocketInfo))
			{
				_persistent.Remove(tcpSocketInfo.PrimaryRemoteUri, tcpSocketInfo.ConnectionName);
				flag2 = _persistent.Get(tcpSocketInfo.PrimaryRemoteUri, null) != null;
				flag = true;
			}
		}
		if (flag)
		{
			tcpSocketInfo.MessageContainer.Dispose(reason);
		}
		if (!flag2)
		{
			tcpSocketInfo.SignalState(GenuineEventType.GeneralConnectionClosed, reason, null);
			ITransportContext.KnownHosts.ReleaseHostResources(tcpSocketInfo.Remote, reason);
		}
		GenuineThreadPool.QueueUserWorkItem(LowLevel_CloseConnection, tcpSocketInfo, longDuration: false);
	}

	private void Pool_ContinueHalfSyncReceiving(object tcpSocketInfoAsObject)
	{
		TcpSocketInfo tcpSocketInfo = null;
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		try
		{
			tcpSocketInfo = (TcpSocketInfo)tcpSocketInfoAsObject;
			bool flag = tcpSocketInfo.ConnectionLevelSecurity == null;
			SyncSocketReadingStream syncSocketReadingStream = new SyncSocketReadingStream(this, tcpSocketInfo, GenuineUtility.GetTimeout(tcpSocketInfo.CloseConnectionAfterInactivity), flag);
			tcpSocketInfo.Renew();
			if (syncSocketReadingStream.IsMessageProcessed)
			{
				if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
				{
					binaryLogWriter.WriteEvent(LogCategory.Connection, "TcpConnectionManager.Pool_ContinueHalfSyncReceiving", LogMessageType.ConnectionPingReceived, null, null, tcpSocketInfo?.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, tcpSocketInfo.DbgConnectionId, 0, 0, 0, null, null, null, null, "The ping message has been received. Stream number: {0}.", syncSocketReadingStream.DbgStreamId);
				}
				if (!flag)
				{
					LowLevel_HalfSync_StartReceiving(tcpSocketInfo);
				}
				return;
			}
			Stream stream = null;
			bool flag2 = binaryLogWriter != null && binaryLogWriter[LogCategory.Transport] > 1;
			if (_tcpReadRequestBeforeProcessing || flag2)
			{
				stream = new GenuineChunkedStream(!flag2);
				GenuineUtility.CopyStreamToStream(syncSocketReadingStream, stream);
			}
			else
			{
				stream = syncSocketReadingStream;
			}
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Transport] > 0)
			{
				binaryLogWriter.WriteTransportContentEvent(LogCategory.Transport, "TcpConnectionManager.Pool_ContinueHalfSyncReceiving", LogMessageType.ReceivingFinished, null, null, tcpSocketInfo.Remote, flag2 ? stream : null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, tcpSocketInfo.DbgConnectionId, (int)((binaryLogWriter[LogCategory.Transport] > 1) ? stream.Length : 0), tcpSocketInfo.Remote.PhysicalAddress.ToString(), null, null, "Content has been received. Stream number: {0}.", syncSocketReadingStream.DbgStreamId);
			}
			if (tcpSocketInfo.ConnectionLevelSecurity != null)
			{
				stream = tcpSocketInfo.ConnectionLevelSecurity.Decrypt(stream);
			}
			if (!flag)
			{
				LowLevel_HalfSync_StartReceiving(tcpSocketInfo);
			}
			ITransportContext.IIncomingStreamHandler.HandleMessage(stream, tcpSocketInfo.Remote, tcpSocketInfo.GenuineConnectionType, tcpSocketInfo.ConnectionName, tcpSocketInfo.DbgConnectionId, useThisThread: true, tcpSocketInfo._iMessageRegistrator, tcpSocketInfo.ConnectionLevelSecurity, null);
			tcpSocketInfo.Renew();
		}
		catch (Exception exception)
		{
			SocketFailed(exception, tcpSocketInfo);
		}
	}

	private string GetNamedConnectionName(bool isServer, HostInformation hostInformation, string connectionName)
	{
		return isServer ? (hostInformation.Uri + "/" + connectionName) : (hostInformation.Url + "/" + connectionName);
	}

	private void SocketFailed(Exception exception, TcpSocketInfo tcpSocketInfo)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		try
		{
			lock (tcpSocketInfo)
			{
				if (!tcpSocketInfo.IsValid)
				{
					return;
				}
				tcpSocketInfo.IsValid = false;
				tcpSocketInfo.LockForSending = true;
			}
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Connection, "TcpConnectionManager.SocketFailed", LogMessageType.ConnectionFailed, exception, null, tcpSocketInfo.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, tcpSocketInfo.ConnectionLevelSecurity, (tcpSocketInfo.ConnectionLevelSecurity == null) ? null : tcpSocketInfo.ConnectionLevelSecurity.Name, tcpSocketInfo.DbgConnectionId, 0, 0, 0, null, null, null, null, "TCP connection has failed.");
			}
			switch (tcpSocketInfo.GenuineConnectionType)
			{
			case GenuineConnectionType.Persistent:
			{
				int num;
				if (!ConnectionManager.IsExceptionCritical(exception as OperationException) && (int)ITransportContext.IParameterProvider[GenuineParameter.ReconnectionTries] > 0)
				{
					TimeSpan timeSpan = (TimeSpan)ITransportContext.IParameterProvider[GenuineParameter.SleepBetweenReconnections];
					num = ((timeSpan.TotalMilliseconds <= 0.0) ? 1 : 0);
				}
				else
				{
					num = 1;
				}
				bool flag = (byte)num != 0;
				bool flag2 = false;
				lock (tcpSocketInfo)
				{
					try
					{
						if (tcpSocketInfo.Message != null)
						{
							tcpSocketInfo.Message.SerializedContent.Position = 0L;
							tcpSocketInfo.MessageContainer.AddMessage(tcpSocketInfo.Message, onlyCheckIn: false);
						}
						tcpSocketInfo.Message = null;
					}
					catch (Exception)
					{
						flag = true;
						flag2 = true;
					}
					try
					{
						tcpSocketInfo.MessagesSentSynchronously.MoveMessages(tcpSocketInfo.MessageContainer);
					}
					catch (Exception)
					{
						flag = true;
						flag2 = true;
					}
				}
				if (!object.ReferenceEquals(tcpSocketInfo, _persistent.Get(tcpSocketInfo.PrimaryRemoteUri, tcpSocketInfo.ConnectionName)) && !flag2)
				{
					GenuineThreadPool.QueueUserWorkItem(LowLevel_CloseConnection, tcpSocketInfo, longDuration: false);
					return;
				}
				if (flag)
				{
					if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
					{
						binaryLogWriter.WriteEvent(LogCategory.Connection, "TcpConnectionManager.SocketFailed", LogMessageType.ConnectionReestablished, exception, null, tcpSocketInfo.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, tcpSocketInfo.DbgConnectionId, 0, 0, 0, null, null, null, null, "The connection cannot be reestablished due to the critical exception.");
					}
					Pool_DestroyPersistentConnection(tcpSocketInfo, exception);
					return;
				}
				tcpSocketInfo.SignalState(GenuineEventType.GeneralConnectionReestablishing, exception, null);
				try
				{
					lock (_persistent.SyncRoot)
					{
						TcpSocketInfo objA = _persistent.Get(tcpSocketInfo.PrimaryRemoteUri, tcpSocketInfo.ConnectionName) as TcpSocketInfo;
						if (object.ReferenceEquals(objA, tcpSocketInfo))
						{
							lock (tcpSocketInfo)
							{
								TcpSocketInfo tcpSocketInfo2 = new TcpSocketInfo(null, ITransportContext, tcpSocketInfo.ConnectionName);
								tcpSocketInfo2.LockForSending = true;
								tcpSocketInfo2.MessageContainer = tcpSocketInfo.MessageContainer;
								tcpSocketInfo2.CloseConnectionAfterInactivity = GenuineUtility.ConvertToMilliseconds(ITransportContext.IParameterProvider[GenuineParameter.MaxTimeSpanToReconnect]);
								tcpSocketInfo2.Remote = tcpSocketInfo.Remote;
								tcpSocketInfo2.GenuineConnectionType = GenuineConnectionType.Persistent;
								tcpSocketInfo2.IsServer = tcpSocketInfo.IsServer;
								tcpSocketInfo2.Renew();
								if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
								{
									binaryLogWriter.WriteEvent(LogCategory.Connection, "TcpConnectionManager.SocketFailed", LogMessageType.ConnectionReestablishing, exception, null, tcpSocketInfo.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, tcpSocketInfo2.DbgConnectionId, 0, 0, 0, null, null, null, null, "The stub connection is constructed after exception.");
								}
								tcpSocketInfo2.Remote.Renew(tcpSocketInfo2.CloseConnectionAfterInactivity, canMakeShorter: false);
								_persistent.Set(tcpSocketInfo.PrimaryRemoteUri, tcpSocketInfo.ConnectionName, tcpSocketInfo2);
								if (tcpSocketInfo.Remote.GenuinePersistentConnectionState == GenuinePersistentConnectionState.Opened)
								{
									GenuineThreadPool.QueueUserWorkItem(ReestablishConnection, tcpSocketInfo2, longDuration: true);
								}
							}
						}
						else if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
						{
							binaryLogWriter.WriteEvent(LogCategory.Connection, "TcpConnectionManager.SocketFailed", LogMessageType.ConnectionReestablishing, exception, null, tcpSocketInfo.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, tcpSocketInfo.DbgConnectionId, 0, 0, 0, null, null, null, null, "The connection establishing has not been initiated since the original TcpSocketInfo is absent.");
						}
					}
				}
				catch (Exception exception2)
				{
					if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
					{
						binaryLogWriter.WriteEvent(LogCategory.Connection, "TcpConnectionManager.SocketFailed", LogMessageType.ConnectionReestablishing, exception2, null, tcpSocketInfo?.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, tcpSocketInfo.DbgConnectionId, 0, 0, 0, null, null, null, null, "An exception is occurred while trying to initiate connection reestablishing.");
					}
					SocketFailed(GenuineExceptions.Get_Receive_ConnectionClosed(), tcpSocketInfo);
					return;
				}
				break;
			}
			case GenuineConnectionType.Named:
			{
				string namedConnectionName = GetNamedConnectionName(tcpSocketInfo.IsServer, tcpSocketInfo.Remote, tcpSocketInfo.ConnectionName);
				lock (_named)
				{
					if (object.ReferenceEquals(_named[namedConnectionName], tcpSocketInfo))
					{
						_named.Remove(namedConnectionName);
					}
				}
				if (tcpSocketInfo.MessageContainer != null)
				{
					tcpSocketInfo.MessageContainer.Dispose(exception);
				}
				if (exception is GenuineExceptions.ChannelClosed || exception is GenuineExceptions.ServerHasBeenRestarted)
				{
					ITransportContext.KnownHosts.ReleaseHostResources(tcpSocketInfo.Remote, exception);
				}
				break;
			}
			case GenuineConnectionType.Invocation:
				lock (tcpSocketInfo)
				{
					switch (tcpSocketInfo.TcpInvocationFiniteAutomatonState)
					{
					case TcpInvocationFiniteAutomatonState.ClientAvailable:
					case TcpInvocationFiniteAutomatonState.ClientSending:
					case TcpInvocationFiniteAutomatonState.ClientReceiving:
						lock (_invocation)
						{
							ArrayList arrayList = (ArrayList)_invocation[tcpSocketInfo.Remote.Url];
							if (arrayList != null)
							{
								lock (arrayList)
								{
									arrayList.Remove(tcpSocketInfo);
								}
							}
						}
						lock (tcpSocketInfo)
						{
							if (tcpSocketInfo.InitialMessage != null)
							{
								if (tcpSocketInfo.InitialMessage.IsResendAfterFail && !tcpSocketInfo.InitialMessage.IsSynchronous)
								{
									Message initialMessage = tcpSocketInfo.InitialMessage;
									tcpSocketInfo.InitialMessage = null;
									Send(initialMessage);
								}
								else
								{
									ITransportContext.IIncomingStreamHandler.DispatchException(tcpSocketInfo.InitialMessage, exception);
									tcpSocketInfo.InitialMessage = null;
								}
							}
						}
						break;
					case TcpInvocationFiniteAutomatonState.ServerAwaiting:
					case TcpInvocationFiniteAutomatonState.ServerExecution:
					case TcpInvocationFiniteAutomatonState.ServerSending:
						lock (_knownInvocationConnections.SyncRoot)
						{
							_knownInvocationConnections.Remove(tcpSocketInfo.ConnectionName);
						}
						break;
					}
				}
				break;
			}
			GenuineThreadPool.QueueUserWorkItem(LowLevel_CloseConnection, tcpSocketInfo, longDuration: false);
		}
		catch (Exception exception3)
		{
			binaryLogWriter?.WriteImplementationWarningEvent("TcpConnectionManager.SocketFailed", LogMessageType.Warning, exception3, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, "An unexpected exception is raised inside TcpConnectionManager.SocketFailed method. Most likely, something must be fixed.");
		}
	}

	public override void StartListening(object endPointAsObject)
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
		string text = endPointAsObject.ToString();
		string text2 = GenuineUtility.SplitToHostAndPort(text, out var port);
		if (text2 == null || port < 0 || port > 65535)
		{
			throw GenuineExceptions.Get_Server_IncorrectAddressToListen(text);
		}
		if (_listeningSockets.ContainsKey(text))
		{
			throw GenuineExceptions.Get_Server_EndPointIsAlreadyBeingListenedTo(text);
		}
		IPEndPoint iPEndPoint = new IPEndPoint(GenuineUtility.ResolveIPAddress(text2), port);
		Socket socket = new Socket(iPEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
		try
		{
			LingerOption optionValue = new LingerOption(enable: true, 3);
			socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, optionValue);
			socket.Bind(iPEndPoint);
			socket.Listen(15);
			AcceptConnectionClosure acceptConnectionClosure = new AcceptConnectionClosure(ITransportContext, text, socket, this);
			lock (_listeningSockets.SyncRoot)
			{
				if (_listeningSockets.ContainsKey(text))
				{
					LowLevel_CloseConnection(socket);
					throw GenuineExceptions.Get_Server_EndPointIsAlreadyBeingListenedTo(text);
				}
				_listeningSockets[text] = acceptConnectionClosure;
			}
			ITransportContext.IGenuineEventProvider.Fire(new GenuineEventArgs(GenuineEventType.GeneralListenerStarted, null, Local, text));
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.AcceptingConnection] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.AcceptingConnection, "TcpConnectionManager.StartListening", LogMessageType.ListeningStarted, null, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, text, null, null, null, "A listening socket has been associated with the \"{0}\" local end point.", text);
			}
			Thread thread = new Thread(acceptConnectionClosure.AcceptConnections);
			thread.IsBackground = true;
			thread.Start();
		}
		catch (Exception exception)
		{
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.AcceptingConnection] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.AcceptingConnection, "TcpConnectionManager.StartListening", LogMessageType.ListeningStarted, exception, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, text, null, null, null, "A listening socket cannot be associated with the \"{0}\" local end point.", text);
			}
			SocketUtility.CloseSocket(socket);
			throw;
		}
		LocalPort = ((IPEndPoint)socket.LocalEndPoint).Port;
	}

	public override void StopListening(object endPointAsObject)
	{
		string text = (string)endPointAsObject;
		if (!_listeningSockets.ContainsKey(text))
		{
			return;
		}
		AcceptConnectionClosure acceptConnectionClosure = (AcceptConnectionClosure)_listeningSockets[text];
		if (LocalPort == ((IPEndPoint)acceptConnectionClosure.Socket.LocalEndPoint).Port)
		{
			lock (_listeningSockets.SyncRoot)
			{
				foreach (object listeningSocket in _listeningSockets)
				{
					DictionaryEntry dictionaryEntry = (DictionaryEntry)listeningSocket;
					if (dictionaryEntry.Key.ToString() == text)
					{
						continue;
					}
					AcceptConnectionClosure acceptConnectionClosure2 = (AcceptConnectionClosure)dictionaryEntry.Value;
					LocalPort = ((IPEndPoint)acceptConnectionClosure2.Socket.LocalEndPoint).Port;
					break;
				}
			}
		}
		acceptConnectionClosure.StopListening.Set();
		SocketUtility.CloseSocket(acceptConnectionClosure.Socket);
		_listeningSockets.Remove(text);
		ITransportContext.IGenuineEventProvider.Fire(new GenuineEventArgs(GenuineEventType.GeneralListenerShutDown, null, Local, text));
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		if (binaryLogWriter != null && binaryLogWriter[LogCategory.AcceptingConnection] > 0)
		{
			binaryLogWriter.WriteEvent(LogCategory.AcceptingConnection, "TcpConnectionManager.StopListening", LogMessageType.ListeningStopped, null, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, text, null, null, null, "A listening socket is no longer associated with the \"{0}\" local end point.", text);
		}
	}

	void IAcceptConnectionConsumer.AcceptConnection(Socket clientSocket)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		string remoteUri = "Unknown.";
		string remoteAddress = "Address of the remote host.";
		TcpSocketInfo tcpSocketInfo = null;
		HostInformation hostInformation = null;
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
		ThreadDataSlotKeeper threadDataSlotKeeper = new ThreadDataSlotKeeper("__GC__EstablishingSocket", clientSocket);
		try
		{
			ConnectionAcceptedCancellableEventParameter connectionAcceptedCancellableEventParameter = new ConnectionAcceptedCancellableEventParameter();
			connectionAcceptedCancellableEventParameter.Socket = clientSocket;
			connectionAcceptedCancellableEventParameter.IPEndPoint = (IPEndPoint)clientSocket.RemoteEndPoint;
			ITransportContext.IGenuineEventProvider.Fire(new GenuineEventArgs(GenuineEventType.GTcpConnectionAccepted, null, null, connectionAcceptedCancellableEventParameter));
			if (connectionAcceptedCancellableEventParameter.Cancel)
			{
				throw GenuineExceptions.Get_Connect_CanNotAcceptIncomingConnection("Connection accepting was cancelled by the event consumer.");
			}
			Exception reasonOfStateLost = null;
			tcpSocketInfo = LowLevel_AcceptConnection(clientSocket, Local.Uri, out remoteUri, out remoteAddress, out var connectionName, out var _, out reasonOfStateLost);
			hostInformation = ITransportContext.KnownHosts[remoteUri];
			if (remoteAddress != null)
			{
				hostInformation.UpdateUrl(remoteAddress);
			}
			tcpSocketInfo.Remote = hostInformation;
			hostInformation.PhysicalAddress = tcpSocketInfo.Socket.RemoteEndPoint;
			hostInformation.LocalPhysicalAddress = tcpSocketInfo.Socket.LocalEndPoint;
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.HostInformation] > 0)
			{
				binaryLogWriter.WriteHostInformationEvent("TcpConnectionManager.LowLevel_AcceptConnection", LogMessageType.HostInformationCreated, null, hostInformation, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, tcpSocketInfo.ConnectionLevelSecurity, (tcpSocketInfo.ConnectionLevelSecurity == null) ? null : tcpSocketInfo.ConnectionLevelSecurity.Name, tcpSocketInfo.DbgConnectionId, "HostInformation is ready for actions.");
			}
			readerAutoLocker = new ReaderAutoLocker(_disposeLock);
			try
			{
				if (base._disposed)
				{
					throw OperationException.WrapException(_disposeReason);
				}
				switch (tcpSocketInfo.GenuineConnectionType)
				{
				case GenuineConnectionType.Persistent:
					lock (hostInformation.PersistentConnectionEstablishingLock)
					{
						if (hostInformation.GenuinePersistentConnectionState == GenuinePersistentConnectionState.Opened)
						{
							throw GenuineExceptions.Get_Processing_LogicError("Remote host tries to open persistent connection while it's a server.");
						}
						hostInformation.GenuinePersistentConnectionState = GenuinePersistentConnectionState.Accepted;
						lock (_persistent.SyncRoot)
						{
							if (_persistent.Get(remoteUri, connectionName) is TcpSocketInfo tcpSocketInfo2)
							{
								lock (tcpSocketInfo2)
								{
									tcpSocketInfo.MessageContainer = tcpSocketInfo2.MessageContainer;
								}
								SocketFailed(GenuineExceptions.Get_Connect_ConnectionReestablished(), tcpSocketInfo2);
								LowLevel_CloseConnection(tcpSocketInfo2);
							}
							else
							{
								tcpSocketInfo.MessageContainer = new MessageContainer(ITransportContext);
							}
							_persistent.Set(remoteUri, connectionName, tcpSocketInfo);
						}
					}
					tcpSocketInfo.CloseConnectionAfterInactivity = GenuineUtility.ConvertToMilliseconds(ITransportContext.IParameterProvider[GenuineParameter.ClosePersistentConnectionAfterInactivity]);
					tcpSocketInfo.Renew();
					tcpSocketInfo.SignalState(GenuineEventType.GeneralConnectionEstablished, null, null);
					if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
					{
						binaryLogWriter.WriteEvent(LogCategory.Connection, "TcpConnectionManager.IAcceptConnectionConsumer.AcceptConnection", LogMessageType.ConnectionEstablished, null, null, tcpSocketInfo.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, tcpSocketInfo.ConnectionLevelSecurity, (tcpSocketInfo.ConnectionLevelSecurity == null) ? null : tcpSocketInfo.ConnectionLevelSecurity.Name, tcpSocketInfo.DbgConnectionId, (int)tcpSocketInfo.GenuineConnectionType, 0, 0, GetType().Name, hostInformation.LocalPhysicalAddress.ToString(), hostInformation.PhysicalAddress.ToString(), null, "The persistent connection is established.");
					}
					if (reasonOfStateLost != null)
					{
						tcpSocketInfo.MessagesSentSynchronously.ReleaseAllMessages();
						tcpSocketInfo.MessageContainer.Dispose(reasonOfStateLost);
						tcpSocketInfo.MessageContainer = new MessageContainer(ITransportContext);
					}
					Pool_StartSending(tcpSocketInfo);
					Pool_InitiateReceiving(tcpSocketInfo, ITransportContext.IParameterProvider);
					break;
				case GenuineConnectionType.Named:
				{
					string key = remoteUri + "/" + tcpSocketInfo.ConnectionName;
					tcpSocketInfo.MessageContainer = new MessageContainer(ITransportContext);
					lock (_named)
					{
						_named[key] = tcpSocketInfo;
					}
					if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
					{
						binaryLogWriter.WriteEvent(LogCategory.Connection, "TcpConnectionManager.IAcceptConnectionConsumer.AcceptConnection", LogMessageType.ConnectionEstablished, null, null, tcpSocketInfo.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, tcpSocketInfo.ConnectionLevelSecurity, (tcpSocketInfo.ConnectionLevelSecurity == null) ? null : tcpSocketInfo.ConnectionLevelSecurity.Name, tcpSocketInfo.DbgConnectionId, (int)tcpSocketInfo.GenuineConnectionType, 0, 0, GetType().Name, hostInformation.LocalPhysicalAddress.ToString(), hostInformation.PhysicalAddress.ToString(), null, "The named connection is established.");
					}
					Pool_StartSending(tcpSocketInfo);
					Pool_InitiateReceiving(tcpSocketInfo, ITransportContext.IParameterProvider);
					tcpSocketInfo.CloseConnectionAfterInactivity = GenuineUtility.ConvertToMilliseconds(ITransportContext.IParameterProvider[GenuineParameter.CloseNamedConnectionAfterInactivity]);
					break;
				}
				case GenuineConnectionType.Invocation:
					tcpSocketInfo.TcpInvocationFiniteAutomatonState = TcpInvocationFiniteAutomatonState.ServerAwaiting;
					_knownInvocationConnections[tcpSocketInfo.ConnectionName] = tcpSocketInfo;
					if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
					{
						binaryLogWriter.WriteEvent(LogCategory.Connection, "TcpConnectionManager.IAcceptConnectionConsumer.AcceptConnection", LogMessageType.ConnectionEstablished, null, null, tcpSocketInfo.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, tcpSocketInfo.ConnectionLevelSecurity, (tcpSocketInfo.ConnectionLevelSecurity == null) ? null : tcpSocketInfo.ConnectionLevelSecurity.Name, tcpSocketInfo.DbgConnectionId, (int)tcpSocketInfo.GenuineConnectionType, 0, 0, GetType().Name, hostInformation.LocalPhysicalAddress.ToString(), hostInformation.PhysicalAddress.ToString(), null, "The invocation connection is established.");
					}
					Pool_InitiateReceiving(tcpSocketInfo, ITransportContext.IParameterProvider);
					tcpSocketInfo.CloseConnectionAfterInactivity = GenuineUtility.ConvertToMilliseconds(ITransportContext.IParameterProvider[GenuineParameter.CloseInvocationConnectionAfterInactivity]);
					break;
				case GenuineConnectionType.Persistent | GenuineConnectionType.Named:
					break;
				}
			}
			finally
			{
				readerAutoLocker.Dispose();
			}
			tcpSocketInfo.Renew();
		}
		catch (Exception exception)
		{
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.AcceptingConnection] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.AcceptingConnection, "TcpConnectionManager.IAcceptConnectionConsumer.AcceptConnection", LogMessageType.ConnectionAccepting, exception, null, hostInformation, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, tcpSocketInfo?.DbgConnectionId ?? (-1), 0, 0, 0, null, null, null, null, "The inbound connection has not been accepted.");
			}
			SocketUtility.CloseSocket(clientSocket);
		}
		finally
		{
			threadDataSlotKeeper.Dispose();
		}
	}

	public bool IsDisposed()
	{
		return base._disposed;
	}

	private void ReleaseConnections_InspectPersistentConnections(object tcpSocketInfoAsObject, object releaseConnections_ParametersAsObject)
	{
		TcpSocketInfo tcpSocketInfo = (TcpSocketInfo)tcpSocketInfoAsObject;
		ReleaseConnections_Parameters releaseConnections_Parameters = (ReleaseConnections_Parameters)releaseConnections_ParametersAsObject;
		if ((releaseConnections_Parameters.HostInformation == null || tcpSocketInfo.Remote == releaseConnections_Parameters.HostInformation) && (releaseConnections_Parameters.GenuineConnectionType & tcpSocketInfo.GenuineConnectionType) != GenuineConnectionType.None)
		{
			releaseConnections_Parameters.FailedConnections.Add(tcpSocketInfo);
		}
	}

	public override void ReleaseConnections(HostInformation hostInformation, GenuineConnectionType genuineConnectionType, Exception reason)
	{
		TcpSocketInfo tcpSocketInfo = null;
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
		{
			binaryLogWriter.WriteEvent(LogCategory.Connection, "TcpConnectionManager.ReleaseConnections", LogMessageType.ReleaseConnections, reason, null, hostInformation, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, Enum.Format(typeof(GenuineConnectionType), genuineConnectionType, "g"), null, null, null, "Connections \"{0}\" are terminated.", Enum.Format(typeof(GenuineConnectionType), genuineConnectionType, "g"), null);
		}
		ArrayList arrayList = new ArrayList();
		ReleaseConnections_Parameters releaseConnections_Parameters = new ReleaseConnections_Parameters();
		releaseConnections_Parameters.FailedConnections = arrayList;
		releaseConnections_Parameters.HostInformation = hostInformation;
		releaseConnections_Parameters.GenuineConnectionType = genuineConnectionType;
		_persistent.InspectAllConnections(_releaseConnections_InspectPersistentConnections, releaseConnections_Parameters);
		if ((genuineConnectionType & GenuineConnectionType.Persistent) != GenuineConnectionType.None || (genuineConnectionType & GenuineConnectionType.Named) != GenuineConnectionType.None)
		{
			lock (_named)
			{
				foreach (object item in _named)
				{
					DictionaryEntry dictionaryEntry = (DictionaryEntry)item;
					tcpSocketInfo = (TcpSocketInfo)dictionaryEntry.Value;
					if ((hostInformation == null || tcpSocketInfo.Remote == hostInformation) && (genuineConnectionType & tcpSocketInfo.GenuineConnectionType) != GenuineConnectionType.None)
					{
						arrayList.Add(tcpSocketInfo);
					}
				}
			}
		}
		if ((genuineConnectionType & GenuineConnectionType.Invocation) != GenuineConnectionType.None)
		{
			lock (_invocation.SyncRoot)
			{
				foreach (object item2 in _invocation)
				{
					DictionaryEntry dictionaryEntry2 = (DictionaryEntry)item2;
					ArrayList arrayList2 = (ArrayList)dictionaryEntry2.Value;
					lock (arrayList2)
					{
						foreach (TcpSocketInfo item3 in arrayList2)
						{
							if (hostInformation == null || item3.Remote == hostInformation)
							{
								arrayList.Add(item3);
							}
						}
					}
				}
			}
			lock (_knownInvocationConnections.SyncRoot)
			{
				foreach (object knownInvocationConnection in _knownInvocationConnections)
				{
					DictionaryEntry dictionaryEntry3 = (DictionaryEntry)knownInvocationConnection;
					tcpSocketInfo = (TcpSocketInfo)dictionaryEntry3.Value;
					if (hostInformation == null || tcpSocketInfo.Remote == hostInformation)
					{
						arrayList.Add(tcpSocketInfo);
					}
				}
			}
		}
		foreach (TcpSocketInfo item4 in arrayList)
		{
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Security] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Connection, "TcpConnectionManager.ReleaseConnections", LogMessageType.ConnectionShuttingDown, reason, null, item4.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, item4.DbgConnectionId, 0, 0, 0, null, null, null, null, "The connection is terminated.");
			}
			SocketFailed(GenuineExceptions.Get_Channel_ConnectionShutDown(reason), item4);
			if (item4.GenuineConnectionType == GenuineConnectionType.Persistent)
			{
				item4.SignalState(GenuineEventType.GeneralConnectionClosed, reason, null);
			}
		}
	}

	public override string[] GetConnectionNames(string uri)
	{
		uri = GenuineUtility.Parse(uri, out var _);
		return _persistent.GetAll(uri);
	}

	public override void InternalDispose(Exception reason)
	{
		ReleaseConnections(null, GenuineConnectionType.All, reason);
		object[] array = null;
		lock (_listeningSockets.SyncRoot)
		{
			array = new object[_listeningSockets.Count];
			_listeningSockets.Keys.CopyTo(array, array.Length);
		}
		object[] array2 = array;
		foreach (object endPoint in array2)
		{
			try
			{
				StopListening(endPoint);
			}
			catch
			{
			}
		}
	}
}
