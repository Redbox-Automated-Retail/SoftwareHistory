using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Belikov.GenuineChannels.BufferPooling;
using Belikov.GenuineChannels.Connection;
using Belikov.GenuineChannels.DotNetRemotingLayer;
using Belikov.GenuineChannels.Logbook;
using Belikov.GenuineChannels.Messaging;
using Belikov.GenuineChannels.Parameters;
using Belikov.GenuineChannels.Receiving;
using Belikov.GenuineChannels.TransportContext;
using Belikov.GenuineChannels.Utilities;

namespace Belikov.GenuineChannels.GenuineUdp;

internal class UdpConnectionManager : ConnectionManager, ITimerConsumer
{
	private const int HEADER_SIZE = 21;

	public IMessageRegistrator _iMessageRegistrator = new MessageRegistratorWithLimitedTime();

	private int _closeInvocationConnectionAfterInactivity;

	private Socket _socket;

	private object _socketLock = new object();

	private IPEndPoint _multicastTo;

	private byte[] _sendBuffer;

	private bool _closing;

	private ManualResetEvent _receivingThreadClosed = new ManualResetEvent(initialState: true);

	private Hashtable _streams = Hashtable.Synchronized(new Hashtable());

	private int _dbgConnectionId = ConnectionManager.GetUniqueConnectionId();

	public int DbgConnectionId => _dbgConnectionId;

	public UdpConnectionManager(ITransportContext iTransportContext)
		: base(iTransportContext)
	{
		Local = new HostInformation("_gudp://" + iTransportContext.HostIdentifier, iTransportContext);
		_sendBuffer = new byte[(int)iTransportContext.IParameterProvider[GenuineParameter.UdpPacketSize]];
		TimerProvider.Attach(this);
		_closeInvocationConnectionAfterInactivity = GenuineUtility.ConvertToMilliseconds(iTransportContext.IParameterProvider[GenuineParameter.CloseInvocationConnectionAfterInactivity]);
	}

