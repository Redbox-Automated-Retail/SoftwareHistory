using System;
using System.Net.Sockets;
using System.Threading;
using Belikov.GenuineChannels.DotNetRemotingLayer;
using Belikov.GenuineChannels.Logbook;
using Belikov.GenuineChannels.TransportContext;
using Belikov.GenuineChannels.Utilities;

namespace Belikov.GenuineChannels.GenuineTcp;

internal class AcceptConnectionClosure
{
	public ITransportContext ITransportContext;

	public Socket Socket;

	public IAcceptConnectionConsumer IAcceptConnectionConsumer;

	public string ListeningEndPoint;

	public ManualResetEvent StopListening = new ManualResetEvent(initialState: false);

	public AcceptConnectionClosure(ITransportContext iTransportContext, string listeningEndPoint, Socket socket, IAcceptConnectionConsumer iAcceptConnectionConsumer)
	{
		ITransportContext = iTransportContext;
		ListeningEndPoint = listeningEndPoint;
		Socket = socket;
		IAcceptConnectionConsumer = iAcceptConnectionConsumer;
	}

	public void AcceptConnections()
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		Exception ex = null;
		int num = 0;
		try
		{
			while (true)
			{
				try
				{
					if (IAcceptConnectionConsumer.IsDisposed() || StopListening.WaitOne(0, exitContext: false))
					{
						break;
					}
					Socket state = Socket.Accept();
					GenuineThreadPool.QueueUserWorkItem(AcceptConnection, state, longDuration: true);
				}
				catch (Exception ex2)
				{
					num++;
					if (binaryLogWriter != null && binaryLogWriter[LogCategory.AcceptingConnection] > 0)
					{
						binaryLogWriter.WriteEvent(LogCategory.AcceptingConnection, "GenuineTcp.AcceptConnectionClosure.AcceptConnections", LogMessageType.ConnectionAccepting, ex2, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "An inbound TCP connection has not been accepted. Number of failure: {0}.", num);
					}
					ITransportContext.IGenuineEventProvider.Fire(new GenuineEventArgs(GenuineEventType.GeneralListenerFailure, ex2, null, ListeningEndPoint));
				}
			}
		}
		catch (Exception ex3)
		{
			ex = ex3;
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.AcceptingConnection] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.AcceptingConnection, "GenuineTcp.AcceptConnectionClosure.AcceptConnections", LogMessageType.ListeningStopped, ex3, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "Fatal Error. The socket does not accept inbound connections any longer.");
			}
			ITransportContext.IGenuineEventProvider.Fire(new GenuineEventArgs(GenuineEventType.GeneralListenerFailure, ex3, null, ListeningEndPoint));
		}
		finally
		{
			SocketUtility.CloseSocket(Socket);
		}
	}

	public void AcceptConnection(object socketAsObject)
	{
		Socket socket = (Socket)socketAsObject;
		IAcceptConnectionConsumer.AcceptConnection(socket);
	}
}
