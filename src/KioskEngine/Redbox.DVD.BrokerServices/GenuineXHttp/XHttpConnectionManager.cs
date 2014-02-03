using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using Belikov.Common.ThreadProcessing;
using Belikov.GenuineChannels.Connection;
using Belikov.GenuineChannels.DotNetRemotingLayer;
using Belikov.GenuineChannels.GenuineHttp;
using Belikov.GenuineChannels.GenuineTcp;
using Belikov.GenuineChannels.Logbook;
using Belikov.GenuineChannels.Messaging;
using Belikov.GenuineChannels.Parameters;
using Belikov.GenuineChannels.Security;
using Belikov.GenuineChannels.TransportContext;
using Belikov.GenuineChannels.Utilities;

namespace Belikov.GenuineChannels.GenuineXHttp;

internal class XHttpConnectionManager : ConnectionManager, ITimerConsumer, IAcceptConnectionConsumer
{
	private class ReleaseConnections_Parameters
	{
		public ArrayList FailedConnections;

		public HostInformation HostInformation;
	}

	private class PhysicalConnectionAndSocket
	{
		public XHttpPhysicalConnection XHttpPhysicalConnection;

		public Socket Socket;

		public PhysicalConnectionAndSocket(XHttpPhysicalConnection xHttpPhysicalConnection, Socket socket)
		{
			Socket = socket;
			XHttpPhysicalConnection = xHttpPhysicalConnection;
		}
	}

	private class Internal_TimerCallback_Parameters
	{
		public ArrayList ExpiredConnections;

		public ArrayList SendPingTo;

		public int SendPingAfter;

		public int Now;
	}

	private AsyncCallback _HalfSync_Server_onEndReceiving;

	private AsyncCallback _HalfSync_Client_onEndReceiving;

	private AsyncCallback _AsyncSending_onEndSending;

	private WaitCallback _HalfSync_Server_onContinueReceiving;

	private WaitCallback _HalfSync_Client_onContinueReceiving;

	private byte[] _unusedBuffer = new byte[1];

	private PersistentConnectionStorage _persistent = new PersistentConnectionStorage();

	private int _xHttpReadHeaderTimeout;

	private PersistentConnectionStorage.ProcessConnectionEventHandler _releaseConnections_InspectPersistentConnections;

	private static byte[] _100ContinueContent = Encoding.ASCII.GetBytes("HTTP/1.1 100 Continue\r\n\r\n");

	public int LocalPort = -1;

	private Hashtable _listeningSockets = Hashtable.Synchronized(new Hashtable());

	private WaitCallback _internal_TimerCallback;

	private PersistentConnectionStorage.ProcessConnectionEventHandler _internal_TimerCallback_InspectPersistentConnections;

	public XHttpConnectionManager(ITransportContext iTransportContext)
		: base(iTransportContext)
	{
		_HalfSync_Client_onEndReceiving = LowLevel_HalfSync_Client_EndReceiving;
		_HalfSync_Server_onEndReceiving = LowLevel_HalfSync_Server_EndReceiving;
		_AsyncSending_onEndSending = LowLevel_OnEndAsyncSending;
		_internal_TimerCallback = Internal_TimerCallback;
		_HalfSync_Client_onContinueReceiving = Pool_Client_ContinueHalfSyncReceiving;
		_HalfSync_Server_onContinueReceiving = Pool_Server_ContinueHalfSyncReceiving;
		_releaseConnections_InspectPersistentConnections = ReleaseConnections_InspectPersistentConnections;
		_internal_TimerCallback_InspectPersistentConnections = Internal_TimerCallback_InspectPersistentConnections;
		Local = new HostInformation("_ghttp://" + iTransportContext.HostIdentifier, iTransportContext);
		TimerProvider.Attach(this);
		_xHttpReadHeaderTimeout = GenuineUtility.ConvertToMilliseconds(iTransportContext.IParameterProvider[GenuineParameter.XHttpReadHttpMessageTimeout]);
	}

	protected override void InternalSend(Message message)
	{
		XHttpConnection xHttpConnection = Pool_GetConnectionForSending(message);
		XHttpPhysicalConnection xHttpPhysicalConnection = null;
		try
		{
			xHttpPhysicalConnection = ((!xHttpConnection.IsClient) ? xHttpConnection.Listener : xHttpConnection.Sender);
			lock (xHttpPhysicalConnection.PhysicalConnectionStateLock)
			{
				if (xHttpPhysicalConnection.AcquireIfAvailable())
				{
					LowLevel_SendHttpContent(GenuineUtility.GetTimeout(xHttpConnection.CloseConnectionAfterInactivity), message, null, null, xHttpConnection.MessageContainer, xHttpPhysicalConnection, xHttpConnection.GenuineConnectionType, HttpPacketType.Usual, repeatSending: false, message.IsSynchronous, startAutomaticReceiving: true, applyClss: true);
				}
				else
				{
					xHttpConnection.MessageContainer.AddMessage(message, onlyCheckIn: false);
				}
			}
		}
		catch (Exception exception)
		{
			ConnectionFailed(exception, xHttpConnection, xHttpPhysicalConnection);
		}
	}