	protected override void InternalSend(Message message)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		IPEndPoint iPEndPoint = ((message.Recipient.Uri == null || !message.Recipient.Uri.StartsWith("_gb")) ? (message.Recipient.PhysicalAddress as IPEndPoint) : _multicastTo);
		if (iPEndPoint == null)
		{
			try
			{
				string url = GenuineUtility.SplitToHostAndPort(message.Recipient.Url, out var port);
				iPEndPoint = (IPEndPoint)(message.Recipient.PhysicalAddress = new IPEndPoint(GenuineUtility.ResolveIPAddress(url), port));
			}
			catch (Exception)
			{
				throw GenuineExceptions.Get_Send_DestinationIsUnreachable(message.Recipient.ToString());
			}
		}
		Stream stream = message.SerializedContent;
		if ((int)ITransportContext.IParameterProvider[GenuineParameter.CompatibilityLevel] > 0)
		{
			GenuineChunkedStream genuineChunkedStream = new GenuineChunkedStream(releaseOnReadMode: false);
			BinaryWriter binaryWriter = new BinaryWriter(genuineChunkedStream);
			genuineChunkedStream.Write(ITransportContext.BinaryHostIdentifier, 0, ITransportContext.BinaryHostIdentifier.Length);
			binaryWriter.Write(message.Recipient.LocalHostUniqueIdentifier);
			binaryWriter.Write((short)0);
			genuineChunkedStream.WriteStream(stream);
			stream = genuineChunkedStream;
		}
		if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
		{
			binaryLogWriter.WriteEvent(LogCategory.Connection, "UdpConnectionManager.InternalSend", LogMessageType.MessageIsSentSynchronously, null, message, message.Recipient, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, message.ConnectionLevelSecuritySession, (message.ConnectionLevelSecuritySession == null) ? null : message.ConnectionLevelSecuritySession.Name, -1, 0, 0, 0, iPEndPoint.ToString(), null, null, null, "The message is being sent synchronously to {0}.", iPEndPoint.ToString());
		}
		byte[] src = Guid.NewGuid().ToByteArray();
		lock (_socketLock)
		{
			int num = 1;
			while (true)
			{
				int num2 = stream.Read(_sendBuffer, 21, _sendBuffer.Length - 21);
				_sendBuffer[0] = 55;
				Buffer.BlockCopy(src, 0, _sendBuffer, 1, 16);
				if (num2 < _sendBuffer.Length - 21)
				{
					num = -num;
				}
				MessageCoder.WriteInt32(_sendBuffer, 17, num);
				if (binaryLogWriter != null && binaryLogWriter[LogCategory.Transport] > 0)
				{
					binaryLogWriter.WriteTransportContentEvent(LogCategory.Transport, "UdpConnectionManager.InternalSend", LogMessageType.SynchronousSendingStarted, null, message, message.Recipient, (binaryLogWriter[LogCategory.Transport] > 1) ? new MemoryStream(GenuineUtility.CutOutBuffer(_sendBuffer, 0, num2 + 21)) : null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, DbgConnectionId, num2 + 21, iPEndPoint.ToString(), null, null, "Content is sent synchronously to {0}.", iPEndPoint.ToString());
				}
				_socket.SendTo(_sendBuffer, 0, num2 + 21, SocketFlags.None, iPEndPoint);
				if (num < 0)
				{
					break;
				}
				num++;
			}
		}
	}

	private void ReceiveSynchronously()
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		byte[] array = null;
		_receivingThreadClosed.Reset();
		try
		{
			int num = (int)ITransportContext.IParameterProvider[GenuineParameter.UdpMtu];
			while (!_closing)
			{
				array = ((3000 < num) ? new byte[num] : BufferPool.ObtainBuffer());
				try
				{
					IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, 0);
					EndPoint remoteEP = iPEndPoint;
					int num2 = _socket.ReceiveFrom(array, 0, array.Length, SocketFlags.None, ref remoteEP);
					iPEndPoint = (IPEndPoint)remoteEP;
					if (binaryLogWriter != null && binaryLogWriter[LogCategory.Transport] > 0)
					{
						binaryLogWriter.WriteTransportContentEvent(LogCategory.Transport, "UdpConnectionManager.ReceiveSynchronously", LogMessageType.ReceivingFinished, null, null, null, (binaryLogWriter[LogCategory.Transport] > 1) ? new MemoryStream(GenuineUtility.CutOutBuffer(array, 0, num2)) : null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, DbgConnectionId, num2, iPEndPoint.ToString(), null, null, "Content is received from {0}.", iPEndPoint.ToString());
					}
					if (num2 < 21)
					{
						throw GenuineExceptions.Get_Receive_IncorrectData();
					}
					if (array[0] != 55)
					{
						throw GenuineExceptions.Get_Receive_IncorrectData();
					}
					byte[] array2 = new byte[16];
					Buffer.BlockCopy(array, 1, array2, 0, 16);
					Guid guid = new Guid(array2);
					int num3 = MessageCoder.ReadInt32(array, 17);
					bool isLast = num3 < 0;
					if (num3 < 0)
					{
						num3 = -num3;
					}
					num3--;
					StreamAssembled streamAssembled;
					lock (_streams.SyncRoot)
					{
						streamAssembled = _streams[guid] as StreamAssembled;
						if (streamAssembled == null)
						{
							streamAssembled = (StreamAssembled)(_streams[guid] = new StreamAssembled(iPEndPoint, 21));
						}
						if (streamAssembled.IsProcessed)
						{
							continue;
						}
					}
					string uri = "gudp://" + iPEndPoint.ToString();
					HostInformation hostInformation = ITransportContext.KnownHosts[uri];
					hostInformation.Renew(_closeInvocationConnectionAfterInactivity, canMakeShorter: false);
					if ((int)ITransportContext.IParameterProvider[GenuineParameter.CompatibilityLevel] <= 0)
					{
						hostInformation.UpdateUri(uri, 0, checkHostInfoVersion: false);
					}
					if (streamAssembled.BufferReceived(num3, array, num2, isLast))
					{
						_streams.Remove(guid);
						streamAssembled.IsProcessed = true;
						if ((int)ITransportContext.IParameterProvider[GenuineParameter.CompatibilityLevel] > 0)
						{
							BinaryReader binaryReader = new BinaryReader(streamAssembled);
							byte[] array3 = new byte[16];
							GenuineUtility.ReadDataFromStream(streamAssembled, array3, 0, array3.Length);
							string uri2 = "_gudp://" + new Guid(array3).ToString("N");
							int remoteHostUniqueIdentifier = binaryReader.ReadInt32();
							hostInformation.UpdateUri(uri2, remoteHostUniqueIdentifier, checkHostInfoVersion: false);
							GenuineUtility.CopyStreamToStream(streamAssembled, Stream.Null, binaryReader.ReadInt16());
						}
						if (hostInformation.PhysicalAddress == null && binaryLogWriter != null && binaryLogWriter[LogCategory.HostInformation] > 0)
						{
							binaryLogWriter.WriteHostInformationEvent("UdpConnectionManager.ReceiveSynchronously", LogMessageType.HostInformationCreated, null, hostInformation, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, DbgConnectionId, "HostInformation is ready for actions.");
						}
						hostInformation.PhysicalAddress = remoteEP;
						ITransportContext.IIncomingStreamHandler.HandleMessage(streamAssembled, hostInformation, GenuineConnectionType.Persistent, string.Empty, -1, useThisThread: false, _iMessageRegistrator, null, null);
					}
				}
				catch (Exception ex)
				{
					if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
					{
						binaryLogWriter.WriteEvent(LogCategory.Connection, "UdpConnectionManager.ReceiveSynchronously", LogMessageType.ReceivingFinished, ex, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, DbgConnectionId, 0, 0, 0, null, null, null, null, "UDP socket failure.");
					}
					if (_closing)
					{
						break;
					}
					ITransportContext.IGenuineEventProvider.Fire(new GenuineEventArgs(GenuineEventType.GUdpSocketException, ex, Local, null));
				}
			}
		}
		finally
		{
			_receivingThreadClosed.Set();
		}
	}

	public override void StartListening(object endPoint)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		_closing = false;
		if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
		{
			binaryLogWriter.WriteConnectionParameterEvent(LogCategory.Connection, "UdpConnectionManager.StartListening", LogMessageType.ConnectionParameters, null, null, ITransportContext.IParameterProvider, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, DbgConnectionId, "UDP socket is being associated with the end point: {0}.", endPoint.ToString());
		}
		IPEndPoint iPEndPoint = null;
		int port;
		try
		{
			string url = (string)endPoint;
			url = GenuineUtility.SplitToHostAndPort(url, out port);
			iPEndPoint = new IPEndPoint(GenuineUtility.ResolveIPAddress(url), port);
		}
		catch (Exception exception)
		{
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Connection, "UdpConnectionManager.StartListening", LogMessageType.ListeningStarted, exception, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, DbgConnectionId, 0, 0, 0, null, null, null, null, "The listening socket cannot be associated with the {0} local end point.", endPoint.ToString());
			}
			throw GenuineExceptions.Get_Server_IncorrectAddressToListen(endPoint as string);
		}
		lock (this)
		{
			if (_socket != null)
			{
				throw GenuineExceptions.Get_Server_EndPointIsAlreadyBeingListenedTo(_socket.LocalEndPoint.ToString());
			}
			_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			if (ITransportContext.IParameterProvider[GenuineParameter.UdpTtl] != null)
			{
				_socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, (int)ITransportContext.IParameterProvider[GenuineParameter.UdpTtl]);
			}
			_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, (int)ITransportContext.IParameterProvider[GenuineParameter.UdpReceiveBuffer]);
			_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
			_socket.Bind(iPEndPoint);
			if (ITransportContext.IParameterProvider[GenuineParameter.UdpMulticastTo] != null)
			{
				try
				{
					string url = (string)ITransportContext.IParameterProvider[GenuineParameter.UdpMulticastTo];
					url = GenuineUtility.SplitToHostAndPort(url, out port);
					_multicastTo = new IPEndPoint(GenuineUtility.ResolveIPAddress(url), port);
					_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
					if (ITransportContext.IParameterProvider[GenuineParameter.UdpTtl] != null)
					{
						_socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, (int)ITransportContext.IParameterProvider[GenuineParameter.UdpTtl]);
					}
				}
				catch (Exception)
				{
					throw GenuineExceptions.Get_Channel_InvalidParameter("UdpMulticastTo");
				}
			}
			if (ITransportContext.IParameterProvider[GenuineParameter.UdpJoinTo] != null)
			{
				string url2 = (string)ITransportContext.IParameterProvider[GenuineParameter.UdpJoinTo];
				_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
				IPAddress iPAddress = GenuineUtility.ResolveIPAddress(url2);
				MulticastOption optionValue = new MulticastOption(iPAddress, iPEndPoint.Address);
				_socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, optionValue);
			}
			Thread thread = new Thread(ReceiveSynchronously);
			thread.IsBackground = true;
			thread.Start();
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Connection, "UdpConnectionManager.StartListening", LogMessageType.ConnectionEstablished, null, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, DbgConnectionId, 4, 0, 0, GetType().Name, _socket.LocalEndPoint.ToString(), null, null, "The UDP socket is ready for action.");
			}
		}
	}

	public override void StopListening(object endPoint)
	{
		lock (this)
		{
			if (_socket == null)
			{
				return;
			}
			BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Connection] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Connection, "UdpConnectionManager.StopListening", LogMessageType.ListeningStopped, null, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, DbgConnectionId, 0, 0, 0, null, null, null, null, "The listening socket is not longer associated with the {0} local end point.", (endPoint == null) ? string.Empty : endPoint.ToString());
			}
			SocketUtility.CloseSocket(_socket);
			_socket = null;
			_closing = true;
		}
		if (!_receivingThreadClosed.WaitOne(TimeSpan.FromMinutes(2.0), exitContext: false))
		{
			throw GenuineExceptions.Get_Processing_LogicError("Receiving thread didn't exit within 2 minutes.");
		}
	}

	public override void ReleaseConnections(HostInformation hostInformation, GenuineConnectionType genuineConnectionType, Exception reason)
	{
	}

	public override void InternalDispose(Exception reason)
	{
		StopListening(null);
	}

	public void TimerCallback()
	{
		int tickCount = GenuineUtility.TickCount;
		int num = GenuineUtility.ConvertToMilliseconds(ITransportContext.IParameterProvider[GenuineParameter.UdpAssembleTimeSpan]);
		lock (_streams.SyncRoot)
		{
			ArrayList arrayList = new ArrayList();
			foreach (object stream in _streams)
			{
				DictionaryEntry dictionaryEntry = (DictionaryEntry)stream;
				StreamAssembled streamAssembled = (StreamAssembled)dictionaryEntry.Value;
				if (GenuineUtility.IsTimeoutExpired(streamAssembled.Started + num, tickCount) && !streamAssembled.IsProcessed)
				{
					arrayList.Add(dictionaryEntry.Key);
					streamAssembled.Close();
				}
			}
			foreach (object item in arrayList)
			{
				_streams.Remove(item);
			}
		}
	}
}
