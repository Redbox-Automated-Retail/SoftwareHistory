using System;
using System.Collections;
using System.IO;
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

namespace Belikov.GenuineChannels.GenuineSharedMemory;

internal class SharedMemoryConnectionManager : ConnectionManager, ITimerConsumer
{
	private class SynchronousReceiving
	{
		public SharedMemoryConnection SharedMemoryConnection;

		public SharedMemoryConnectionManager SharedMemoryConnectionManager;

		public void ReceiveSynchronously()
		{
			try
			{
				while (SharedMemoryConnection.IsValid)
				{
					using Stream inputStream = SharedMemoryConnection.LowLevel_ReadSync(GenuineUtility.GetTimeout(SharedMemoryConnection._closeAfterInactivity));
					GenuineChunkedStream genuineChunkedStream = new GenuineChunkedStream(releaseOnReadMode: true);
					GenuineUtility.CopyStreamToStream(inputStream, genuineChunkedStream);
					SharedMemoryConnection.Renew();
					if (genuineChunkedStream.Length == 0)
					{
						BinaryLogWriter binaryLogWriter = SharedMemoryConnectionManager.ITransportContext.BinaryLogWriter;
						if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
						{
							binaryLogWriter.WriteEvent(LogCategory.Connection, "SharedMemoryConnectionManager.ReceiveSynchronously", LogMessageType.ConnectionPingReceived, null, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, SharedMemoryConnection.DbgConnectionId, 0, 0, 0, null, null, null, null, "A message with zero size is treated as ping.");
						}
					}
					else
					{
						SharedMemoryConnectionManager.ITransportContext.IIncomingStreamHandler.HandleMessage(genuineChunkedStream, SharedMemoryConnection.Remote, GenuineConnectionType.Persistent, null, SharedMemoryConnection.DbgConnectionId, useThisThread: false, null, SharedMemoryConnection.ConnectionLevelSecurity, null);
						SharedMemoryConnection.Renew();
					}
				}
			}
			catch (Exception exception)
			{
				SharedMemoryConnectionManager.ConnectionFailed(exception, SharedMemoryConnection);
			}
		}
	}

	private int _sendTimeoutSpan;

	private Hashtable _persistent = Hashtable.Synchronized(new Hashtable());

	private SMAcceptConnectionClosure _smAcceptConnectionClosure;

	public SharedMemoryConnectionManager(ITransportContext iTransportContext)
		: base(iTransportContext)
	{
		_sendTimeoutSpan = GenuineUtility.ConvertToMilliseconds(iTransportContext.IParameterProvider[GenuineParameter.SMSendTimeout]);
		Local = new HostInformation("_gshmem://" + iTransportContext.HostIdentifier, iTransportContext);
		TimerProvider.Attach(this);
	}

