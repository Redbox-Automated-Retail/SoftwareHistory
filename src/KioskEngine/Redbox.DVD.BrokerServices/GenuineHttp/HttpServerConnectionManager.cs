using System;
using System.Collections;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Web;
using Belikov.Common.ThreadProcessing;
using Belikov.GenuineChannels.BufferPooling;
using Belikov.GenuineChannels.Connection;
using Belikov.GenuineChannels.DotNetRemotingLayer;
using Belikov.GenuineChannels.Logbook;
using Belikov.GenuineChannels.Messaging;
using Belikov.GenuineChannels.Parameters;
using Belikov.GenuineChannels.Receiving;
using Belikov.GenuineChannels.Security;
using Belikov.GenuineChannels.TransportContext;
using Belikov.GenuineChannels.Utilities;

namespace Belikov.GenuineChannels.GenuineHttp;

internal class HttpServerConnectionManager : ConnectionManager, ITimerConsumer
{
	private class ReleaseConnections_Parameters
	{
		public ArrayList FailedConnections;

		public HostInformation HostInformation;
	}

	private class Internal_TimerCallback_Parameters
	{
		public ArrayList SendPingTo = new ArrayList();

		public ArrayList ExpiredConnections = new ArrayList();

		public int CloseListenerConnectionsAfter;

		public int Now;
	}

	private int _closeInvocationConnectionAfterInactivity;

	private IMessageRegistrator _iMessageRegistrator = new MessageRegistratorWithLimitedTime();

	private PersistentConnectionStorage _persistent = new PersistentConnectionStorage();

	private Hashtable _invocation = Hashtable.Synchronized(new Hashtable());

	private PersistentConnectionStorage.ProcessConnectionEventHandler _releaseConnections_InspectPersistentConnections;

	private WaitCallback _internal_TimerCallback;

	private PersistentConnectionStorage.ProcessConnectionEventHandler _internal_TimerCallback_InspectPersistentConnections;

	public HttpServerConnectionManager(ITransportContext iTransportContext)
		: base(iTransportContext)
	{
		Local = new HostInformation("_ghttp://" + iTransportContext.HostIdentifier, iTransportContext);
		_releaseConnections_InspectPersistentConnections = ReleaseConnections_InspectPersistentConnections;
		_internal_TimerCallback_InspectPersistentConnections = Internal_TimerCallback_InspectPersistentConnections;
		_internal_TimerCallback = Internal_TimerCallback;
		_closeInvocationConnectionAfterInactivity = GenuineUtility.ConvertToMilliseconds(ITransportContext.IParameterProvider[GenuineParameter.CloseInvocationConnectionAfterInactivity]);
		TimerProvider.Attach(this);
	}