	private void Pool_Client_ContinueHalfSyncReceiving(object xHttpPhysicalConnectionAsObject)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		XHttpPhysicalConnection xHttpPhysicalConnection = (XHttpPhysicalConnection)xHttpPhysicalConnectionAsObject;
		try
		{
			Stream stream = LowLevel_ParseHttpContent(GenuineUtility.GetTimeout(_xHttpReadHeaderTimeout), client: true, xHttpPhysicalConnection.Socket, xHttpPhysicalConnection.ConnectionLevelSecurity, out var _, out var uri, out var sequenceNo, out var _, out var _, out var httpPacketType, out var _, out var remoteHostUniqueIdentifier);
			xHttpPhysicalConnection.XHttpConnection.Renew();
			xHttpPhysicalConnection.XHttpConnection.SignalState(GenuineEventType.GeneralConnectionEstablished, null, null);
			if (sequenceNo != xHttpPhysicalConnection.SequenceNo)
			{
				throw GenuineExceptions.Get_Channel_Desynchronization();
			}
			xHttpPhysicalConnection.Remote.UpdateUri(uri, remoteHostUniqueIdentifier);
			switch (httpPacketType)
			{
			case HttpPacketType.Desynchronization:
			case HttpPacketType.ClosedManually:
				throw GenuineExceptions.Get_Receive_ConnectionClosed();
			case HttpPacketType.SenderError:
				throw GenuineExceptions.Get_Receive_ConnectionClosed(LowLevel_ParseException(stream).Message);
			}
			if (!xHttpPhysicalConnection.IsSender && httpPacketType != HttpPacketType.ListenerTimedOut)
			{
				LowLevel_ParseLabelledStream(stream, xHttpPhysicalConnection);
			}
			stream.Close();
			Pool_HandleClientConnection(xHttpPhysicalConnection);
		}
		catch (Exception exception)
		{
			ConnectionFailed(exception, xHttpPhysicalConnection.XHttpConnection, xHttpPhysicalConnection);
		}
	}

	private void Pool_Server_ContinueHalfSyncReceiving(object socketAsObject)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		XHttpConnection xHttpConnection = null;
		XHttpPhysicalConnection xHttpPhysicalConnection = null;
		Socket socket = (Socket)socketAsObject;
		string uri = null;
		int sequenceNo = 0;
		GenuineConnectionType genuineConnectionType = GenuineConnectionType.None;
		Guid hostId = Guid.Empty;
		HttpPacketType httpPacketType = HttpPacketType.Unkown;
		try
		{
			Stream stream = LowLevel_ParseHttpContent(GenuineUtility.GetTimeout(GenuineUtility.ConvertToMilliseconds(ITransportContext.IParameterProvider[GenuineParameter.XHttpReadHttpMessageTimeout])), client: false, socket, null, out var protocolVersion, out uri, out sequenceNo, out genuineConnectionType, out hostId, out httpPacketType, out var connectionName, out var remoteHostUniqueIdentifier);
			HostInformation hostInformation = ITransportContext.KnownHosts["_ghttp://" + hostId.ToString("N")];
			hostInformation.UpdateUri(hostInformation.Uri, remoteHostUniqueIdentifier);
			hostInformation.ProtocolVersion = protocolVersion;
			lock (hostInformation.PersistentConnectionEstablishingLock)
			{
				xHttpConnection = _persistent.Get(hostInformation.Uri, connectionName) as XHttpConnection;
				if (httpPacketType == HttpPacketType.Establishing_ResetConnection && xHttpConnection != null)
				{
					xHttpConnection.Dispose(GenuineExceptions.Get_Receive_ConnectionClosed("The connection was closed by HttpPacketType.Establishing_ResetConnection client request."));
					xHttpConnection = null;
				}
				if (xHttpConnection == null)
				{
					ReaderAutoLocker readerAutoLocker = new ReaderAutoLocker(_disposeLock);
					try
					{
						if (base._disposed)
						{
							throw OperationException.WrapException(_disposeReason);
						}
						ConnectionAcceptedCancellableEventParameter connectionAcceptedCancellableEventParameter = new ConnectionAcceptedCancellableEventParameter();
						connectionAcceptedCancellableEventParameter.Socket = socket;
						connectionAcceptedCancellableEventParameter.IPEndPoint = (IPEndPoint)socket.RemoteEndPoint;
						ITransportContext.IGenuineEventProvider.Fire(new GenuineEventArgs(GenuineEventType.GHttpConnectionAccepted, null, null, connectionAcceptedCancellableEventParameter));
						if (connectionAcceptedCancellableEventParameter.Cancel)
						{
							throw GenuineExceptions.Get_Connect_CanNotAcceptIncomingConnection("Connection accepting was cancelled by the event consumer.");
						}
						xHttpConnection = Pool_CreateConnection(hostInformation, hostInformation.Uri, isClient: false, connectionName);
					}
					finally
					{
						readerAutoLocker.Dispose();
					}
				}
			}
			bool flag;
			lock (xHttpConnection.Listener.PhysicalConnectionStateLock)
			{
				if (httpPacketType == HttpPacketType.Listening)
				{
					if (xHttpConnection.Listener.ConnectionAvailable && xHttpConnection.Listener.Socket != socket)
					{
						LowLevel_SendServerError(new PhysicalConnectionAndSocket(xHttpConnection.Listener, socket));
						xHttpConnection.Listener.AcquireIfAvailable();
					}
					xHttpConnection.Listener.Socket = socket;
					xHttpPhysicalConnection = xHttpConnection.Listener;
				}
				else
				{
					xHttpConnection.Sender.Socket = socket;
					xHttpPhysicalConnection = xHttpConnection.Sender;
				}
				flag = xHttpPhysicalConnection.SequenceNo == sequenceNo && xHttpPhysicalConnection.SentContent != null;
				xHttpPhysicalConnection.Remote.PhysicalAddress = socket.RemoteEndPoint;
				xHttpPhysicalConnection.Remote.LocalPhysicalAddress = socket.LocalEndPoint;
			}
			xHttpConnection.Renew();
			xHttpPhysicalConnection.XHttpConnection.SignalState(GenuineEventType.GeneralConnectionEstablished, null, null);
			if (flag)
			{
				stream.Close();
				LowLevel_SendHttpContent(hostInformation.ExpireTime, null, null, null, null, xHttpPhysicalConnection, genuineConnectionType, HttpPacketType.RequestRepeated, repeatSending: true, synchronous: true, startAutomaticReceiving: true, applyClss: true);
				return;
			}
			lock (xHttpConnection.Listener.PhysicalConnectionStateLock)
			{
				if ((httpPacketType != HttpPacketType.Establishing_ResetConnection && xHttpPhysicalConnection.SequenceNo > sequenceNo) || sequenceNo > xHttpPhysicalConnection.SequenceNo + 1)
				{
					if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
					{
						binaryLogWriter.WriteEvent(LogCategory.Connection, "XHttpConnectionManager.Pool_Server_ContinueHalfSyncreceiving", LogMessageType.ConnectionStreamDesynchronization, GenuineExceptions.Get_Debugging_GeneralWarning("Stream desynchronization."), null, xHttpPhysicalConnection.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, xHttpPhysicalConnection.ConnectionLevelSecurity, (xHttpPhysicalConnection.ConnectionLevelSecurity == null) ? null : xHttpPhysicalConnection.ConnectionLevelSecurity.Name, xHttpConnection.DbgConnectionId, 0, 0, 0, null, null, null, null, "Stream desynchronization error. Received sequence number: {0}. Expected sequence number: {1}.", xHttpPhysicalConnection.SequenceNo, sequenceNo);
					}
					LowLevel_SendHttpContent(hostInformation.ExpireTime, null, null, null, null, xHttpPhysicalConnection, genuineConnectionType, HttpPacketType.Desynchronization, repeatSending: false, synchronous: true, startAutomaticReceiving: true, applyClss: true);
					return;
				}
				if (xHttpPhysicalConnection.SentContent != null)
				{
					xHttpPhysicalConnection.SentContent.Close();
					xHttpPhysicalConnection.SentContent = null;
				}
				xHttpPhysicalConnection.SequenceNo = sequenceNo;
				xHttpPhysicalConnection.Socket = socket;
				xHttpPhysicalConnection.MessagesBeingSent.Clear();
			}
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Connection, "XHttpConnectionManager.Pool_Server_ContinueHalfSyncreceiving", LogMessageType.ReceivingFinished, null, null, xHttpPhysicalConnection.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, xHttpPhysicalConnection.ConnectionLevelSecurity, (xHttpPhysicalConnection.ConnectionLevelSecurity == null) ? null : xHttpPhysicalConnection.ConnectionLevelSecurity.Name, xHttpConnection.DbgConnectionId, 0, 0, 0, null, null, null, null, "HTTP Stream received. Packet type: {0}.", Enum.Format(typeof(HttpPacketType), httpPacketType, "g"));
			}
			switch (httpPacketType)
			{
			case HttpPacketType.Establishing_ResetConnection:
			case HttpPacketType.Establishing:
			{
				lock (hostInformation.PersistentConnectionEstablishingLock)
				{
					hostInformation.GenuinePersistentConnectionState = GenuinePersistentConnectionState.Accepted;
				}
				BinaryReader binaryReader = new BinaryReader(stream);
				Stream stream2 = null;
				GenuineChunkedStream genuineChunkedStream = new GenuineChunkedStream(releaseOnReadMode: false);
				int num = binaryReader.ReadInt32();
				GenuineChunkedStreamSizeLabel genuineChunkedStreamSizeLabel = new GenuineChunkedStreamSizeLabel(genuineChunkedStream);
				try
				{
					if (xHttpConnection.Sender.ConnectionLevelSecurity != null && !xHttpConnection.Sender.ConnectionLevelSecurity.IsEstablished)
					{
						using Stream input = new DelimiterStream(stream, num);
						stream2 = xHttpConnection.Sender.ConnectionLevelSecurity.EstablishSession(input, connectionLevel: true);
					}
					if (stream2 != null)
					{
						GenuineUtility.CopyStreamToStream(stream2, genuineChunkedStream);
					}
				}
				finally
				{
					genuineChunkedStreamSizeLabel.Dispose();
				}
				num = binaryReader.ReadInt32();
				genuineChunkedStreamSizeLabel = new GenuineChunkedStreamSizeLabel(genuineChunkedStream);
				try
				{
					if (xHttpConnection.Listener.ConnectionLevelSecurity != null && !xHttpConnection.Listener.ConnectionLevelSecurity.IsEstablished)
					{
						using Stream input2 = new DelimiterStream(stream, num);
						stream2 = xHttpConnection.Listener.ConnectionLevelSecurity.EstablishSession(input2, connectionLevel: true);
					}
					if (stream2 != null)
					{
						GenuineUtility.CopyStreamToStream(stream2, genuineChunkedStream);
					}
				}
				finally
				{
					genuineChunkedStreamSizeLabel.Dispose();
				}
				stream.Close();
				LowLevel_SendHttpContent(hostInformation.ExpireTime, null, genuineChunkedStream, null, null, xHttpPhysicalConnection, genuineConnectionType, httpPacketType, repeatSending: false, synchronous: true, startAutomaticReceiving: true, applyClss: false);
				break;
			}
			case HttpPacketType.Listening:
				stream.Close();
				Pool_Server_ProcessListenerRequest(xHttpConnection, xHttpPhysicalConnection);
				break;
			case HttpPacketType.Usual:
				if (xHttpPhysicalConnection.ConnectionLevelSecurity != null)
				{
					stream = xHttpPhysicalConnection.ConnectionLevelSecurity.Decrypt(stream);
				}
				Pool_Server_ProcessSenderRequest(xHttpConnection, xHttpPhysicalConnection, stream);
				break;
			default:
				throw GenuineExceptions.Get_Receive_IncorrectData("Unexpected type of the packet.");
			}
		}
		catch (Exception exception)
		{
			try
			{
				if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
				{
					binaryLogWriter.WriteEvent(LogCategory.Connection, "XHttpConnectionManager.Pool_Server_ContinueHalfSyncreceiving", LogMessageType.ReceivingFinished, exception, null, xHttpPhysicalConnection.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, xHttpPhysicalConnection.ConnectionLevelSecurity, (xHttpPhysicalConnection.ConnectionLevelSecurity == null) ? null : xHttpPhysicalConnection.ConnectionLevelSecurity.Name, xHttpConnection.DbgConnectionId, 0, 0, 0, null, null, null, null, "Error occurred while receiving HTTP Stream. Sequence No: {0}. Connection type: {1}. Packet type: {2}.", sequenceNo, genuineConnectionType, httpPacketType);
				}
				SocketUtility.CloseSocket(socket);
			}
			catch
			{
			}
		}
	}

	private void Pool_Server_ProcessListenerRequest(XHttpConnection xHttpConnection, XHttpPhysicalConnection xHttpPhysicalConnection)
	{
		lock (xHttpConnection.Listener.PhysicalConnectionStateLock)
		{
			try
			{
				Message message = xHttpConnection.MessageContainer.GetMessage();
				if (message == null)
				{
					xHttpPhysicalConnection.MarkAsAvailable();
				}
				else
				{
					LowLevel_SendHttpContent(GenuineUtility.GetTimeout(xHttpConnection.CloseConnectionAfterInactivity), message, null, null, xHttpConnection.MessageContainer, xHttpPhysicalConnection, xHttpConnection.GenuineConnectionType, HttpPacketType.Usual, repeatSending: false, synchronous: true, startAutomaticReceiving: true, applyClss: true);
				}
			}
			catch (Exception exception)
			{
				BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
				if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
				{
					binaryLogWriter.WriteEvent(LogCategory.Connection, "XHttpConnectionManager.Pool_Server_ProcessListenerRequest", LogMessageType.AsynchronousSendingStarted, exception, null, xHttpPhysicalConnection.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, xHttpConnection.DbgConnectionId, 0, 0, 0, null, null, null, null, "Error occurred while sending HTTP Listener Request. Sequence No: {0}.", xHttpPhysicalConnection.SequenceNo);
				}
			}
		}
	}

	private void Pool_Server_ProcessSenderRequest(XHttpConnection xHttpConnection, XHttpPhysicalConnection xHttpPhysicalConnection, Stream inputStream)
	{
		Exception ex = null;
		try
		{
			LowLevel_ParseLabelledStream(inputStream, xHttpPhysicalConnection);
			inputStream.Close();
		}
		catch (Exception ex2)
		{
			ex = ex2;
			BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Connection, "XHttpConnectionManager.Pool_Server_ProcessSenderRequest", LogMessageType.ReceivingFinished, ex2, null, xHttpConnection.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, xHttpConnection.DbgConnectionId, 0, 0, 0, null, null, null, null, "Error occurred while parsing HTTP Sender Request. Sequence No: {0}.", xHttpPhysicalConnection.SequenceNo);
			}
		}
		if (ex != null)
		{
			ex = OperationException.WrapException(ex);
			GenuineChunkedStream serializationStream = new GenuineChunkedStream(releaseOnReadMode: false);
			BinaryFormatter binaryFormatter = new BinaryFormatter(new RemotingSurrogateSelector(), new StreamingContext(StreamingContextStates.Other));
			binaryFormatter.Serialize(serializationStream, ex);
			LowLevel_SendHttpContent(GenuineUtility.GetTimeout(xHttpConnection.CloseConnectionAfterInactivity), null, null, ex, null, xHttpPhysicalConnection, GenuineConnectionType.Persistent, HttpPacketType.SenderError, repeatSending: false, synchronous: true, startAutomaticReceiving: true, applyClss: false);
		}
		else
		{
			LowLevel_SendHttpContent(GenuineUtility.GetTimeout(xHttpConnection.CloseConnectionAfterInactivity), null, null, null, null, xHttpPhysicalConnection, GenuineConnectionType.Persistent, HttpPacketType.SenderResponse, repeatSending: false, synchronous: true, startAutomaticReceiving: true, applyClss: false);
		}
	}

	private void Pool_HandleClientConnection(XHttpPhysicalConnection xHttpPhysicalConnection)
	{
		if (xHttpPhysicalConnection.IsSender)
		{
			Pool_Sender_HandleClientConnection(xHttpPhysicalConnection);
		}
		else
		{
			Pool_Listener_HandleClientConnection(xHttpPhysicalConnection);
		}
	}

	private void Pool_Sender_HandleClientConnection(XHttpPhysicalConnection xHttpPhysicalConnection)
	{
		lock (xHttpPhysicalConnection.PhysicalConnectionStateLock)
		{
			Message message = xHttpPhysicalConnection.XHttpConnection.MessageContainer.GetMessage();
			if (message == null)
			{
				xHttpPhysicalConnection.MarkAsAvailable();
			}
			else
			{
				LowLevel_SendHttpContent(GenuineUtility.GetTimeout(xHttpPhysicalConnection.XHttpConnection.CloseConnectionAfterInactivity), message, null, null, xHttpPhysicalConnection.XHttpConnection.MessageContainer, xHttpPhysicalConnection, GenuineConnectionType.Persistent, HttpPacketType.Usual, repeatSending: false, synchronous: true, startAutomaticReceiving: true, applyClss: true);
			}
		}
	}

	private void Pool_Listener_HandleClientConnection(XHttpPhysicalConnection xHttpPhysicalConnection)
	{
		LowLevel_SendHttpContent(GenuineUtility.GetTimeout(xHttpPhysicalConnection.XHttpConnection.CloseConnectionAfterInactivity), null, null, null, null, xHttpPhysicalConnection, GenuineConnectionType.Persistent, HttpPacketType.Listening, repeatSending: false, synchronous: true, startAutomaticReceiving: true, applyClss: true);
	}

	private XHttpConnection Pool_GetConnectionForSending(Message message)
	{
		XHttpConnection xHttpConnection = null;
		bool flag = false;
		string text = null;
		ReaderAutoLocker readerAutoLocker = new ReaderAutoLocker(_disposeLock);
		try
		{
			if (base._disposed)
			{
				throw OperationException.WrapException(_disposeReason);
			}
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
						xHttpConnection = _persistent.Get(text, message.ConnectionName) as XHttpConnection;
						if (xHttpConnection == null)
						{
							xHttpConnection = Pool_EstablishPersistentConnection(message.Recipient, message.ConnectionName);
							xHttpConnection.MessageContainer = new MessageContainer(ITransportContext);
							_persistent.Set(text, xHttpConnection.ConnectionName, xHttpConnection);
							xHttpConnection.CloseConnectionAfterInactivity = GenuineUtility.ConvertToMilliseconds(ITransportContext.IParameterProvider[GenuineParameter.ClosePersistentConnectionAfterInactivity]);
							xHttpConnection.Renew();
							xHttpConnection.SignalState(GenuineEventType.GeneralConnectionEstablished, null, null);
						}
					}
					else
					{
						lock (_persistent)
						{
							xHttpConnection = _persistent.Get(text, message.ConnectionName) as XHttpConnection;
						}
						if (xHttpConnection == null)
						{
							throw GenuineExceptions.Get_Send_DestinationIsUnreachable(message.Recipient.Uri);
						}
					}
				}
				break;
			}
		}
		finally
		{
			readerAutoLocker.Dispose();
		}
		return xHttpConnection;
	}

	private void ReleaseConnections_InspectPersistentConnections(object xHttpConnectionAsObject, object releaseConnections_ParametersAsObject)
	{
		XHttpConnection xHttpConnection = (XHttpConnection)xHttpConnectionAsObject;
		ReleaseConnections_Parameters releaseConnections_Parameters = (ReleaseConnections_Parameters)releaseConnections_ParametersAsObject;
		if (releaseConnections_Parameters.HostInformation == null || xHttpConnection.Remote == releaseConnections_Parameters.HostInformation)
		{
			releaseConnections_Parameters.FailedConnections.Add(xHttpConnection);
		}
	}

	public override void ReleaseConnections(HostInformation hostInformation, GenuineConnectionType genuineConnectionType, Exception reason)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		ArrayList arrayList = new ArrayList();
		if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
		{
			binaryLogWriter.WriteEvent(LogCategory.Connection, "XHttpConnectionManager.ReleaseConnections", LogMessageType.ReleaseConnections, reason, null, hostInformation, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, Enum.Format(typeof(GenuineConnectionType), genuineConnectionType, "g"), null, null, null, "Connections \"{0}\" are terminated.", Enum.Format(typeof(GenuineConnectionType), genuineConnectionType, "g"), null);
		}
		WriterAutoLocker writerAutoLocker = new WriterAutoLocker(_disposeLock);
		try
		{
			if ((genuineConnectionType & GenuineConnectionType.Persistent) != GenuineConnectionType.None)
			{
				ReleaseConnections_Parameters releaseConnections_Parameters = new ReleaseConnections_Parameters();
				releaseConnections_Parameters.FailedConnections = arrayList;
				releaseConnections_Parameters.HostInformation = hostInformation;
				_persistent.InspectAllConnections(_releaseConnections_InspectPersistentConnections, releaseConnections_Parameters);
			}
		}
		finally
		{
			writerAutoLocker.Dispose();
		}
		foreach (XHttpConnection item in arrayList)
		{
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Connection, "XHttpConnectionManager.ReleaseConnections", LogMessageType.ConnectionShuttingDown, reason, null, hostInformation, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, item.DbgConnectionId, 0, 0, 0, Enum.Format(typeof(GenuineConnectionType), genuineConnectionType, "g"), null, null, null, "Connection is terminated.");
			}
			ConnectionFailed(GenuineExceptions.Get_Channel_ConnectionShutDown(reason), item, null);
			if (item.GenuineConnectionType == GenuineConnectionType.Persistent)
			{
				item.SignalState(GenuineEventType.GeneralConnectionClosed, reason, null);
			}
		}
	}

	public override string[] GetConnectionNames(string uri)
	{
		uri = GenuineUtility.Parse(uri, out var _);
		return _persistent.GetAll(uri);
	}

	private void ConnectionFailed(Exception exception, XHttpConnection xHttpConnection, XHttpPhysicalConnection xHttpPhysicalConnection)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		try
		{
			if (xHttpPhysicalConnection == null && xHttpConnection == null)
			{
				binaryLogWriter?.WriteImplementationWarningEvent("XHttpConnectionManager.ConnectionFailed", LogMessageType.ConnectionFailed, exception, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, "The connection has not been specified while invoking XHttpConnectionManager.ConnectionFailed.");
				return;
			}
			OperationException ex = exception as OperationException;
			bool flag = false;
			if (ex != null)
			{
				flag = ex.OperationErrorMessage.ErrorIdentifier.IndexOf("ConflictOfConnections") >= 0;
			}
			bool flag2 = !ConnectionManager.IsExceptionCritical(exception as OperationException);
			HostInformation hostInformation = null;
			if (xHttpConnection != null)
			{
				hostInformation = xHttpConnection.Remote;
			}
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Connection, "XHttpConnectionManager.ConnectionFailed", LogMessageType.ConnectionFailed, exception, null, hostInformation, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, xHttpConnection?.DbgConnectionId ?? (-1), 0, 0, 0, null, null, null, null, "XHTTP connection has failed.");
			}
			ReaderAutoLocker readerAutoLocker = new ReaderAutoLocker(_disposeLock);
			try
			{
				if (base._disposed)
				{
					flag2 = false;
				}
			}
			finally
			{
				readerAutoLocker.Dispose();
			}
			if (xHttpPhysicalConnection != null)
			{
				GenuineThreadPool.QueueUserWorkItem(SocketUtility.CloseSocket, xHttpPhysicalConnection.Socket, longDuration: true);
			}
			switch (xHttpConnection.GenuineConnectionType)
			{
			case GenuineConnectionType.Persistent:
				if (flag)
				{
					break;
				}
				lock (hostInformation.PersistentConnectionEstablishingLock)
				{
					readerAutoLocker = new ReaderAutoLocker(_disposeLock);
					try
					{
						if (!flag2 || xHttpConnection.IsDisposed)
						{
							_persistent.Remove(hostInformation.PrimaryUri, xHttpConnection.ConnectionName);
							ITransportContext.KnownHosts.ReleaseHostResources(xHttpConnection.Remote, exception);
							xHttpConnection.Dispose(exception);
							xHttpConnection.SignalState(GenuineEventType.GeneralConnectionClosed, exception, null);
							if (exception is OperationException && ((OperationException)exception).OperationErrorMessage.ErrorIdentifier == "GenuineChannels.Exception.Receive.ServerHasBeenRestarted")
							{
								xHttpConnection.SignalState(GenuineEventType.GeneralServerRestartDetected, exception, null);
							}
						}
						else
						{
							xHttpConnection.SignalState(GenuineEventType.GeneralConnectionReestablishing, exception, null);
							if (hostInformation.GenuinePersistentConnectionState == GenuinePersistentConnectionState.Opened && xHttpPhysicalConnection != null && xHttpPhysicalConnection.Reestablish_ObtainStatus())
							{
								GenuineThreadPool.QueueUserWorkItem(ReestablishConnection, xHttpPhysicalConnection, longDuration: true);
							}
						}
						break;
					}
					finally
					{
						readerAutoLocker.Dispose();
					}
				}
			}
		}
		catch (Exception exception2)
		{
			binaryLogWriter?.WriteImplementationWarningEvent("XHttpConnectionManager.ConnectionFailed", LogMessageType.Warning, exception2, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, "An unexpected exception is raised inside XHttpConnectionManager.ConnectionFailed method. Most likely, something must be fixed.");
		}
	}

	private void ReestablishConnection(object xHttpPhysicalConnectionAsObject)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		XHttpPhysicalConnection xHttpPhysicalConnection = (XHttpPhysicalConnection)xHttpPhysicalConnectionAsObject;
		HostInformation remote = xHttpPhysicalConnection.Remote;
		binaryLogWriter?.WriteImplementationWarningEvent("XHttpConnectionManager.ReestablishConnection", LogMessageType.Warning, GenuineExceptions.Get_Debugging_GeneralWarning("The connection is available during ReestablishConnection invocation."), GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, "The connection is available during ReestablishConnection invocation.");
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
				binaryLogWriter.WriteEvent(LogCategory.Connection, "XHttpConnectionManager.ReestablishConnection", LogMessageType.ConnectionReestablishing, null, null, xHttpPhysicalConnection.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, xHttpPhysicalConnection.XHttpConnection.DbgConnectionId, 0, 0, 0, null, null, null, null, "Trying to reconnect to {0}.", remote.Url);
			}
			remote.Renew(GenuineUtility.ConvertToMilliseconds(ITransportContext.IParameterProvider[GenuineParameter.MaxTimeSpanToReconnect]), canMakeShorter: false);
			int expireTime = remote.ExpireTime;
			int num = 0;
			while (true)
			{
				Thread.Sleep((TimeSpan)ITransportContext.IParameterProvider[GenuineParameter.SleepBetweenReconnections]);
				if (num > (int)ITransportContext.IParameterProvider[GenuineParameter.ReconnectionTries] || GenuineUtility.IsTimeoutExpired(expireTime))
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
				if (xHttpPhysicalConnection.XHttpConnection.IsDisposed)
				{
					return;
				}
				try
				{
					xHttpPhysicalConnection.CheckConnectionStatus();
					bool flag;
					lock (xHttpPhysicalConnection.PhysicalConnectionStateLock)
					{
						flag = xHttpPhysicalConnection.SentContent != null;
					}
					if (flag)
					{
						LowLevel_SendHttpContent(expireTime, null, null, null, xHttpPhysicalConnection.XHttpConnection.MessageContainer, xHttpPhysicalConnection, GenuineConnectionType.Persistent, HttpPacketType.Unkown, repeatSending: true, synchronous: true, startAutomaticReceiving: true, applyClss: true);
					}
					else
					{
						Pool_HandleClientConnection(xHttpPhysicalConnection);
					}
				}
				catch (Exception exception)
				{
					if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
					{
						binaryLogWriter.WriteEvent(LogCategory.Connection, "XHttpConnectionManager.ReestablishConnection", LogMessageType.ConnectionReestablishing, exception, null, remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "Reconnection failed. The current try: {0}. Milliseconds remained: {1}.", num, GenuineUtility.CompareTickCounts(expireTime, GenuineUtility.TickCount));
					}
					goto IL_02db;
				}
				break;
				IL_02db:
				num++;
			}
			readerAutoLocker = new ReaderAutoLocker(_disposeLock);
			try
			{
				if (base._disposed)
				{
					throw OperationException.WrapException(_disposeReason);
				}
				xHttpPhysicalConnection.Reestablish_ResetStatus();
				xHttpPhysicalConnection.XHttpConnection.Renew();
				xHttpPhysicalConnection.MarkAsAvailable();
				xHttpPhysicalConnection.XHttpConnection.SignalState(GenuineEventType.GeneralConnectionEstablished, null, null);
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
				binaryLogWriter.WriteEvent(LogCategory.Connection, "XHttpConnectionManager.ReestablishConnection", LogMessageType.ConnectionReestablished, ex, null, remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "The connection cannot be reestablished.");
			}
			ConnectionFailed(GenuineExceptions.Get_Channel_ConnectionShutDown(ex), xHttpPhysicalConnection.XHttpConnection, xHttpPhysicalConnection);
		}
	}

	private void LowLevel_Send100Continue(Socket socket)
	{
		socket.Send(_100ContinueContent, 0, _100ContinueContent.Length, SocketFlags.None);
	}

	private void LowLevel_SendHttpContent(int timeout, Message message, GenuineChunkedStream content, Exception exceptionToBeSent, MessageContainer messageContainer, XHttpPhysicalConnection xHttpPhysicalConnection, GenuineConnectionType genuineConnectionType, HttpPacketType httpPacketType, bool repeatSending, bool synchronous, bool startAutomaticReceiving, bool applyClss)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		xHttpPhysicalConnection.CheckConnectionStatus();
		if (!xHttpPhysicalConnection.IsSender)
		{
			xHttpPhysicalConnection.XHttpConnection.LastTimeContentWasSent = GenuineUtility.TickCount;
		}
		Socket socket;
		Stream sentContent;
		lock (xHttpPhysicalConnection.PhysicalConnectionStateLock)
		{
			if (!repeatSending)
			{
				if (xHttpPhysicalConnection.XHttpConnection.IsClient)
				{
					xHttpPhysicalConnection.SequenceNo++;
				}
				if (message != null || (message == null && content == null && exceptionToBeSent == null))
				{
					GenuineChunkedStream genuineChunkedStream = new GenuineChunkedStream(releaseOnReadMode: false);
					MessageCoder.FillInLabelledStream(message, messageContainer, xHttpPhysicalConnection.MessagesBeingSent, genuineChunkedStream, xHttpPhysicalConnection.AsyncSendBuffer, (int)ITransportContext.IParameterProvider[GenuineParameter.HttpRecommendedPacketSize]);
					if (binaryLogWriter != null && binaryLogWriter[LogCategory.Transport] > 0)
					{
						for (int i = 0; i < xHttpPhysicalConnection.MessagesBeingSent.Count; i++)
						{
							Message message2 = (Message)xHttpPhysicalConnection.MessagesBeingSent[i];
							binaryLogWriter.WriteEvent(LogCategory.Transport, "XHttpConnectionManager.LowLevel_SendHttpContent", LogMessageType.MessageIsSentAsynchronously, null, message2, xHttpPhysicalConnection.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, xHttpPhysicalConnection.ConnectionLevelSecurity, null, xHttpPhysicalConnection.XHttpConnection.DbgConnectionId, xHttpPhysicalConnection.SequenceNo, 0, 0, null, null, null, null, "The message will be sent in the {0} stream N: {1}.", (xHttpPhysicalConnection == xHttpPhysicalConnection.XHttpConnection.Sender) ? "SENDER" : "LISTENER", xHttpPhysicalConnection.SequenceNo);
						}
					}
					xHttpPhysicalConnection.SentContent = genuineChunkedStream;
				}
				else if (content != null)
				{
					xHttpPhysicalConnection.MessagesBeingSent.Clear();
					xHttpPhysicalConnection.SentContent = content;
				}
				else
				{
					Exception graph = OperationException.WrapException(exceptionToBeSent);
					GenuineChunkedStream genuineChunkedStream2 = new GenuineChunkedStream(releaseOnReadMode: false);
					BinaryFormatter binaryFormatter = new BinaryFormatter(new RemotingSurrogateSelector(), new StreamingContext(StreamingContextStates.Other));
					binaryFormatter.Serialize(genuineChunkedStream2, graph);
					xHttpPhysicalConnection.MessagesBeingSent.Clear();
					xHttpPhysicalConnection.SentContent = genuineChunkedStream2;
				}
				if (applyClss && xHttpPhysicalConnection.XHttpConnection.IsClient && xHttpPhysicalConnection.ConnectionLevelSecurity != null)
				{
					GenuineChunkedStream genuineChunkedStream3 = new GenuineChunkedStream(releaseOnReadMode: false);
					xHttpPhysicalConnection.ConnectionLevelSecurity.Encrypt(xHttpPhysicalConnection.SentContent, genuineChunkedStream3);
					xHttpPhysicalConnection.SentContent = genuineChunkedStream3;
				}
				GenuineChunkedStream genuineChunkedStream4 = new GenuineChunkedStream(releaseOnReadMode: false);
				BinaryWriter binaryWriter = new BinaryWriter(genuineChunkedStream4);
				if (xHttpPhysicalConnection.XHttpConnection.IsClient)
				{
					HttpMessageCoder.WriteRequestHeader(binaryWriter, 3, genuineConnectionType, xHttpPhysicalConnection.XHttpConnection.HostId, httpPacketType, xHttpPhysicalConnection.SequenceNo, xHttpPhysicalConnection.XHttpConnection.ConnectionName, xHttpPhysicalConnection.XHttpConnection.Remote.LocalHostUniqueIdentifier);
				}
				else
				{
					HttpMessageCoder.WriteResponseHeader(binaryWriter, xHttpPhysicalConnection.XHttpConnection.Remote.ProtocolVersion, ITransportContext.ConnectionManager.Local.Uri, xHttpPhysicalConnection.SequenceNo, httpPacketType, xHttpPhysicalConnection.XHttpConnection.Remote.LocalHostUniqueIdentifier);
				}
				genuineChunkedStream4.WriteStream(xHttpPhysicalConnection.SentContent);
				xHttpPhysicalConnection.SentContent = genuineChunkedStream4;
				if (applyClss && !xHttpPhysicalConnection.XHttpConnection.IsClient && xHttpPhysicalConnection.ConnectionLevelSecurity != null)
				{
					GenuineChunkedStream genuineChunkedStream5 = new GenuineChunkedStream(releaseOnReadMode: false);
					xHttpPhysicalConnection.ConnectionLevelSecurity.Encrypt(xHttpPhysicalConnection.SentContent, genuineChunkedStream5);
					xHttpPhysicalConnection.SentContent = genuineChunkedStream5;
				}
				if (!xHttpPhysicalConnection.SentContent.CanSeek)
				{
					GenuineChunkedStream genuineChunkedStream6 = new GenuineChunkedStream(releaseOnReadMode: false);
					GenuineUtility.CopyStreamToStream(xHttpPhysicalConnection.SentContent, genuineChunkedStream6, xHttpPhysicalConnection.AsyncSendBuffer);
					xHttpPhysicalConnection.SentContent = genuineChunkedStream6;
				}
				genuineChunkedStream4 = new GenuineChunkedStream(releaseOnReadMode: false);
				StreamWriter streamWriter = new StreamWriter(new NonClosableStream(genuineChunkedStream4), Encoding.ASCII, 3500);
				if (xHttpPhysicalConnection.XHttpConnection.IsClient)
				{
					streamWriter.Write("POST /{0} HTTP/1.1\r\nAccept: */*\r\nContent-Type: application/octet-stream\r\nContent-Length: {1}\r\nUser-Agent: {2}\r\nHost: {3}\r\nConnection: Keep-Alive\r\nPragma: no-cache\r\n\r\n", xHttpPhysicalConnection.EntryUri, xHttpPhysicalConnection.SentContent.Length, xHttpPhysicalConnection.XHttpConnection.UserAgent, xHttpPhysicalConnection.LocalEndPoint);
				}
				else
				{
					string arg = DateTime.Now.ToString("r");
					streamWriter.Write("HTTP/1.1 200 OK\r\nServer: GXHTTP\r\nDate: {0}\r\nX-Powered-By: Genuine Channels\r\nCache-Control: private\r\nContent-Type: application/octet-stream\r\nContent-Length: {1}\r\n\r\n", arg, xHttpPhysicalConnection.SentContent.Length);
				}
				streamWriter.Flush();
				streamWriter.Close();
				genuineChunkedStream4.WriteStream(xHttpPhysicalConnection.SentContent);
				xHttpPhysicalConnection.SentContent = genuineChunkedStream4;
			}
			else
			{
				xHttpPhysicalConnection.SentContent.Position = 0L;
			}
			socket = xHttpPhysicalConnection.Socket;
			sentContent = xHttpPhysicalConnection.SentContent;
		}
		if (synchronous)
		{
			SyncSocketWritingStream outputStream = new SyncSocketWritingStream(this, socket, timeout, xHttpPhysicalConnection.XHttpConnection.DbgConnectionId, xHttpPhysicalConnection.Remote);
			GenuineUtility.CopyStreamToStreamPhysically(sentContent, outputStream, xHttpPhysicalConnection.AsyncSendBuffer);
			if (startAutomaticReceiving)
			{
				if (xHttpPhysicalConnection.XHttpConnection.IsClient)
				{
					LowLevel_HalfSync_Client_StartReceiving(xHttpPhysicalConnection);
				}
				else
				{
					LowLevel_HalfSync_Server_StartReceiving(xHttpPhysicalConnection.Socket);
				}
			}
		}
		else
		{
			xHttpPhysicalConnection.AsyncSendBufferCurrentPosition = 0;
			xHttpPhysicalConnection.AsyncSendBufferIsLastPacket = false;
			xHttpPhysicalConnection.AsyncSendBufferSizeOfValidContent = 0;
			xHttpPhysicalConnection.AsyncSendStream = sentContent;
			LowLevel_InitiateAsyncSending(xHttpPhysicalConnection);
		}
	}

	public void LowLevel_SendServerError(object PhysicalConnectionAndSocketAsObject)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		try
		{
			PhysicalConnectionAndSocket physicalConnectionAndSocket = (PhysicalConnectionAndSocket)PhysicalConnectionAndSocketAsObject;
			try
			{
				int timeout = GenuineUtility.GetTimeout(30000);
				SyncSocketWritingStream underlyingStream = new SyncSocketWritingStream(this, physicalConnectionAndSocket.Socket, timeout, physicalConnectionAndSocket.XHttpPhysicalConnection.XHttpConnection.DbgConnectionId, physicalConnectionAndSocket.XHttpPhysicalConnection.Remote);
				StreamWriter streamWriter = new StreamWriter(new NonClosableStream(underlyingStream), Encoding.ASCII, 3500);
				streamWriter.WriteLine("HTTP/1.1 409 Conflict\r\nServer: GXHTTP\r\nDate: {0}\r\nX-Powered-By: Genuine Channels\r\nContent-Length: 0\r\n\r\n");
				streamWriter.Flush();
				streamWriter.Close();
				binaryLogWriter.WriteEvent(LogCategory.Transport, "XHttpConnectionManager.LowLevel_SendServerError", LogMessageType.SynchronousSendingFinished, GenuineExceptions.Get_Debugging_GeneralWarning("409 Conflict HTTP response has been sent."), null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "409 Conflict HTTP response has been sent.");
			}
			catch (Exception exception)
			{
				binaryLogWriter.WriteEvent(LogCategory.Transport, "XHttpConnectionManager.LowLevel_SendServerError", LogMessageType.SynchronousSendingFinished, exception, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "409 Conflict HTTP response cannot be sent.");
			}
			finally
			{
				SocketUtility.CloseSocket(physicalConnectionAndSocket.Socket);
			}
		}
		catch
		{
		}
	}

	private bool LowLevel_InitiateAsyncSending(XHttpPhysicalConnection xHttpPhysicalConnection)
	{
		xHttpPhysicalConnection.AsyncSendBufferCurrentPosition = 0;
		xHttpPhysicalConnection.AsyncSendBufferSizeOfValidContent = GenuineUtility.TryToReadFromStream(xHttpPhysicalConnection.AsyncSendStream, xHttpPhysicalConnection.AsyncSendBuffer, 0, xHttpPhysicalConnection.AsyncSendBuffer.Length);
		if (xHttpPhysicalConnection.AsyncSendBufferSizeOfValidContent == 0)
		{
			return false;
		}
		xHttpPhysicalConnection.AsyncSendBufferIsLastPacket = xHttpPhysicalConnection.AsyncSendBufferSizeOfValidContent < xHttpPhysicalConnection.AsyncSendBuffer.Length;
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		if (binaryLogWriter != null && binaryLogWriter[LogCategory.Transport] > 0)
		{
			binaryLogWriter.WriteEvent(LogCategory.Transport, "XHttpConnectionManager.LowLevel_InitiateAsyncSending", LogMessageType.AsynchronousSendingStarted, null, null, xHttpPhysicalConnection.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, xHttpPhysicalConnection.XHttpConnection.DbgConnectionId, 0, 0, 0, null, null, null, null, "The connection has been obtained and the stream is being sent asynchronously.");
		}
		AsyncThreadStarter.QueueTask(new Async_InitiateSocketSending(xHttpPhysicalConnection.Socket, xHttpPhysicalConnection.AsyncSendBuffer, xHttpPhysicalConnection.AsyncSendBufferCurrentPosition, xHttpPhysicalConnection.AsyncSendBufferSizeOfValidContent - xHttpPhysicalConnection.AsyncSendBufferCurrentPosition, _AsyncSending_onEndSending, xHttpPhysicalConnection));
		return true;
	}

	private void LowLevel_OnEndAsyncSending(IAsyncResult ar)
	{
		XHttpPhysicalConnection xHttpPhysicalConnection = null;
		try
		{
			xHttpPhysicalConnection = (XHttpPhysicalConnection)ar.AsyncState;
			int num = xHttpPhysicalConnection.Socket.EndSend(ar);
			IncreaseBytesSent(num);
			xHttpPhysicalConnection.AsyncSendBufferCurrentPosition += num;
			if (xHttpPhysicalConnection.AsyncSendBufferCurrentPosition < xHttpPhysicalConnection.AsyncSendBufferSizeOfValidContent)
			{
				AsyncThreadStarter.QueueTask(new Async_InitiateSocketSending(xHttpPhysicalConnection.Socket, xHttpPhysicalConnection.AsyncSendBuffer, xHttpPhysicalConnection.AsyncSendBufferCurrentPosition, xHttpPhysicalConnection.AsyncSendBufferSizeOfValidContent - xHttpPhysicalConnection.AsyncSendBufferCurrentPosition, _AsyncSending_onEndSending, xHttpPhysicalConnection));
			}
			else if (!LowLevel_InitiateAsyncSending(xHttpPhysicalConnection))
			{
				if (xHttpPhysicalConnection.XHttpConnection.IsClient)
				{
					LowLevel_HalfSync_Client_StartReceiving(xHttpPhysicalConnection);
				}
				else
				{
					LowLevel_HalfSync_Server_StartReceiving(xHttpPhysicalConnection.Socket);
				}
			}
		}
		catch (Exception exception)
		{
			ConnectionFailed(exception, xHttpPhysicalConnection.XHttpConnection, xHttpPhysicalConnection);
		}
	}

	private string LowLevel_ReadToUpperLine(Stream stream, out int indexOfFirstDigit, out int indexOfLastDigit)
	{
		indexOfFirstDigit = -1;
		indexOfLastDigit = -1;
		StringBuilder stringBuilder = new StringBuilder(50);
		while (true)
		{
			int num = stream.ReadByte();
			switch (num)
			{
			case 13:
				continue;
			case 10:
				return stringBuilder.ToString();
			}
			char c = (char)(short)num;
			if (char.IsDigit(c) && indexOfFirstDigit < 0)
			{
				indexOfFirstDigit = stringBuilder.Length;
			}
			if (char.IsDigit(c) && indexOfFirstDigit >= 0)
			{
				indexOfLastDigit = stringBuilder.Length;
			}
			goto IL_006e;
			IL_006e:
			stringBuilder.Append(char.ToUpper(c));
		}
	}

	private Stream LowLevel_ParseHttpContent(int timeout, bool client, Socket socket, SecuritySession connectionLevelSecurity, out byte protocolVersion, out string uri, out int sequenceNo, out GenuineConnectionType genuineConnectionType, out Guid hostId, out HttpPacketType httpPacketType, out string connectionName, out int remoteHostUniqueIdentifier)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		protocolVersion = 3;
		uri = null;
		genuineConnectionType = GenuineConnectionType.None;
		hostId = Guid.Empty;
		httpPacketType = HttpPacketType.Unkown;
		connectionName = null;
		Stream stream = new BufferedStream(new SyncSocketStream(socket, this, timeout));
		int num = -1;
		GXHTTPHeaderParser gXHTTPHeaderParser = new GXHTTPHeaderParser(client);
		while (true)
		{
			string text = LowLevel_ReadToUpperLine(stream, out var indexOfFirstDigit, out var indexOfLastDigit);
			if (text.Length <= 0)
			{
				break;
			}
			if (gXHTTPHeaderParser.ParseHeader(text, indexOfFirstDigit, indexOfLastDigit) == GXHTTPHeaderParser.HeaderFields.Expect100Continue && gXHTTPHeaderParser.IsHttp11)
			{
				LowLevel_Send100Continue(socket);
			}
		}
		num = (int)gXHTTPHeaderParser.ContentLength;
		if (num < 0)
		{
			throw GenuineExceptions.Get_Receive_IncorrectData();
		}
		stream = new DelimiterStream(stream, num);
		if (binaryLogWriter != null && binaryLogWriter[LogCategory.LowLevelTransport] > 0)
		{
			GenuineChunkedStream genuineChunkedStream = null;
			bool flag = binaryLogWriter[LogCategory.LowLevelTransport] > 1;
			if (flag)
			{
				genuineChunkedStream = new GenuineChunkedStream(releaseOnReadMode: false);
				GenuineUtility.CopyStreamToStream(stream, genuineChunkedStream, num);
			}
			binaryLogWriter.WriteTransportContentEvent(LogCategory.LowLevelTransport, "XHttpConnectionManager.LowLevel_ParseHttpContent", LogMessageType.LowLevelTransport_SyncReceivingCompleted, null, null, null, flag ? genuineChunkedStream : null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, -1, num, socket.RemoteEndPoint.ToString(), null, null, "XHTTP Message.", num);
			if (flag)
			{
				stream = genuineChunkedStream;
			}
		}
		if (client && connectionLevelSecurity != null && connectionLevelSecurity.IsEstablished)
		{
			stream = connectionLevelSecurity.Decrypt(stream);
		}
		BinaryReader binaryReader = new BinaryReader(stream);
		if (client)
		{
			HttpMessageCoder.ReadResponseHeader(binaryReader, out uri, out sequenceNo, out httpPacketType, out remoteHostUniqueIdentifier);
		}
		else
		{
			HttpMessageCoder.ReadRequestHeader(binaryReader, out protocolVersion, out genuineConnectionType, out hostId, out httpPacketType, out sequenceNo, out connectionName, out remoteHostUniqueIdentifier);
		}
		return stream;
	}

	private void LowLevel_ParseLabelledStream(Stream stream, XHttpPhysicalConnection xHttpPhysicalConnection)
	{
		BinaryReader binaryReader = new BinaryReader(stream);
		while (binaryReader.ReadByte() == 0)
		{
			using LabelledStream inputStream = new LabelledStream(ITransportContext, stream, xHttpPhysicalConnection.AsyncReceiveBuffer);
			GenuineChunkedStream genuineChunkedStream = new GenuineChunkedStream(releaseOnReadMode: true);
			GenuineUtility.CopyStreamToStream(inputStream, genuineChunkedStream);
			ITransportContext.IIncomingStreamHandler.HandleMessage(genuineChunkedStream, xHttpPhysicalConnection.Remote, xHttpPhysicalConnection.XHttpConnection.GenuineConnectionType, xHttpPhysicalConnection.XHttpConnection.ConnectionName, xHttpPhysicalConnection.XHttpConnection.DbgConnectionId, useThisThread: false, null, xHttpPhysicalConnection.ConnectionLevelSecurity, null);
		}
	}

	private Exception LowLevel_ParseException(Stream stream)
	{
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter(new RemotingSurrogateSelector(), new StreamingContext(StreamingContextStates.Other));
			if (!(binaryFormatter.Deserialize(stream) is Exception result))
			{
				return GenuineExceptions.Get_Receive_IncorrectData();
			}
			return result;
		}
		catch
		{
			return GenuineExceptions.Get_Receive_IncorrectData();
		}
	}

	internal void LowLevel_HalfSync_Client_StartReceiving(XHttpPhysicalConnection xHttpPhysicalConnection)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		if (binaryLogWriter != null && binaryLogWriter[LogCategory.Transport] > 0)
		{
			binaryLogWriter.WriteEvent(LogCategory.Transport, "XHttpConnectionManager.LowLevel_HalfSync_Client_StartReceiving", LogMessageType.ReceivingStarted, null, null, xHttpPhysicalConnection.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, xHttpPhysicalConnection.ConnectionLevelSecurity, (xHttpPhysicalConnection.ConnectionLevelSecurity == null) ? null : xHttpPhysicalConnection.ConnectionLevelSecurity.Name, xHttpPhysicalConnection.XHttpConnection.DbgConnectionId, 0, 0, 0, null, null, null, null, "The asynchronous receiving is initiated.");
		}
		xHttpPhysicalConnection.CheckConnectionStatus();
		AsyncThreadStarter.QueueTask(new Async_InitiateSocketReceiving(xHttpPhysicalConnection.Socket, _unusedBuffer, 0, 1, _HalfSync_Client_onEndReceiving, xHttpPhysicalConnection));
	}

	private void LowLevel_HalfSync_Client_EndReceiving(IAsyncResult ar)
	{
		XHttpPhysicalConnection xHttpPhysicalConnection = (XHttpPhysicalConnection)ar.AsyncState;
		try
		{
			int num = xHttpPhysicalConnection.Socket.EndReceive(ar);
			if (num == 0)
			{
				throw GenuineExceptions.Get_Receive_Portion();
			}
			ITransportContext.ConnectionManager.IncreaseBytesReceived(num);
			xHttpPhysicalConnection.XHttpConnection.Renew();
			GenuineThreadPool.QueueUserWorkItem(_HalfSync_Client_onContinueReceiving, xHttpPhysicalConnection, longDuration: true);
		}
		catch (Exception exception)
		{
			ConnectionFailed(exception, xHttpPhysicalConnection.XHttpConnection, xHttpPhysicalConnection);
		}
	}

	internal void LowLevel_HalfSync_Server_StartReceiving(Socket socket)
	{
		AsyncThreadStarter.QueueTask(new Async_InitiateSocketReceiving(socket, _unusedBuffer, 0, 1, _HalfSync_Server_onEndReceiving, socket));
	}

	private void LowLevel_HalfSync_Server_EndReceiving(IAsyncResult ar)
	{
		try
		{
			Socket socket = (Socket)ar.AsyncState;
			try
			{
				int num = socket.EndReceive(ar);
				if (num == 0)
				{
					throw GenuineExceptions.Get_Receive_Portion();
				}
				ITransportContext.ConnectionManager.IncreaseBytesReceived(num);
				GenuineThreadPool.QueueUserWorkItem(_HalfSync_Server_onContinueReceiving, socket, longDuration: true);
			}
			catch
			{
				SocketUtility.CloseSocket(socket);
			}
		}
		catch (Exception exception)
		{
			ITransportContext.BinaryLogWriter?.WriteImplementationWarningEvent("XHttpConnectionManager.LowLevel_HalfSync_Server_EndReceiving", LogMessageType.Error, exception, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, "Something must be fixed...");
		}
	}

	private XHttpConnection Pool_CreateConnection(HostInformation remote, string primaryUri, bool isClient, string connectionName)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		if (connectionName == null)
		{
			connectionName = GetUniqueConnectionName();
		}
		ReaderAutoLocker readerAutoLocker = new ReaderAutoLocker(_disposeLock);
		try
		{
			XHttpConnection xHttpConnection = new XHttpConnection(this, isClient, connectionName);
			xHttpConnection.Remote = remote;
			xHttpConnection.CloseConnectionAfterInactivity = GenuineUtility.ConvertToMilliseconds(ITransportContext.IParameterProvider[GenuineParameter.ClosePersistentConnectionAfterInactivity]);
			xHttpConnection.GenuineConnectionType = GenuineConnectionType.Persistent;
			xHttpConnection.Sender = new XHttpPhysicalConnection(xHttpConnection, sender: true);
			xHttpConnection.Listener = new XHttpPhysicalConnection(xHttpConnection, sender: false);
			xHttpConnection.MessageContainer = new MessageContainer(ITransportContext);
			_persistent.Set(primaryUri, connectionName, xHttpConnection);
			if (ITransportContext.IParameterProvider[GenuineParameter.SecuritySessionForPersistentConnections] is string name)
			{
				xHttpConnection.Sender.ConnectionLevelSecurity = ITransportContext.IKeyStore.GetKey(name).CreateSecuritySession(name, null);
				xHttpConnection.Listener.ConnectionLevelSecurity = ITransportContext.IKeyStore.GetKey(name).CreateSecuritySession(name, null);
			}
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
			{
				binaryLogWriter.WriteConnectionParameterEvent(LogCategory.Connection, "XHttpConnectionManager.Pool_CreateConnection", LogMessageType.ConnectionParameters, null, remote, ITransportContext.IParameterProvider, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, xHttpConnection.DbgConnectionId, "An XHTTP connection is being established.");
			}
			return xHttpConnection;
		}
		finally
		{
			readerAutoLocker.Dispose();
		}
	}

	internal XHttpConnection Pool_EstablishPersistentConnection(HostInformation remote, string connectionName)
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
		int timeout = GenuineUtility.GetTimeout((TimeSpan)ITransportContext.IParameterProvider[GenuineParameter.ConnectTimeout]);
		XHttpConnection xHttpConnection = Pool_CreateConnection(remote, remote.Url, isClient: true, connectionName);
		xHttpConnection.Sender.CheckConnectionStatus();
		string text = ITransportContext.IParameterProvider[GenuineParameter.SecuritySessionForPersistentConnections] as string;
		if (text != null)
		{
			xHttpConnection.Sender.ConnectionLevelSecurity = ITransportContext.IKeyStore.GetKey(text).CreateSecuritySession(text, null);
			xHttpConnection.Listener.ConnectionLevelSecurity = ITransportContext.IKeyStore.GetKey(text).CreateSecuritySession(text, null);
		}
		Stream stream2 = Stream.Null;
		Stream stream3 = Stream.Null;
		GenuineChunkedStream genuineChunkedStream = null;
		bool flag = false;
		bool flag2;
		do
		{
			IL_0101:
			flag2 = false;
			if (GenuineUtility.IsTimeoutExpired(timeout))
			{
				throw GenuineExceptions.Get_Connect_CanNotConnectToRemoteHost(remote.Url, "Timeout expired.");
			}
			if (!flag)
			{
				genuineChunkedStream = new GenuineChunkedStream();
				BinaryWriter binaryWriter = new BinaryWriter(genuineChunkedStream);
				flag2 = xHttpConnection.GatherContentOfConnectionLevelSecuritySessions(stream2, stream3, genuineChunkedStream, xHttpConnection.Sender.ConnectionLevelSecurity, xHttpConnection.Listener.ConnectionLevelSecurity);
			}
			else
			{
				genuineChunkedStream.Position = 0L;
			}
			try
			{
				LowLevel_SendHttpContent(timeout, null, genuineChunkedStream, null, null, xHttpConnection.Sender, GenuineConnectionType.Persistent, HttpPacketType.Establishing, flag, synchronous: true, startAutomaticReceiving: false, applyClss: false);
				stream = LowLevel_ParseHttpContent(timeout, client: true, xHttpConnection.Sender.Socket, xHttpConnection.Sender.ConnectionLevelSecurity, out var _, out var uri, out var _, out var _, out var _, out var httpPacketType, out var _, out var remoteHostUniqueIdentifier);
				switch (httpPacketType)
				{
				case HttpPacketType.SenderError:
					throw GenuineExceptions.Get_Connect_CanNotConnectToRemoteHost(remote.ToString(), LowLevel_ParseException(stream).Message);
				default:
					throw GenuineExceptions.Get_Connect_CanNotConnectToRemoteHost(remote.ToString(), "Wrong response received from the remote host.");
				case HttpPacketType.Establishing:
					remote.UpdateUri(uri, remoteHostUniqueIdentifier);
					using (stream)
					{
						BinaryReader binaryReader = new BinaryReader(stream);
						int size = binaryReader.ReadInt32();
						stream2 = new GenuineChunkedStream(releaseOnReadMode: true);
						GenuineUtility.CopyStreamToStream(stream, stream2, size);
						size = binaryReader.ReadInt32();
						stream3 = new GenuineChunkedStream(releaseOnReadMode: true);
						GenuineUtility.CopyStreamToStream(stream, stream3, size);
					}
					flag = false;
					break;
				}
			}
			catch (Exception ex)
			{
				if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
				{
					binaryLogWriter.WriteEvent(LogCategory.Connection, "XHttpConnectionManager.Pool_EstablishPersistentConnection", LogMessageType.SynchronousSendingFinished, ex, null, remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, xHttpConnection.Sender.ConnectionLevelSecurity, text, xHttpConnection.DbgConnectionId, 0, 0, 0, null, null, null, null, "Cannot send the establishing packet to the remote host.");
				}
				if (ConnectionManager.IsExceptionCritical(ex as OperationException))
				{
					throw;
				}
				flag = true;
				xHttpConnection.Sender.CloseSocket();
				goto IL_0101;
			}
		}
		while ((xHttpConnection.Sender.ConnectionLevelSecurity != null && !xHttpConnection.Sender.ConnectionLevelSecurity.IsEstablished) || (xHttpConnection.Listener.ConnectionLevelSecurity != null && !xHttpConnection.Listener.ConnectionLevelSecurity.IsEstablished) || flag2);
		if (binaryLogWriter != null && binaryLogWriter[LogCategory.HostInformation] > 0)
		{
			binaryLogWriter.WriteHostInformationEvent("XHttpConnectionManager.Pool_EstablishPersistentConnection", LogMessageType.HostInformationCreated, null, remote, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, xHttpConnection.Sender.ConnectionLevelSecurity, text, xHttpConnection.DbgConnectionId, "HostInformation is ready for actions.");
		}
		xHttpConnection.Remote = remote;
		xHttpConnection.Sender.MarkAsAvailable();
		if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
		{
			binaryLogWriter.WriteEvent(LogCategory.Connection, "XHttpConnectionManager.Pool_EstablishPersistentConnection", LogMessageType.ConnectionEstablished, null, null, xHttpConnection.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, xHttpConnection.Sender.ConnectionLevelSecurity, text, xHttpConnection.DbgConnectionId, 1, 0, 0, GetType().Name, remote.LocalPhysicalAddress.ToString(), remote.PhysicalAddress.ToString(), null, "The connection to the remote host is established.");
		}
		try
		{
			Pool_HandleClientConnection(xHttpConnection.Listener);
		}
		catch (Exception exception)
		{
			ConnectionFailed(exception, xHttpConnection, xHttpConnection.Listener);
		}
		return xHttpConnection;
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
					SocketUtility.CloseSocket(socket);
					throw GenuineExceptions.Get_Server_EndPointIsAlreadyBeingListenedTo(text);
				}
				_listeningSockets[text] = acceptConnectionClosure;
			}
			ITransportContext.IGenuineEventProvider.Fire(new GenuineEventArgs(GenuineEventType.GeneralListenerStarted, null, Local, text));
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.AcceptingConnection] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.AcceptingConnection, "XHttpConnectionManager.StartListening", LogMessageType.ListeningStarted, null, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, text, null, null, null, "A listening socket has been associated with the \"{0}\" local end point.", text);
			}
			Thread thread = new Thread(acceptConnectionClosure.AcceptConnections);
			thread.IsBackground = true;
			thread.Start();
		}
		catch (Exception exception)
		{
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.AcceptingConnection] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.AcceptingConnection, "XHttpConnectionManager.StartListening", LogMessageType.ListeningStarted, exception, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, text, null, null, null, "Can't associate a socket with the specified end point \"{0}\".", text);
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
			binaryLogWriter.WriteEvent(LogCategory.AcceptingConnection, "XHttpConnectionManager.StopListening", LogMessageType.ListeningStopped, null, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, text, null, null, null, "A listening socket is no longer associated with the \"{0}\" local end point.", text);
		}
	}

	void IAcceptConnectionConsumer.AcceptConnection(Socket clientSocket)
	{
		LowLevel_HalfSync_Server_StartReceiving(clientSocket);
	}

	public bool IsDisposed()
	{
		return base._disposed;
	}

	public void TimerCallback()
	{
		GenuineThreadPool.QueueUserWorkItem(_internal_TimerCallback, null, longDuration: false);
	}

	private void Internal_TimerCallback_InspectPersistentConnections(object xHttpConnectionAsObject, object parametersAsObject)
	{
		XHttpConnection xHttpConnection = (XHttpConnection)xHttpConnectionAsObject;
		Internal_TimerCallback_Parameters internal_TimerCallback_Parameters = (Internal_TimerCallback_Parameters)parametersAsObject;
		if (GenuineUtility.IsTimeoutExpired(xHttpConnection.ShutdownTime, internal_TimerCallback_Parameters.Now))
		{
			internal_TimerCallback_Parameters.ExpiredConnections.Add(xHttpConnection);
		}
		if (GenuineUtility.IsTimeoutExpired(xHttpConnection.LastTimeContentWasSent + internal_TimerCallback_Parameters.SendPingAfter, internal_TimerCallback_Parameters.Now))
		{
			internal_TimerCallback_Parameters.SendPingTo.Add(xHttpConnection);
		}
	}

	private void Internal_TimerCallback(object ignored)
	{
		int tickCount = GenuineUtility.TickCount;
		int sendPingAfter = GenuineUtility.ConvertToMilliseconds(ITransportContext.IParameterProvider[GenuineParameter.PersistentConnectionSendPingAfterInactivity]);
		ArrayList arrayList = new ArrayList();
		ArrayList arrayList2 = new ArrayList();
		Internal_TimerCallback_Parameters internal_TimerCallback_Parameters = new Internal_TimerCallback_Parameters();
		internal_TimerCallback_Parameters.Now = tickCount;
		internal_TimerCallback_Parameters.SendPingAfter = sendPingAfter;
		internal_TimerCallback_Parameters.ExpiredConnections = arrayList;
		internal_TimerCallback_Parameters.SendPingTo = arrayList2;
		_persistent.InspectAllConnections(_internal_TimerCallback_InspectPersistentConnections, internal_TimerCallback_Parameters);
		foreach (XHttpConnection item in arrayList)
		{
			ConnectionFailed(GenuineExceptions.Get_Channel_ConnectionClosedAfterTimeout(), item, null);
		}
		foreach (XHttpConnection item2 in arrayList2)
		{
			GenuineThreadPool.QueueUserWorkItem(SendPing, item2, longDuration: true);
		}
	}

	private void SendPing(object xHttpConnectionAsObject)
	{
		XHttpConnection xHttpConnection = (XHttpConnection)xHttpConnectionAsObject;
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		try
		{
			Message message = new Message(ITransportContext, xHttpConnection.Remote, -3, new TransportHeaders(), Stream.Null);
			message.IsSynchronous = false;
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.MessageProcessing] > 0)
			{
				binaryLogWriter.WriteMessageCreatedEvent("XHttpConnectionManager.SendPing", LogMessageType.MessageCreated, null, message, true, xHttpConnection.Remote, null, "HTTP PING", "HTTP PING", GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, xHttpConnection.DbgConnectionId, -1, null, -1, null, "HTTP ping is created and sent.");
			}
			Send(message);
		}
		catch (Exception exception)
		{
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Connection, "XHttpConnectionManager.SendPing", LogMessageType.ConnectionPingSending, exception, null, xHttpConnection.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, xHttpConnection.DbgConnectionId, 0, 0, 0, null, null, null, null, "Cannot send a ping.");
			}
		}
	}
}
