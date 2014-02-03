using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Belikov.GenuineChannels.Parameters;
using Belikov.GenuineChannels.Utilities;

namespace Belikov.GenuineChannels.GenuineTcp;

internal class ConnectionEstablishingClosure : IDisposable
{
	public ManualResetEvent Completed;

	private Exception _exception;

	private int _portNumber;

	private bool _continue;

	private Socket socket;

	private string hostName;

	private IParameterProvider _parameterProvider;

	private object _syncRoot;

	public Exception Exception
	{
		get
		{
			lock (_syncRoot)
			{
				return _exception;
			}
		}
		set
		{
			lock (_syncRoot)
			{
				_exception = value;
			}
		}
	}

	public ConnectionEstablishingClosure(string __hostName, int portNumber, IParameterProvider parameterProvider)
	{
		hostName = __hostName;
		_portNumber = portNumber;
		_parameterProvider = parameterProvider;
		Completed = new ManualResetEvent(initialState: false);
		_continue = true;
		_syncRoot = new object();
		socket = null;
		_exception = null;
	}

	public void StartOperation()
	{
		GenuineThreadPool.QueueUserWorkItem(StartConnecting, null, longDuration: true);
	}

	private void StartConnecting(object ignored)
	{
		try
		{
			lock (_syncRoot)
			{
				if (!_continue)
				{
					return;
				}
			}
			IPAddress iPAddress = GenuineUtility.ResolveIPAddress(hostName);
			if (iPAddress == null)
			{
				throw GenuineExceptions.Get_Connect_CanNotResolveHostName(hostName);
			}
			IPEndPoint iPEndPoint = new IPEndPoint(iPAddress, _portNumber);
			if (iPEndPoint == null)
			{
				throw GenuineExceptions.Get_Connect_CanNotConnectToRemoteHost(hostName, "The IPEndPoint instance cannot be created.");
			}
			socket = new Socket(iPEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			LingerOption optionValue = new LingerOption(enable: true, 3);
			socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, optionValue);
			if ((bool)_parameterProvider[GenuineParameter.TcpDisableNagling])
			{
				socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Debug, 1);
			}
			int num = (int)_parameterProvider[GenuineParameter.TcpReceiveBufferSize];
			int num2 = (int)_parameterProvider[GenuineParameter.TcpSendBufferSize];
			if (num >= 0)
			{
				socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, num);
			}
			if (num2 >= 0)
			{
				socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, num2);
			}
			lock (_syncRoot)
			{
				if (!_continue)
				{
					return;
				}
			}
			socket.Connect(iPEndPoint);
			Completed.Set();
		}
		catch (Exception reason)
		{
			CancelConnecting(reason);
		}
	}

	public Socket CompleteOperation()
	{
		lock (_syncRoot)
		{
			Socket result = socket;
			socket = null;
			return result;
		}
	}

	public void CancelConnecting(Exception reason)
	{
		lock (_syncRoot)
		{
			_continue = false;
			Exception = reason;
			Completed.Set();
		}
	}

	public void Dispose()
	{
		try
		{
			lock (_syncRoot)
			{
				if (socket != null)
				{
					GenuineThreadPool.QueueUserWorkItem(SocketUtility.CloseSocket, socket, longDuration: true);
					socket = null;
				}
			}
		}
		catch
		{
		}
	}
}