	protected override void InternalSend(Message message)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		switch (message.SecuritySessionParameters.GenuineConnectionType)
		{
		case GenuineConnectionType.Persistent:
		{
			if (message.ConnectionName == null)
			{
				message.ConnectionName = message.SecuritySessionParameters.ConnectionName;
			}
			if (!(_persistent.Get(message.Recipient.Uri, message.ConnectionName) is HttpServerConnection httpServerConnection))
			{
				throw GenuineExceptions.Get_Send_DestinationIsUnreachable(message.Recipient.Uri);
			}
			bool flag = false;
			lock (httpServerConnection.Listener_Lock)
			{
				if (httpServerConnection.Listener != null)
				{
					try
					{
						if (httpServerConnection.Listener.HttpContext.Response.IsClientConnected)
						{
							flag = true;
							GenuineChunkedStream genuineChunkedStream = LowLevel_CreateStreamWithHeader(HttpPacketType.Usual, httpServerConnection.Listener_SequenceNo, message.Recipient);
							MessageCoder.FillInLabelledStream(message, httpServerConnection.Listener_MessageContainer, httpServerConnection.Listener_MessagesBeingSent, genuineChunkedStream, httpServerConnection.Listener_IntermediateBuffer, (int)ITransportContext.IParameterProvider[GenuineParameter.HttpRecommendedPacketSize]);
							if (binaryLogWriter != null && binaryLogWriter[LogCategory.MessageProcessing] > 0)
							{
								for (int i = 0; i < httpServerConnection.Listener_MessagesBeingSent.Count; i++)
								{
									Message message2 = (Message)httpServerConnection.Listener_MessagesBeingSent[i];
									binaryLogWriter.WriteEvent(LogCategory.MessageProcessing, "HttpServerConnectionManager.InternalSend", LogMessageType.MessageIsSentAsynchronously, null, message2, httpServerConnection.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, httpServerConnection.Listener_SecuritySession, null, httpServerConnection.DbgConnectionId, httpServerConnection.Listener_SequenceNo, 0, 0, null, null, null, null, "The message will be sent in the LISTENER stream N: {0}.", httpServerConnection.Listener_SequenceNo);
								}
							}
							httpServerConnection.Listener_SentStream = genuineChunkedStream;
							LowLevel_SendStream(httpServerConnection.Listener.HttpContext, listener: true, httpServerConnection, applyClss: true, ref httpServerConnection.Listener_SentStream, httpServerConnection);
						}
					}
					catch (Exception exception)
					{
						if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
						{
							binaryLogWriter.WriteEvent(LogCategory.Connection, "HttpServerConnectionManager.InternalSend", LogMessageType.ConnectionEstablishing, exception, null, httpServerConnection.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, httpServerConnection.DbgConnectionId, 0, 0, 0, null, null, null, null, "Asynchronous sending failed. Listener seq no: {0}.", httpServerConnection.Listener_SequenceNo);
						}
					}
					finally
					{
						httpServerConnection.Listener.Complete(completedSynchronously: false);
						httpServerConnection.Listener = null;
					}
				}
				if (!flag)
				{
					try
					{
						httpServerConnection.Listener_MessageContainer.AddMessage(message, onlyCheckIn: false);
						break;
					}
					catch (Exception reason)
					{
						httpServerConnection.Dispose(reason);
						ITransportContext.KnownHosts.ReleaseHostResources(message.Recipient, reason);
						break;
					}
				}
				break;
			}
		}
		case GenuineConnectionType.Named:
			throw new NotSupportedException("The named pattern is not supported by the GHTTP server channel.");
		case GenuineConnectionType.Invocation:
			lock (_invocation.SyncRoot)
			{
				if (_invocation.ContainsKey(message.SecuritySessionParameters.ConnectionName))
				{
					if (_invocation[message.SecuritySessionParameters.ConnectionName] != null)
					{
						throw GenuineExceptions.Get_Processing_LogicError("The response has been already registered.");
					}
					_invocation[message.SecuritySessionParameters.ConnectionName] = message;
					break;
				}
			}
			throw GenuineExceptions.Get_Send_DestinationIsUnreachable(message.SecuritySessionParameters.ConnectionName);
		case GenuineConnectionType.Persistent | GenuineConnectionType.Named:
			break;
		}
	}

	public void HandleIncomingRequest(object httpServerRequestResultAsObject)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		HttpServerRequestResult httpServerRequestResult = (HttpServerRequestResult)httpServerRequestResultAsObject;
		HttpServerConnection httpServerConnection = null;
		bool flag = false;
		try
		{
			HttpRequest request = httpServerRequestResult.HttpContext.Request;
			Stream stream = request.InputStream;
			if (stream.Length <= 0)
			{
				if (binaryLogWriter != null && binaryLogWriter[LogCategory.LowLevelTransport] > 0)
				{
					binaryLogWriter.WriteEvent(LogCategory.Connection, "HttpServerConnectionManager.HandleIncomingRequest", LogMessageType.LowLevelTransport_AsyncReceivingCompleted, null, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "Empty content has been received. It will be ignored.");
				}
				return;
			}
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.LowLevelTransport] > 0)
			{
				bool flag2 = binaryLogWriter[LogCategory.LowLevelTransport] > 1;
				GenuineChunkedStream genuineChunkedStream = null;
				if (flag2)
				{
					genuineChunkedStream = new GenuineChunkedStream(releaseOnReadMode: false);
					GenuineUtility.CopyStreamToStream(stream, genuineChunkedStream);
				}
				binaryLogWriter.WriteTransportContentEvent(LogCategory.LowLevelTransport, "HttpServerConnectionManager.HandleIncomingRequest", LogMessageType.LowLevelTransport_AsyncReceivingCompleted, null, null, null, flag2 ? genuineChunkedStream : null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, -1, httpServerRequestResult.HttpContext.Request.ContentLength, httpServerRequestResult.HttpContext.Request.UserHostAddress, null, null, "HTTP request has been received.");
				if (flag2)
				{
					stream = genuineChunkedStream;
				}
			}
			BinaryReader binaryReader = new BinaryReader(stream);
			HttpMessageCoder.ReadRequestHeader(binaryReader, out var protocolVersion, out var genuineConnectionType, out var hostId, out var httpPacketType, out var sequenceNo, out var connectionName, out var remoteHostUniqueIdentifier);
			HostInformation hostInformation = ITransportContext.KnownHosts["_ghttp://" + hostId.ToString("N")];
			hostInformation.ProtocolVersion = protocolVersion;
			hostInformation.PhysicalAddress = request.UserHostAddress;
			hostInformation.UpdateUri(hostInformation.Uri, remoteHostUniqueIdentifier);
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Connection, "HttpServerConnectionManager.HandleIncomingRequest", LogMessageType.ReceivingFinished, null, null, hostInformation, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "HTTP request is being processed. Packet type: {0}. Sequence no: {1}. Content length: {2}. Host address: {3}.", Enum.Format(typeof(HttpPacketType), httpPacketType, "g"), sequenceNo, request.ContentLength, request.UserHostAddress);
			}
			GenuineChunkedStream genuineChunkedStream2 = new GenuineChunkedStream(releaseOnReadMode: false);
			BinaryWriter binaryWriter = new BinaryWriter(genuineChunkedStream2);
			HttpMessageCoder.WriteResponseHeader(binaryWriter, protocolVersion, ITransportContext.ConnectionManager.Local.Uri, sequenceNo, httpPacketType, hostInformation.LocalHostUniqueIdentifier);
			switch (genuineConnectionType)
			{
			case GenuineConnectionType.Persistent:
				lock (hostInformation.PersistentConnectionEstablishingLock)
				{
					httpServerConnection = _persistent.Get(hostInformation.Uri, connectionName) as HttpServerConnection;
					if (httpServerConnection != null && httpServerConnection._disposed)
					{
						httpServerConnection = null;
					}
					if (httpPacketType == HttpPacketType.Establishing_ResetConnection)
					{
						httpServerConnection?.Dispose(GenuineExceptions.Get_Receive_ConnectionClosed("Client sends Establishing_ResetCLSS flag."));
						httpServerConnection = null;
					}
					if (httpServerConnection == null)
					{
						httpServerConnection = new HttpServerConnection(ITransportContext, hostId, hostInformation, connectionName, GenuineUtility.ConvertToMilliseconds(ITransportContext.IParameterProvider[GenuineParameter.ClosePersistentConnectionAfterInactivity]) + GenuineUtility.ConvertToMilliseconds(ITransportContext.IParameterProvider[GenuineParameter.MaxTimeSpanToReconnect]));
						if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
						{
							binaryLogWriter.WriteConnectionParameterEvent(LogCategory.Connection, "HttpServerConnectionManager.HandleIncomingRequest", LogMessageType.ConnectionParameters, null, hostInformation, ITransportContext.IParameterProvider, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, httpServerConnection.DbgConnectionId, "HTTP connection is being established.");
						}
						_persistent.Set(hostInformation.Uri, connectionName, httpServerConnection);
						string text = ITransportContext.IParameterProvider[GenuineParameter.SecuritySessionForPersistentConnections] as string;
						if (text != null)
						{
							httpServerConnection.Sender_SecuritySession = ITransportContext.IKeyStore.GetKey(text).CreateSecuritySession(text, null);
							httpServerConnection.Listener_SecuritySession = ITransportContext.IKeyStore.GetKey(text).CreateSecuritySession(text, null);
						}
						if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
						{
							binaryLogWriter.WriteEvent(LogCategory.Connection, "HttpServerConnectionManager.HandleIncomingRequest", LogMessageType.ConnectionEstablished, null, null, hostInformation, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, httpServerConnection.Sender_SecuritySession, text, httpServerConnection.DbgConnectionId, 1, 0, 0, GetType().Name, null, null, null, "The connection is being established.");
						}
						if (binaryLogWriter != null && binaryLogWriter[LogCategory.HostInformation] > 0)
						{
							binaryLogWriter.WriteHostInformationEvent("HttpServerConnectionManager.HandleIncomingRequest", LogMessageType.HostInformationCreated, null, hostInformation, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, httpServerConnection.Sender_SecuritySession, text, httpServerConnection.DbgConnectionId, "HostInformation is ready for actions.");
						}
						ITransportContext.IGenuineEventProvider.Fire(new GenuineEventArgs(GenuineEventType.GHttpConnectionAccepted, null, hostInformation, request.UserHostAddress));
					}
				}
				httpServerConnection.Renew();
				httpServerConnection.SignalState(GenuineEventType.GeneralConnectionEstablished, null, null);
				switch (httpPacketType)
				{
				case HttpPacketType.Establishing_ResetConnection:
				case HttpPacketType.Establishing:
				{
					Stream stream2 = Stream.Null;
					int num = binaryReader.ReadInt32();
					if (httpServerConnection.Sender_SecuritySession != null)
					{
						using Stream input = new DelimiterStream(stream, num);
						stream2 = httpServerConnection.Sender_SecuritySession.EstablishSession(input, connectionLevel: true);
					}
					if (stream2 == null)
					{
						stream2 = Stream.Null;
					}
					GenuineChunkedStreamSizeLabel genuineChunkedStreamSizeLabel = new GenuineChunkedStreamSizeLabel(genuineChunkedStream2);
					try
					{
						GenuineUtility.CopyStreamToStream(stream2, genuineChunkedStream2);
					}
					finally
					{
						genuineChunkedStreamSizeLabel.Dispose();
					}
					num = binaryReader.ReadInt32();
					stream2 = Stream.Null;
					if (httpServerConnection.Listener_SecuritySession != null)
					{
						stream2 = httpServerConnection.Listener_SecuritySession.EstablishSession(new DelimiterStream(stream, num), connectionLevel: true);
					}
					if (stream2 == null)
					{
						stream2 = Stream.Null;
					}
					genuineChunkedStreamSizeLabel = new GenuineChunkedStreamSizeLabel(genuineChunkedStream2);
					try
					{
						GenuineUtility.CopyStreamToStream(stream2, genuineChunkedStream2);
					}
					finally
					{
						genuineChunkedStreamSizeLabel.Dispose();
					}
					Stream content = genuineChunkedStream2;
					LowLevel_SendStream(httpServerRequestResult.HttpContext, listener: false, null, applyClss: false, ref content, httpServerConnection);
					break;
				}
				case HttpPacketType.Listening:
					flag = LowLevel_ProcessListenerRequest(httpServerRequestResult, httpServerConnection, sequenceNo);
					break;
				case HttpPacketType.Usual:
					LowLevel_ProcessSenderRequest(genuineConnectionType, stream, httpServerRequestResult, httpServerConnection, sequenceNo, hostInformation);
					break;
				default:
					if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
					{
						binaryLogWriter.WriteEvent(LogCategory.Connection, "HttpServerConnectionManager.HandleIncomingRequest", LogMessageType.Error, GenuineExceptions.Get_Debugging_GeneralWarning("Unexpected type of the packet."), null, hostInformation, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, httpServerConnection?.DbgConnectionId ?? (-1), 0, 0, 0, null, null, null, null, "Unexpected type of the packet. Packet type: {0}. Sequence no: {1}. Content length: {2}. Host address: {3}.", Enum.Format(typeof(HttpPacketType), httpPacketType, "g"), sequenceNo, request.ContentLength, request.UserHostAddress);
					}
					break;
				}
				break;
			case GenuineConnectionType.Named:
				throw new NotSupportedException("Named connections are not supported.");
			case GenuineConnectionType.Invocation:
				hostInformation.Renew(_closeInvocationConnectionAfterInactivity, canMakeShorter: false);
				LowLevel_ProcessSenderRequest(genuineConnectionType, stream, httpServerRequestResult, null, sequenceNo, hostInformation);
				break;
			case GenuineConnectionType.Persistent | GenuineConnectionType.Named:
				break;
			}
		}
		catch (Exception exception)
		{
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Connection, "HttpServerConnectionManager.HandleIncomingRequest", LogMessageType.ReceivingFinished, exception, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "Error occurred while processing incoming HTTP request.");
			}
		}
		finally
		{
			if (!flag)
			{
				httpServerRequestResult.Complete(completedSynchronously: true);
			}
		}
	}

	public bool LowLevel_ProcessListenerRequest(HttpServerRequestResult httpServerRequestResult, HttpServerConnection httpServerConnection, int requestedSequenceNo)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		GenuineChunkedStream genuineChunkedStream = null;
		lock (httpServerConnection.Listener_Lock)
		{
			if (httpServerConnection.Listener != null)
			{
				try
				{
					LowLevel_ReportServerError(httpServerConnection.Listener.HttpContext);
				}
				finally
				{
					httpServerConnection.Listener.Complete(completedSynchronously: false);
					httpServerConnection.Listener = null;
				}
			}
			try
			{
				if (requestedSequenceNo == httpServerConnection.Listener_SequenceNo && httpServerConnection.Listener_SentStream != null)
				{
					httpServerConnection.Listener_SentStream.Position = 0L;
					LowLevel_SendStream(httpServerRequestResult.HttpContext, listener: true, httpServerConnection, applyClss: false, ref httpServerConnection.Listener_SentStream, httpServerConnection);
					return false;
				}
				if (requestedSequenceNo < httpServerConnection.Listener_SequenceNo || requestedSequenceNo > httpServerConnection.Listener_SequenceNo + 1)
				{
					if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
					{
						string text = $"Desynchronization error. Current sequence number: {httpServerConnection.Listener_SequenceNo}. Requested sequence number: {requestedSequenceNo}.";
						binaryLogWriter.WriteEvent(LogCategory.Connection, "HttpServerConnectionManager.LowLevel_ProcessListenerRequest", LogMessageType.Error, GenuineExceptions.Get_Debugging_GeneralWarning(text), null, httpServerConnection.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, httpServerConnection.DbgConnectionId, 0, 0, 0, null, null, null, null, text);
					}
					Stream content = LowLevel_CreateStreamWithHeader(HttpPacketType.Desynchronization, requestedSequenceNo, httpServerConnection.Remote);
					MessageCoder.FillInLabelledStream(null, null, null, (GenuineChunkedStream)content, httpServerConnection.Listener_IntermediateBuffer, (int)ITransportContext.IParameterProvider[GenuineParameter.HttpRecommendedPacketSize]);
					LowLevel_SendStream(httpServerRequestResult.HttpContext, listener: true, httpServerConnection, applyClss: true, ref content, httpServerConnection);
					return false;
				}
				if (httpServerConnection.Listener_SentStream != null)
				{
					httpServerConnection.Listener_MessagesBeingSent.Clear();
					httpServerConnection.Listener_SentStream.Close();
					httpServerConnection.Listener_SentStream = null;
				}
				Message message = httpServerConnection.Listener_MessageContainer.GetMessage();
				if (message == null)
				{
					if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
					{
						binaryLogWriter.WriteEvent(LogCategory.Connection, "HttpServerConnectionManager.LowLevel_ProcessListenerRequest", LogMessageType.ReceivingFinished, null, null, httpServerConnection.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, httpServerConnection.DbgConnectionId, 0, 0, 0, null, null, null, null, "Listener request is postponed.");
					}
					httpServerConnection.Listener_Opened = GenuineUtility.TickCount;
					httpServerConnection.Listener = httpServerRequestResult;
					httpServerConnection.Listener_SequenceNo = requestedSequenceNo;
					return true;
				}
				genuineChunkedStream = LowLevel_CreateStreamWithHeader(HttpPacketType.Usual, requestedSequenceNo, httpServerConnection.Remote);
				MessageCoder.FillInLabelledStream(message, httpServerConnection.Listener_MessageContainer, httpServerConnection.Listener_MessagesBeingSent, genuineChunkedStream, httpServerConnection.Listener_IntermediateBuffer, (int)ITransportContext.IParameterProvider[GenuineParameter.HttpRecommendedPacketSize]);
				if (binaryLogWriter != null && binaryLogWriter[LogCategory.MessageProcessing] > 0)
				{
					for (int i = 0; i < httpServerConnection.Listener_MessagesBeingSent.Count; i++)
					{
						Message message2 = (Message)httpServerConnection.Listener_MessagesBeingSent[i];
						binaryLogWriter.WriteEvent(LogCategory.MessageProcessing, "HttpServerConnectionManager.LowLevel_ProcessListenerRequest", LogMessageType.MessageIsSentAsynchronously, null, message2, httpServerConnection.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, httpServerConnection.Listener_SecuritySession, null, httpServerConnection.DbgConnectionId, httpServerConnection.Listener_SequenceNo, 0, 0, null, null, null, null, "The message is sent in the LISTENER stream N: {0}.", httpServerConnection.Listener_SequenceNo);
					}
				}
				httpServerConnection.Listener_SentStream = genuineChunkedStream;
				httpServerConnection.Listener_SequenceNo = requestedSequenceNo;
				LowLevel_SendStream(httpServerRequestResult.HttpContext, listener: true, httpServerConnection, applyClss: true, ref httpServerConnection.Listener_SentStream, httpServerConnection);
			}
			catch (Exception exception)
			{
				if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
				{
					binaryLogWriter.WriteEvent(LogCategory.Connection, "HttpServerConnectionManager.LowLevel_ProcessListenerRequest", LogMessageType.Error, exception, null, httpServerConnection.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, httpServerConnection.DbgConnectionId, 0, 0, 0, null, null, null, null, "Error occurred while processing the listener request N: {0}.", httpServerConnection.Listener_SequenceNo);
				}
			}
		}
		return false;
	}

	public void LowLevel_ProcessSenderRequest(GenuineConnectionType genuineConnectionType, Stream input, HttpServerRequestResult httpServerRequestResult, HttpServerConnection httpServerConnection, int sequenceNo, HostInformation remote)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		GenuineChunkedStream genuineChunkedStream = null;
		bool flag = genuineConnectionType != GenuineConnectionType.Persistent;
		BinaryReader binaryReader = new BinaryReader(input);
		BufferKeeper bufferKeeper = new BufferKeeper(0);
		try
		{
			switch (genuineConnectionType)
			{
			case GenuineConnectionType.Persistent:
			{
				Exception ex = null;
				try
				{
					if (httpServerConnection.Sender_SecuritySession != null)
					{
						input = httpServerConnection.Sender_SecuritySession.Decrypt(input);
						binaryReader = new BinaryReader(input);
					}
					while (binaryReader.ReadByte() == 0)
					{
						using LabelledStream inputStream = new LabelledStream(ITransportContext, input, bufferKeeper.Buffer);
						GenuineChunkedStream genuineChunkedStream2 = new GenuineChunkedStream(releaseOnReadMode: true);
						GenuineUtility.CopyStreamToStream(inputStream, genuineChunkedStream2);
						ITransportContext.IIncomingStreamHandler.HandleMessage(genuineChunkedStream2, httpServerConnection.Remote, genuineConnectionType, httpServerConnection.ConnectionName, httpServerConnection.DbgConnectionId, useThisThread: false, _iMessageRegistrator, httpServerConnection.Sender_SecuritySession, httpServerRequestResult);
					}
				}
				catch (Exception ex2)
				{
					ex = ex2;
					if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
					{
						binaryLogWriter.WriteEvent(LogCategory.Connection, "HttpServerConnectionManager.LowLevel_ProcessSenderRequest", LogMessageType.Error, ex2, null, httpServerConnection.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, httpServerConnection.DbgConnectionId, 0, 0, 0, null, null, null, null, "Error occurred while processing the sender request N: {0}.", httpServerConnection.Sender_SequenceNo);
					}
				}
				if (ex != null)
				{
					ex = OperationException.WrapException(ex);
					genuineChunkedStream = LowLevel_CreateStreamWithHeader(HttpPacketType.SenderError, sequenceNo, remote);
					BinaryFormatter binaryFormatter = new BinaryFormatter(new RemotingSurrogateSelector(), new StreamingContext(StreamingContextStates.Other));
					binaryFormatter.Serialize(genuineChunkedStream, ex);
				}
				else
				{
					genuineChunkedStream = LowLevel_CreateStreamWithHeader(HttpPacketType.SenderResponse, sequenceNo, remote);
					MessageCoder.FillInLabelledStream(null, null, null, genuineChunkedStream, bufferKeeper.Buffer, (int)ITransportContext.IParameterProvider[GenuineParameter.HttpRecommendedPacketSize]);
				}
				break;
			}
			case GenuineConnectionType.Invocation:
			{
				string text = Guid.NewGuid().ToString("N");
				try
				{
					if (binaryReader.ReadByte() != 0)
					{
						binaryLogWriter?.WriteImplementationWarningEvent("HttpServerConnectionManager.LowLevel_ProcessSenderRequest", LogMessageType.Error, GenuineExceptions.Get_Debugging_GeneralWarning("The invocation request doesn't contain any messages."), GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, "The invocation request doesn't contain any messages.");
					}
					using (LabelledStream stream = new LabelledStream(ITransportContext, input, bufferKeeper.Buffer))
					{
						_invocation[text] = null;
						ITransportContext.IIncomingStreamHandler.HandleMessage(stream, remote, genuineConnectionType, text, -1, useThisThread: true, null, null, httpServerRequestResult);
					}
					if (binaryReader.ReadByte() != 1)
					{
						binaryLogWriter?.WriteImplementationWarningEvent("HttpServerConnectionManager.LowLevel_ProcessSenderRequest", LogMessageType.Error, GenuineExceptions.Get_Debugging_GeneralWarning("The invocation request must not contain more than one message."), GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, "The invocation request must not contain more than one message.");
					}
					genuineChunkedStream = LowLevel_CreateStreamWithHeader(HttpPacketType.Usual, sequenceNo, remote);
					Message message = _invocation[text] as Message;
					MessageCoder.FillInLabelledStream(message, null, null, genuineChunkedStream, bufferKeeper.Buffer, (int)ITransportContext.IParameterProvider[GenuineParameter.HttpRecommendedPacketSize]);
				}
				finally
				{
					_invocation.Remove(text);
				}
				break;
			}
			}
		}
		finally
		{
			bufferKeeper.Dispose();
		}
		Stream content = genuineChunkedStream;
		LowLevel_SendStream(httpServerRequestResult.HttpContext, listener: false, null, applyClss: true, ref content, httpServerConnection);
	}

	public GenuineChunkedStream LowLevel_CreateStreamWithHeader(HttpPacketType httpPacketType, int sequenceNo, HostInformation remote)
	{
		GenuineChunkedStream genuineChunkedStream = new GenuineChunkedStream(releaseOnReadMode: false);
		BinaryWriter binaryWriter = new BinaryWriter(genuineChunkedStream);
		HttpMessageCoder.WriteResponseHeader(binaryWriter, remote.ProtocolVersion, ITransportContext.ConnectionManager.Local.Uri, sequenceNo, httpPacketType, remote.LocalHostUniqueIdentifier);
		return genuineChunkedStream;
	}

	public void LowLevel_SendStream(HttpContext httpContext, bool listener, HttpServerConnection httpServerConnection, bool applyClss, ref Stream content, HttpServerConnection httpServerConnectionForDebugging)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		try
		{
			if (applyClss)
			{
				SecuritySession securitySession = null;
				if (httpServerConnection != null && listener && httpServerConnection.Listener_SecuritySession != null && httpServerConnection.Listener_SecuritySession.IsEstablished)
				{
					securitySession = httpServerConnection.Listener_SecuritySession;
				}
				if (httpServerConnection != null && !listener && httpServerConnection.Sender_SecuritySession != null && httpServerConnection.Sender_SecuritySession.IsEstablished)
				{
					securitySession = httpServerConnection.Sender_SecuritySession;
				}
				if (securitySession != null)
				{
					GenuineChunkedStream genuineChunkedStream = new GenuineChunkedStream(releaseOnReadMode: false);
					securitySession.Encrypt(content, genuineChunkedStream);
					content = genuineChunkedStream;
				}
			}
			HttpResponse response = httpContext.Response;
			response.ContentType = "application/octet-stream";
			response.StatusCode = 200;
			response.StatusDescription = "OK";
			int num = (int)content.Length;
			response.AppendHeader("Content-Length", num.ToString());
			Stream outputStream = response.OutputStream;
			GenuineUtility.CopyStreamToStream(content, outputStream, num);
			ITransportContext.ConnectionManager.IncreaseBytesSent(num);
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.LowLevelTransport] > 0)
			{
				bool flag = binaryLogWriter[LogCategory.LowLevelTransport] > 1;
				GenuineChunkedStream genuineChunkedStream2 = null;
				if (flag)
				{
					genuineChunkedStream2 = new GenuineChunkedStream(releaseOnReadMode: false);
					GenuineUtility.CopyStreamToStream(content, genuineChunkedStream2);
				}
				binaryLogWriter.WriteTransportContentEvent(LogCategory.LowLevelTransport, "HttpServerConnectionManager.LowLevel_SendStream", LogMessageType.LowLevelTransport_AsyncSendingInitiating, null, null, httpServerConnectionForDebugging.Remote, genuineChunkedStream2, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, httpServerConnectionForDebugging.DbgConnectionId, (int)content.Length, null, null, null, "Response is sent back to the client. Buffer: {0}; BufferOutput: {1}; Charset: {2}; ContentEncoding: {3}; ContentType: {4}; IsClientConnected: {5}; StatusCode: {6}; StatusDescription: {7}; SuppressContent: {8}; Content-Length: {9}. Connection: {10}.", response.Buffer, response.BufferOutput, response.Charset, response.ContentEncoding, response.ContentType, response.IsClientConnected, response.StatusCode, response.StatusDescription, response.SuppressContent, num, listener ? "LISTENER" : "SENDER");
			}
		}
		catch (Exception exception)
		{
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Connection, "HttpServerConnectionManager.LowLevel_SendStream", LogMessageType.MessageIsSentAsynchronously, exception, null, httpServerConnectionForDebugging.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, httpServerConnectionForDebugging.DbgConnectionId, 0, 0, 0, null, null, null, null, "Error occurred while sending a response.");
			}
			throw;
		}
	}

	public void LowLevel_ReportServerError(HttpContext httpContext)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		try
		{
			HttpResponse response = httpContext.Response;
			response.ContentType = "application/octet-stream";
			response.StatusCode = 409;
			response.StatusDescription = "Conflict";
			response.AppendHeader("Content-Length", "0");
		}
		catch (Exception exception)
		{
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Connection, "HttpServerConnectionManager.LowLevel_ReportServerError", LogMessageType.Error, exception, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "Error occurred while sending 409/conflict error.");
			}
		}
	}

	private void Pool_SendThroughListener(HttpServerConnection httpServerConnection, HttpPacketType httpPacketType)
	{
		lock (httpServerConnection.Listener_Lock)
		{
			if (httpServerConnection.Listener == null)
			{
				return;
			}
			try
			{
				Stream content = LowLevel_CreateStreamWithHeader(httpPacketType, httpServerConnection.Listener_SequenceNo, httpServerConnection.Remote);
				LowLevel_SendStream(httpServerConnection.Listener.HttpContext, listener: true, httpServerConnection, applyClss: true, ref content, httpServerConnection);
			}
			finally
			{
				httpServerConnection.Listener.Complete(completedSynchronously: false);
				httpServerConnection.Listener = null;
			}
		}
	}

	private void ReleaseConnections_InspectPersistentConnections(object httpServerConnectionAsObject, object releaseConnections_ParametersAsObject)
	{
		HttpServerConnection httpServerConnection = (HttpServerConnection)httpServerConnectionAsObject;
		ReleaseConnections_Parameters releaseConnections_Parameters = (ReleaseConnections_Parameters)releaseConnections_ParametersAsObject;
		if (releaseConnections_Parameters.HostInformation == null || httpServerConnection.Remote == releaseConnections_Parameters.HostInformation)
		{
			releaseConnections_Parameters.FailedConnections.Add(httpServerConnection);
		}
	}

	public override void ReleaseConnections(HostInformation hostInformation, GenuineConnectionType genuineConnectionType, Exception reason)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		ArrayList arrayList = new ArrayList();
		WriterAutoLocker writerAutoLocker = new WriterAutoLocker(_disposeLock);
		try
		{
			if (base._disposed)
			{
				return;
			}
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Connection, "HttpServerConnectionManager.ReleaseConnections", LogMessageType.ReleaseConnections, reason, null, hostInformation, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, Enum.Format(typeof(GenuineConnectionType), genuineConnectionType, "g"), null, null, null, "Connections \"{0}\" will be terminated.", Enum.Format(typeof(GenuineConnectionType), genuineConnectionType, "g"), null);
			}
			if ((genuineConnectionType & GenuineConnectionType.Persistent) != GenuineConnectionType.None)
			{
				ReleaseConnections_Parameters releaseConnections_Parameters = new ReleaseConnections_Parameters();
				releaseConnections_Parameters.FailedConnections = arrayList;
				releaseConnections_Parameters.HostInformation = hostInformation;
				_persistent.InspectAllConnections(_releaseConnections_InspectPersistentConnections, releaseConnections_Parameters);
			}
			foreach (HttpServerConnection item in arrayList)
			{
				lock (item.Listener_Lock)
				{
					if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
					{
						binaryLogWriter.WriteEvent(LogCategory.Connection, "HttpClientConnectionManager.ReleaseConnections", LogMessageType.ConnectionShuttingDown, reason, null, item.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, item.DbgConnectionId, 0, 0, 0, null, null, null, null, "The connection is being shut down manually.");
					}
					item.SignalState(GenuineEventType.GeneralConnectionClosed, reason, null);
					_persistent.Remove(item.Remote.Uri, item.ConnectionName);
					if (item.Listener != null)
					{
						Pool_SendThroughListener(item, HttpPacketType.ClosedManually);
					}
				}
			}
		}
		finally
		{
			writerAutoLocker.Dispose();
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
	}

	public void TimerCallback()
	{
		GenuineThreadPool.QueueUserWorkItem(_internal_TimerCallback, null, longDuration: false);
	}

	private void Internal_TimerCallback_InspectPersistentConnections(object httpServerConnectionAsObject, object parametersAsObject)
	{
		HttpServerConnection httpServerConnection = (HttpServerConnection)httpServerConnectionAsObject;
		Internal_TimerCallback_Parameters internal_TimerCallback_Parameters = (Internal_TimerCallback_Parameters)parametersAsObject;
		lock (httpServerConnection.Remote.PersistentConnectionEstablishingLock)
		{
			if (GenuineUtility.IsTimeoutExpired(httpServerConnection.ShutdownTime, internal_TimerCallback_Parameters.Now))
			{
				internal_TimerCallback_Parameters.ExpiredConnections.Add(httpServerConnection);
			}
			if (httpServerConnection.Listener != null && GenuineUtility.IsTimeoutExpired(httpServerConnection.Listener_Opened + internal_TimerCallback_Parameters.CloseListenerConnectionsAfter, internal_TimerCallback_Parameters.Now))
			{
				internal_TimerCallback_Parameters.SendPingTo.Add(httpServerConnection);
			}
		}
	}

	private void Internal_TimerCallback(object ignored)
	{
		Internal_TimerCallback_Parameters internal_TimerCallback_Parameters = new Internal_TimerCallback_Parameters();
		internal_TimerCallback_Parameters.ExpiredConnections = new ArrayList();
		internal_TimerCallback_Parameters.SendPingTo = new ArrayList();
		internal_TimerCallback_Parameters.CloseListenerConnectionsAfter = GenuineUtility.ConvertToMilliseconds(ITransportContext.IParameterProvider[GenuineParameter.ClosePersistentConnectionAfterInactivity]);
		internal_TimerCallback_Parameters.Now = GenuineUtility.TickCount;
		_persistent.InspectAllConnections(_internal_TimerCallback_InspectPersistentConnections, internal_TimerCallback_Parameters);
		foreach (HttpServerConnection item in internal_TimerCallback_Parameters.SendPingTo)
		{
			Pool_SendThroughListener(item, HttpPacketType.ListenerTimedOut);
		}
		foreach (HttpServerConnection expiredConnection in internal_TimerCallback_Parameters.ExpiredConnections)
		{
			_persistent.Remove(expiredConnection.Remote.Uri, expiredConnection.ConnectionName);
			if (expiredConnection.Listener != null)
			{
				Pool_SendThroughListener(expiredConnection, HttpPacketType.ClosedManually);
			}
		}
	}

	public override void StartListening(object endPoint)
	{
		throw new NotSupportedException();
	}

	public override void StopListening(object endPoint)
	{
		throw new NotSupportedException();
	}
}
