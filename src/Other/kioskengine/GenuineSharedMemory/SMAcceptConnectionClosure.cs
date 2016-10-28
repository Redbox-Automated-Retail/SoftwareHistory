using System;
using System.IO;
using System.Threading;
using Belikov.GenuineChannels.DotNetRemotingLayer;
using Belikov.GenuineChannels.Logbook;
using Belikov.GenuineChannels.Messaging;
using Belikov.GenuineChannels.Parameters;
using Belikov.GenuineChannels.TransportContext;
using Belikov.GenuineChannels.Utilities;

namespace Belikov.GenuineChannels.GenuineSharedMemory;

internal class SMAcceptConnectionClosure
{
	private class AcceptConnectionInformation
	{
		public string ShareName;

		public byte ProtocolVersion;
	}

	public ITransportContext ITransportContext;

	public SharedMemoryConnection SharedMemoryConnection;

	public SharedMemoryConnectionManager SharedMemoryConnectionManager;

	public string ShareName;

	public ManualResetEvent StopListening = new ManualResetEvent(initialState: false);

	private Mutex _mutex;

	private NamedEvent _clientConnectedEvent;

	private NamedEvent _clientAcceptedEvent;

	public SMAcceptConnectionClosure(ITransportContext iTransportContext, SharedMemoryConnection sharedMemoryConnection, SharedMemoryConnectionManager sharedMemoryConnectionManager, string shareName)
	{
		ITransportContext = iTransportContext;
		SharedMemoryConnection = sharedMemoryConnection;
		SharedMemoryConnectionManager = sharedMemoryConnectionManager;
		ShareName = shareName;
	}

	public void AcceptConnections()
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		try
		{
			IParameterProvider iParameterProvider = ITransportContext.IParameterProvider;
			string name = GenuineSharedMemoryChannel.ConstructSharedObjectName("MUTEX" + ShareName, iParameterProvider);
			string name2 = GenuineSharedMemoryChannel.ConstructSharedObjectName("CC" + ShareName, iParameterProvider);
			string name3 = GenuineSharedMemoryChannel.ConstructSharedObjectName("CA" + ShareName, iParameterProvider);
			_mutex = WindowsAPI.CreateMutex(name);
			_clientConnectedEvent = NamedEvent.CreateNamedEvent(name2, initialState: false, manualReset: false);
			_clientAcceptedEvent = NamedEvent.CreateNamedEvent(name3, initialState: false, manualReset: true);
			WaitHandle[] waitHandles = new WaitHandle[2] { _clientConnectedEvent.ManualResetEvent, StopListening };
			while (true)
			{
				try
				{
					WaitHandle.WaitAny(waitHandles);
					if (StopListening.WaitOne(0, exitContext: false))
					{
						break;
					}
					int timeout = GenuineUtility.GetTimeout((TimeSpan)ITransportContext.IParameterProvider[GenuineParameter.ConnectTimeout]);
					using Stream input = SharedMemoryConnection.LowLevel_ReadSync(timeout);
					BinaryReader binaryReader = new BinaryReader(input);
					MessageCoder.DeserializeConnectionHeader(binaryReader, out var protocolVersion, out var _, out var _);
					string shareName = binaryReader.ReadString();
					_clientAcceptedEvent.ManualResetEvent.Set();
					if (binaryLogWriter != null && binaryLogWriter[LogCategory.AcceptingConnection] > 0)
					{
						binaryLogWriter.WriteEvent(LogCategory.AcceptingConnection, "SMAcceptConnectionClosure.AcceptConnections", LogMessageType.ConnectionAccepting, null, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "An inbound Shared Memory connection is being accepted.");
					}
					AcceptConnectionInformation acceptConnectionInformation = new AcceptConnectionInformation();
					acceptConnectionInformation.ShareName = shareName;
					acceptConnectionInformation.ProtocolVersion = protocolVersion;
					GenuineThreadPool.QueueUserWorkItem(AcceptConnection, acceptConnectionInformation, longDuration: true);
				}
				catch (Exception exception)
				{
					if (binaryLogWriter != null && binaryLogWriter[LogCategory.AcceptingConnection] > 0)
					{
						binaryLogWriter.WriteEvent(LogCategory.AcceptingConnection, "SMAcceptConnectionClosure.AcceptConnections", LogMessageType.ConnectionAccepting, exception, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "Can't accept a connection.");
					}
				}
			}
		}
		catch (Exception ex)
		{
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.AcceptingConnection] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.AcceptingConnection, "SMAcceptConnectionClosure.AcceptConnections", LogMessageType.CriticalError, ex, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "Critical listener failure. No connections will be accepted.");
			}
			SharedMemoryConnectionManager.ITransportContext.IGenuineEventProvider.Fire(new GenuineEventArgs(GenuineEventType.GeneralListenerFailure, ex, null, ShareName));
		}
		finally
		{
			SharedMemoryConnection.ReleaseUnmanagedResources();
			if (_mutex != null)
			{
				_mutex.Close();
			}
		}
	}

	public void AcceptConnection(object acceptConnectionInformationAsObject)
	{
		AcceptConnectionInformation acceptConnectionInformation = (AcceptConnectionInformation)acceptConnectionInformationAsObject;
		try
		{
			SharedMemoryConnectionManager.Connection_AcceptConnection(acceptConnectionInformation.ShareName, acceptConnectionInformation.ProtocolVersion);
		}
		catch (Exception ex)
		{
			ITransportContext.BinaryLogWriter.WriteEvent(LogCategory.Connection, "SMAcceptConnectionClosure.AcceptConnection", LogMessageType.ConnectionAccepting, ex, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "The connection to {0} has been refused due to the exception.", acceptConnectionInformation.ShareName);
			SharedMemoryConnectionManager.ITransportContext.IGenuineEventProvider.Fire(new GenuineEventArgs(GenuineEventType.GeneralListenerFailure, ex, null, acceptConnectionInformation.ShareName));
		}
	}
}
