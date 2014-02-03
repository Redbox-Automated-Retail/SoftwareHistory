using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Belikov.Common.ThreadProcessing;
using Belikov.GenuineChannels.Connection;
using Belikov.GenuineChannels.Messaging;
using Belikov.GenuineChannels.TransportContext;
using Belikov.GenuineChannels.Utilities;

namespace Belikov.GenuineChannels.GenuineXHttp;

internal class XHttpPhysicalConnection : PhysicalConnection
{
	public Socket Socket;

	public string EntryUri;

	public XHttpConnection XHttpConnection;

	public bool IsSender;

	public int SequenceNo = 0;

	public string LocalEndPoint;

	public ArrayList MessagesBeingSent = new ArrayList();

	public Stream SentContent;

	public byte[] AsyncSendBuffer;

	public int AsyncSendBufferSizeOfValidContent;

	public int AsyncSendBufferCurrentPosition;

	public Stream AsyncSendStream;

	public bool AsyncSendBufferIsLastPacket;

	public byte[] AsyncReceiveBuffer;

	public HostInformation Remote => XHttpConnection.Remote;

	public XHttpPhysicalConnection(XHttpConnection xHttpConnection, bool sender)
	{
		XHttpConnection = xHttpConnection;
		IsSender = sender;
		AsyncSendBuffer = new byte[35000];
		AsyncReceiveBuffer = new byte[35000];
	}

	public void CloseSocket()
	{
		lock (_accessToLocalMembers)
		{
			GenuineThreadPool.QueueUserWorkItem(SocketUtility.CloseSocket, Socket, longDuration: false);
			Socket = null;
		}
	}

	public void CheckConnectionStatus()
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException("XHttpPhysicalConnection");
		}
		ReaderAutoLocker readerAutoLocker = new ReaderAutoLocker(DisposeLock);
		try
		{
			lock (PhysicalConnectionStateLock)
			{
				if (EntryUri == null)
				{
					GenuineUtility.ParseUrl(Remote.Url, out EntryUri);
				}
				if (Socket != null && !Socket.Connected)
				{
					CloseSocket();
				}
				if (Socket == null && !XHttpConnection.IsClient)
				{
					throw GenuineExceptions.Get_Processing_TransportConnectionFailed();
				}
				if (Socket != null && LocalEndPoint == null)
				{
					LocalEndPoint = Socket.LocalEndPoint.ToString();
				}
				if (Socket != null)
				{
					return;
				}
			}
			string url = GenuineUtility.Parse(Remote.Url, out var _);
			string url2 = GenuineUtility.SplitHttpLinkToHostAndPort(url, out var port);
			IPAddress address = GenuineUtility.ResolveIPAddress(url2);
			IPEndPoint iPEndPoint = new IPEndPoint(address, port);
			if (iPEndPoint == null)
			{
				return;
			}
			Socket socket = new Socket(iPEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			LingerOption optionValue = new LingerOption(enable: true, 3);
			socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, optionValue);
			socket.Connect(iPEndPoint);
			if (!socket.Connected)
			{
				throw GenuineExceptions.Get_Connect_CanNotConnectToRemoteHost(url, "The value of the Socket.Connected property is false after connecting.");
			}
			Remote.PhysicalAddress = socket.RemoteEndPoint;
			Remote.LocalPhysicalAddress = socket.LocalEndPoint;
			if (LocalEndPoint == null)
			{
				LocalEndPoint = socket.LocalEndPoint.ToString();
			}
			lock (_accessToLocalMembers)
			{
				Socket = socket;
			}
		}
		finally
		{
			readerAutoLocker.Dispose();
		}
	}

	public override void InternalDispose(Exception reason)
	{
		if (Socket != null)
		{
			GenuineThreadPool.QueueUserWorkItem(SocketUtility.CloseSocket, Socket, longDuration: true);
			Socket = null;
		}
		lock (PhysicalConnectionStateLock)
		{
			if (SentContent != null)
			{
				SentContent.Close();
				SentContent = null;
			}
			if (MessagesBeingSent != null)
			{
				foreach (Message item in MessagesBeingSent)
				{
					XHttpConnection.ITransportContext.IIncomingStreamHandler.DispatchException(item, reason);
				}
			}
			MessagesBeingSent.Clear();
		}
	}
}