	protected override void InternalSend(Message message)
	{
		SharedMemoryConnection sharedMemoryConnection = null;
		string key = message.Recipient.Url;
		bool flag = false;
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
				key = message.Recipient.Uri;
				break;
			}
			if (!flag)
			{
				sharedMemoryConnection = _persistent[key] as SharedMemoryConnection;
				if (sharedMemoryConnection == null)
				{
					sharedMemoryConnection = LowLevel_OpenConnection(message.Recipient, Local.Uri, out var remoteUri, out var remoteHostUniqueIdentifier);
					message.Recipient.UpdateUri(remoteUri, remoteHostUniqueIdentifier);
					sharedMemoryConnection.Remote = message.Recipient;
					sharedMemoryConnection.SignalState(GenuineEventType.GeneralConnectionEstablished, null, null);
					_persistent[key] = sharedMemoryConnection;
					Connection_InitiateReceiving(sharedMemoryConnection);
				}
			}
			else
			{
				sharedMemoryConnection = _persistent[key] as SharedMemoryConnection;
				if (sharedMemoryConnection == null)
				{
					throw GenuineExceptions.Get_Send_DestinationIsUnreachable(message.Recipient.Uri);
				}
			}
		}
		SendSync(message, sharedMemoryConnection);
	}

	private void SendSync(Message message, SharedMemoryConnection sharedMemoryConnection)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		try
		{
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Connection, "SharedMemoryConnectionManager.SendSync", LogMessageType.MessageIsSentSynchronously, null, message, sharedMemoryConnection.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, sharedMemoryConnection.ConnectionLevelSecurity, (sharedMemoryConnection.ConnectionLevelSecurity == null) ? null : sharedMemoryConnection.ConnectionLevelSecurity.Name, sharedMemoryConnection.DbgConnectionId, 0, 0, 0, null, null, null, null, "Message is sent synchronously through the Shared Memory connection.");
			}
			int num = GenuineUtility.GetTimeout(_sendTimeoutSpan);
			if (GenuineUtility.IsTimeoutExpired(message.FinishTime, num))
			{
				num = message.FinishTime;
			}
			try
			{
				if (!Monitor.TryEnter(sharedMemoryConnection.WriteAccess, GenuineUtility.GetMillisecondsLeft(num)))
				{
					throw GenuineExceptions.Get_Send_Timeout();
				}
			}
			catch
			{
				throw GenuineExceptions.Get_Send_Timeout();
			}
			try
			{
				sharedMemoryConnection.LowLevel_SendSync(message.SerializedContent, num);
			}
			catch (Exception exception)
			{
				ConnectionFailed(exception, sharedMemoryConnection);
				throw;
			}
			finally
			{
				Monitor.Exit(sharedMemoryConnection.WriteAccess);
			}
			message.Dispose();
		}
		catch (Exception exception2)
		{
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Connection, "SharedMemoryConnectionManager.SendSync", LogMessageType.MessageIsSentSynchronously, exception2, message, sharedMemoryConnection.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, sharedMemoryConnection.DbgConnectionId, 0, 0, 0, null, null, null, null, "The message cannot be delivered.");
			}
			throw;
		}
		finally
		{
			message.Dispose();
		}
	}

	public override void StartListening(object endPoint)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		if (_smAcceptConnectionClosure != null)
		{
			throw GenuineExceptions.Get_Server_EndPointIsAlreadyBeingListenedTo(_smAcceptConnectionClosure.ShareName);
		}
		SharedMemoryConnection sharedMemoryConnection = null;
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
		string text = endPoint as string;
		if (text == null || text.Length <= 0 || !text.StartsWith("gshmem"))
		{
			throw GenuineExceptions.Get_Server_IncorrectAddressToListen(text);
		}
		try
		{
			sharedMemoryConnection = new SharedMemoryConnection(ITransportContext, text, isServer: true, setCloseStatusOnExit: true);
			_smAcceptConnectionClosure = new SMAcceptConnectionClosure(ITransportContext, sharedMemoryConnection, this, text);
			ITransportContext.IGenuineEventProvider.Fire(new GenuineEventArgs(GenuineEventType.GeneralListenerStarted, null, Local, endPoint));
			Thread thread = new Thread(_smAcceptConnectionClosure.AcceptConnections);
			thread.IsBackground = true;
			thread.Start();
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.AcceptingConnection, "SharedMemoryConnectionManager.StartListening", LogMessageType.ListeningStarted, null, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, text, null, null, null, "\"{0}\" is now listened.", text);
			}
		}
		catch (Exception exception)
		{
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.AcceptingConnection, "SharedMemoryConnectionManager.StartListening", LogMessageType.ListeningStarted, exception, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, text, null, null, null, "Listening to \"{0}\" cannot be started.", text);
			}
			sharedMemoryConnection?.ReleaseUnmanagedResources();
			throw;
		}
	}

	public override void StopListening(object endPoint)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		if (_smAcceptConnectionClosure != null)
		{
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.AcceptingConnection, "SharedMemoryConnectionManager.StopListening", LogMessageType.ListeningStopped, null, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, _smAcceptConnectionClosure.ShareName, null, null, null, "\"{0}\" is not now listened.", _smAcceptConnectionClosure.ShareName);
			}
			_smAcceptConnectionClosure.StopListening.Set();
			_smAcceptConnectionClosure = null;
		}
	}

	public override void ReleaseConnections(HostInformation hostInformation, GenuineConnectionType genuineConnectionType, Exception reason)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		reason = GenuineExceptions.Get_Channel_ConnectionShutDown(reason);
		if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
		{
			binaryLogWriter.WriteEvent(LogCategory.Connection, "SharedMemoryConnectionManager.ReleaseConnections", LogMessageType.ReleaseConnections, reason, null, hostInformation, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, Enum.Format(typeof(GenuineConnectionType), genuineConnectionType, "g"), null, null, null, "Connections \"{0}\" will be terminated.", Enum.Format(typeof(GenuineConnectionType), genuineConnectionType, "g"), null);
		}
		if (hostInformation == null)
		{
			InternalDispose(reason);
			return;
		}
		SharedMemoryConnection sharedMemoryConnection = null;
		if (hostInformation.Url != null)
		{
			sharedMemoryConnection = _persistent[hostInformation.Url] as SharedMemoryConnection;
		}
		if (sharedMemoryConnection != null)
		{
			ConnectionFailed(reason, sharedMemoryConnection);
		}
		sharedMemoryConnection = null;
		if (hostInformation.Uri != null)
		{
			sharedMemoryConnection = _persistent[hostInformation.Uri] as SharedMemoryConnection;
		}
		if (sharedMemoryConnection != null)
		{
			ConnectionFailed(reason, sharedMemoryConnection);
		}
	}

	public override void InternalDispose(Exception reason)
	{
		object[] array = null;
		lock (_persistent.SyncRoot)
		{
			array = new object[_persistent.Count];
			_persistent.Values.CopyTo(array, 0);
			_persistent.Clear();
		}
		object[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			SharedMemoryConnection sharedMemoryConnection = (SharedMemoryConnection)array2[i];
			ConnectionFailed(reason, sharedMemoryConnection);
		}
	}

	private SharedMemoryConnection LowLevel_OpenConnection(HostInformation remote, string localUri, out string remoteUri, out int remoteHostUniqueIdentifier)
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
		remoteUri = null;
		Stream stream = null;
		Stream stream2 = null;
		string url = remote.Url;
		int timeout = GenuineUtility.GetTimeout((TimeSpan)ITransportContext.IParameterProvider[GenuineParameter.ConnectTimeout]);
		IParameterProvider iParameterProvider = ITransportContext.IParameterProvider;
		string name = GenuineSharedMemoryChannel.ConstructSharedObjectName("MUTEX" + url, iParameterProvider);
		string name2 = GenuineSharedMemoryChannel.ConstructSharedObjectName("CC" + url, iParameterProvider);
		string name3 = GenuineSharedMemoryChannel.ConstructSharedObjectName("CA" + url, iParameterProvider);
		SharedMemoryConnection sharedMemoryConnection = new SharedMemoryConnection(ITransportContext, url, isServer: false, setCloseStatusOnExit: false);
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
		{
			binaryLogWriter.WriteConnectionParameterEvent(LogCategory.Connection, "SharedMemoryConnectionManager.LowLevel_OpenConnection", LogMessageType.ConnectionParameters, null, remote, ITransportContext.IParameterProvider, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, sharedMemoryConnection.DbgConnectionId, "A Shared Memory connection is being established.");
		}
		string text = "gshmem://" + Guid.NewGuid().ToString("N");
		SharedMemoryConnection sharedMemoryConnection2 = new SharedMemoryConnection(ITransportContext, text, isServer: true, setCloseStatusOnExit: true);
		BinaryWriter binaryWriter = MessageCoder.SerializeConnectionHeader(3, GenuineConnectionType.Persistent, "Default");
		binaryWriter.Write(text);
		Mutex mutex = null;
		try
		{
			mutex = WindowsAPI.OpenMutex(name);
			NamedEvent namedEvent = NamedEvent.OpenNamedEvent(name2);
			NamedEvent namedEvent2 = NamedEvent.OpenNamedEvent(name3);
			if (!GenuineUtility.WaitOne(mutex, GenuineUtility.GetMillisecondsLeft(timeout)))
			{
				throw GenuineExceptions.Get_Connect_CanNotConnectToRemoteHost(url, "Can not acquire the lock for the global mutex.");
			}
			namedEvent2.ManualResetEvent.Reset();
			namedEvent.ManualResetEvent.Set();
			sharedMemoryConnection.LowLevel_SendSync(binaryWriter.BaseStream, timeout);
			if (!GenuineUtility.WaitOne(namedEvent2.ManualResetEvent, GenuineUtility.GetMillisecondsLeft(timeout)))
			{
				throw GenuineExceptions.Get_Connect_CanNotConnectToRemoteHost(url, "Remote server did not accept a request within the specified time span.");
			}
		}
		finally
		{
			if (mutex != null)
			{
				try
				{
					mutex.ReleaseMutex();
				}
				catch
				{
				}
				try
				{
					mutex.Close();
				}
				catch
				{
				}
			}
		}
		string text2 = ITransportContext.IParameterProvider[GenuineParameter.SecuritySessionForPersistentConnections] as string;
		SecuritySession securitySession = null;
		if (text2 != null)
		{
			securitySession = ITransportContext.IKeyStore.GetKey(text2).CreateSecuritySession(text2, null);
		}
		if (securitySession != null && !securitySession.IsEstablished)
		{
			bool flag = true;
			while (true)
			{
				stream = Stream.Null;
				try
				{
					if (!flag)
					{
						stream = sharedMemoryConnection2.LowLevel_ReadSync(timeout);
					}
					else
					{
						flag = false;
					}
					stream2 = securitySession.EstablishSession(stream, connectionLevel: true);
					if (stream2 != null)
					{
						sharedMemoryConnection2.LowLevel_SendSync(stream2, timeout);
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
		sharedMemoryConnection2.ConnectionLevelSecurity = securitySession;
		using (GenuineChunkedStream genuineChunkedStream = new GenuineChunkedStream(releaseOnReadMode: false))
		{
			BinaryWriter binaryWriter2 = new BinaryWriter(genuineChunkedStream);
			binaryWriter2.Write(localUri);
			binaryWriter2.Write(remote.LocalHostUniqueIdentifier);
			sharedMemoryConnection2.LowLevel_SendSync(genuineChunkedStream, timeout);
			using Stream input = sharedMemoryConnection2.LowLevel_ReadSync(timeout);
			BinaryReader binaryReader = new BinaryReader(input);
			remoteUri = binaryReader.ReadString();
			remoteHostUniqueIdentifier = binaryReader.ReadInt32();
		}
		sharedMemoryConnection2.Remote = remote;
		sharedMemoryConnection2.Remote.UpdateUri(remoteUri, remoteHostUniqueIdentifier);
		sharedMemoryConnection2.Remote.GenuinePersistentConnectionState = GenuinePersistentConnectionState.Opened;
		if (binaryLogWriter != null && binaryLogWriter[LogCategory.HostInformation] > 0)
		{
			binaryLogWriter.WriteHostInformationEvent("SharedMemoryConnectionManager.LowLevel_OpenConnection", LogMessageType.HostInformationCreated, null, remote, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, sharedMemoryConnection2.DbgConnectionId, "HostInformation is ready for actions.");
		}
		if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
		{
			binaryLogWriter.WriteEvent(LogCategory.Connection, "SharedMemoryConnectionManager.LowLevel_OpenConnection", LogMessageType.ConnectionEstablished, null, null, remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, securitySession, text2, sharedMemoryConnection2.DbgConnectionId, 1, 0, 0, GetType().Name, null, null, null, "The connection to the remote host is established.");
		}
		return sharedMemoryConnection2;
	}

	private SharedMemoryConnection LowLevel_AcceptConnection_1(string url, string localUri, byte protocolVersion, out string remoteUri, out int remoteHostUniqueIdentifier)
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
		int timeout = GenuineUtility.GetTimeout((TimeSpan)ITransportContext.IParameterProvider[GenuineParameter.ConnectTimeout]);
		SharedMemoryConnection sharedMemoryConnection = new SharedMemoryConnection(ITransportContext, url, isServer: false, setCloseStatusOnExit: true);
		if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
		{
			binaryLogWriter.WriteConnectionParameterEvent(LogCategory.Connection, "SharedMemoryConnectionManager.LowLevel_AcceptConnection", LogMessageType.ConnectionParameters, null, null, ITransportContext.IParameterProvider, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, sharedMemoryConnection.DbgConnectionId, "The connection is being established to \"{0}\".", url);
		}
		string text = ITransportContext.IParameterProvider[GenuineParameter.SecuritySessionForPersistentConnections] as string;
		SecuritySession securitySession = null;
		if (text != null)
		{
			securitySession = ITransportContext.IKeyStore.GetKey(text).CreateSecuritySession(text, null);
		}
		if (securitySession != null && !securitySession.IsEstablished)
		{
			while (true)
			{
				stream = Stream.Null;
				try
				{
					stream = sharedMemoryConnection.LowLevel_ReadSync(timeout);
					stream2 = securitySession.EstablishSession(stream, connectionLevel: true);
					if (stream2 != null)
					{
						sharedMemoryConnection.LowLevel_SendSync(stream2, timeout);
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
		sharedMemoryConnection.ConnectionLevelSecurity = securitySession;
		HostInformation hostInformation = null;
		using (Stream input = sharedMemoryConnection.LowLevel_ReadSync(timeout))
		{
			BinaryReader binaryReader = new BinaryReader(input);
			remoteUri = binaryReader.ReadString();
			hostInformation = ITransportContext.KnownHosts[remoteUri];
			if (protocolVersion > 0)
			{
				remoteHostUniqueIdentifier = binaryReader.ReadInt32();
			}
		}
		using (GenuineChunkedStream genuineChunkedStream = new GenuineChunkedStream(releaseOnReadMode: false))
		{
			BinaryWriter binaryWriter = new BinaryWriter(genuineChunkedStream);
			binaryWriter.Write(localUri);
			if (protocolVersion > 0)
			{
				binaryWriter.Write(hostInformation.LocalHostUniqueIdentifier);
			}
			sharedMemoryConnection.LowLevel_SendSync(genuineChunkedStream, timeout);
		}
		if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
		{
			binaryLogWriter.WriteEvent(LogCategory.Connection, "SharedMemoryConnectionManager.LowLevel_AcceptConnection_1", LogMessageType.ConnectionEstablished, null, null, hostInformation, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, securitySession, text, sharedMemoryConnection.DbgConnectionId, 1, 0, 0, GetType().Name, null, null, null, "The connection to the remote host has been established.");
		}
		return sharedMemoryConnection;
	}

	internal void Connection_AcceptConnection(string shareName, byte protocolVersion)
	{
		try
		{
			string remoteUri = null;
			SharedMemoryConnection sharedMemoryConnection = LowLevel_AcceptConnection_1(shareName, Local.Uri, protocolVersion, out remoteUri, out var remoteHostUniqueIdentifier);
			sharedMemoryConnection.Remote = ITransportContext.KnownHosts[remoteUri];
			sharedMemoryConnection.Remote.ProtocolVersion = protocolVersion;
			sharedMemoryConnection.Remote.GenuinePersistentConnectionState = GenuinePersistentConnectionState.Accepted;
			sharedMemoryConnection.Remote.UpdateUri(remoteUri, remoteHostUniqueIdentifier);
			_persistent[remoteUri] = sharedMemoryConnection;
			sharedMemoryConnection.SignalState(GenuineEventType.GeneralConnectionEstablished, null, null);
			Connection_InitiateReceiving(sharedMemoryConnection);
		}
		catch (Exception ex)
		{
			BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Connection, "SharedMemoryConnectionManager.Connection_AcceptConnection", LogMessageType.ConnectionEstablished, ex, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "The connection from the remote host \"{0}\" has been refused.", shareName);
			}
			ITransportContext.IGenuineEventProvider.Fire(new GenuineEventArgs(GenuineEventType.GeneralListenerFailure, ex, null, shareName));
		}
	}

	internal void Connection_InitiateReceiving(SharedMemoryConnection sharedMemoryConnection)
	{
		SynchronousReceiving synchronousReceiving = new SynchronousReceiving();
		synchronousReceiving.SharedMemoryConnection = sharedMemoryConnection;
		synchronousReceiving.SharedMemoryConnectionManager = this;
		Thread thread = new Thread(synchronousReceiving.ReceiveSynchronously);
		thread.IsBackground = true;
		thread.Start();
	}

	private void ConnectionFailed(Exception exception, SharedMemoryConnection sharedMemoryConnection)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		try
		{
			sharedMemoryConnection.IsValid = false;
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Connection, "SharedMemoryConnectionManager.ConnectionFailed", LogMessageType.ConnectionFailed, exception, null, sharedMemoryConnection.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, sharedMemoryConnection.DbgConnectionId, 0, 0, 0, null, null, null, null, "Connection has failed.");
			}
			if (sharedMemoryConnection.Remote.GenuinePersistentConnectionState == GenuinePersistentConnectionState.Accepted)
			{
				_persistent.Remove(sharedMemoryConnection.Remote.Uri);
			}
			else
			{
				_persistent.Remove(sharedMemoryConnection.Remote.Url);
			}
			ITransportContext.KnownHosts.ReleaseHostResources(sharedMemoryConnection.Remote, exception);
			sharedMemoryConnection.ReleaseUnmanagedResources();
			sharedMemoryConnection.SignalState(GenuineEventType.GeneralConnectionClosed, exception, null);
		}
		catch (Exception exception2)
		{
			binaryLogWriter?.WriteImplementationWarningEvent("SharedMemoryConnectionManager.ConnectionFailed", LogMessageType.CriticalError, exception2, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, "Unexpected exception inside the SharedMemoryClientConnectionManager.ConnectionFailed method. Most likely, something must be fixed.");
		}
	}

	public void TimerCallback()
	{
		int tickCount = GenuineUtility.TickCount;
		int num = GenuineUtility.ConvertToMilliseconds(ITransportContext.IParameterProvider[GenuineParameter.PersistentConnectionSendPingAfterInactivity]);
		lock (_persistent.SyncRoot)
		{
			foreach (object item in _persistent)
			{
				DictionaryEntry dictionaryEntry = (DictionaryEntry)item;
				SharedMemoryConnection sharedMemoryConnection = (SharedMemoryConnection)dictionaryEntry.Value;
				if (GenuineUtility.IsTimeoutExpired(sharedMemoryConnection.LastTimeAMessageWasSent + num, tickCount))
				{
					GenuineThreadPool.QueueUserWorkItem(SendPing, sharedMemoryConnection, longDuration: false);
				}
			}
		}
	}

	private void SendPing(object sharedMemoryConnectionAsObject)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		SharedMemoryConnection sharedMemoryConnection = (SharedMemoryConnection)sharedMemoryConnectionAsObject;
		try
		{
			Message message = new Message(ITransportContext, sharedMemoryConnection.Remote, -3, new TransportHeaders(), Stream.Null);
			message.IsSynchronous = false;
			message.FinishTime = GenuineUtility.GetTimeout((TimeSpan)message.ITransportContext.IParameterProvider[GenuineParameter.InvocationTimeout]);
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Connection, "SharedMemoryConnectionManager.SendPing", LogMessageType.ConnectionPingSending, null, message, sharedMemoryConnection.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, sharedMemoryConnection.DbgConnectionId, 0, 0, 0, null, null, null, null, "Ping is being sending.");
			}
			message.SerializedContent = Stream.Null;
			SendSync(message, sharedMemoryConnection);
		}
		catch (Exception exception)
		{
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Connection, "SharedMemoryConnectionManager.SendPing", LogMessageType.ConnectionPingSent, exception, null, sharedMemoryConnection.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, sharedMemoryConnection.DbgConnectionId, 0, 0, 0, null, null, null, null, "Ping cannot be sent.");
			}
		}
	}
}
