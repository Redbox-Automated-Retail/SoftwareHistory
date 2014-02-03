using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Belikov.GenuineChannels.Connection;
using Belikov.GenuineChannels.DotNetRemotingLayer;
using Belikov.GenuineChannels.Logbook;
using Belikov.GenuineChannels.Messaging;
using Belikov.GenuineChannels.Parameters;
using Belikov.GenuineChannels.Security;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.GenuineSharedMemory;

internal class SharedMemoryConnection : Stream, IDisposable
{
	private const int MIN_SHARE_SIZE = 20000;

	private const int MAX_SHARE_SIZE = 2000000;

	private const int SHARE_CONTENT_OFFSET = 8;

	private object _accessToLocalMembers = new object();

	private bool _setCloseStatusOnExit;

	public string ShareName;

	public ITransportContext ITransportContext;

	public bool IsServer;

	private object _isValidLock = new object();

	private bool _isValid = true;

	public SecuritySession ConnectionLevelSecurity;

	public int DbgConnectionId = ConnectionManager.GetUniqueConnectionId();

	public HostInformation Remote;

	public object WriteAccess = new object();

	private int _shareSize;

	private IntPtr _mapHandle = IntPtr.Zero;

	private IntPtr _pointer = IntPtr.Zero;

	internal NamedEvent _namedEventReadCompleted;

	private NamedEvent _namedEventRemoteReadCompleted;

	private NamedEvent _namedEventWriteCompleted;

	private NamedEvent _namedEventRemoteWriteCompleted;

	private int _receiveOffset;

	private int _receiveSpaceSize;

	private int _sendOffset;

	private int _sendSpaceSize;

	private byte[] _sendBuffer;

	private int _readDeadline;

	private int _validLength;

	private int _currentPosition;

	private bool _messageRead;

	private ConnectionStateSignaller _connectionStateSignaller;

	private object _connectionStateSignallerLock = new object();

	internal int _pingTimeOut;

	internal int _closeAfterInactivity;

	private int _lastTimeAMessageWasSent;

	public bool IsValid
	{
		get
		{
			return _isValid;
		}
		set
		{
			_isValid = value;
		}
	}

	private byte _closed
	{
		get
		{
			return Marshal.ReadByte(_pointer);
		}
		set
		{
			Marshal.WriteByte(_pointer, value);
		}
	}

	private int _writtenShareSize
	{
		get
		{
			return Marshal.ReadInt32(_pointer, 1);
		}
		set
		{
			Marshal.WriteInt32(_pointer, 1, value);
		}
	}

	private int _MessageToSendTotalSize
	{
		set
		{
			Marshal.WriteInt32(_pointer, _sendOffset, value);
		}
	}

	private int _MessageToSendFinishFlag
	{
		set
		{
			Marshal.WriteInt32(_pointer, _sendOffset + 4, value);
		}
	}

	private int _MessageToReceiveTotalSize => Marshal.ReadInt32(_pointer, _receiveOffset);

	private int _MessageToReceiveFinishFlag => Marshal.ReadInt32(_pointer, _receiveOffset + 4);

	public override bool CanRead => true;

	public override bool CanSeek => false;

	public override bool CanWrite => false;

	public override long Length
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public override long Position
	{
		get
		{
			throw new NotSupportedException();
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public int LastTimeAMessageWasSent
	{
		get
		{
			lock (_accessToLocalMembers)
			{
				return _lastTimeAMessageWasSent;
			}
		}
	}

	internal SharedMemoryConnection(ITransportContext iTransportContext, string name, bool isServer, bool setCloseStatusOnExit)
	{
		ITransportContext = iTransportContext;
		ShareName = "GenuineChannels_GShMem_" + name;
		IsServer = isServer;
		_setCloseStatusOnExit = setCloseStatusOnExit;
		_shareSize = (int)iTransportContext.IParameterProvider[GenuineParameter.SMShareSize];
		_pingTimeOut = GenuineUtility.ConvertToMilliseconds(iTransportContext.IParameterProvider[GenuineParameter.PersistentConnectionSendPingAfterInactivity]);
		_closeAfterInactivity = GenuineUtility.ConvertToMilliseconds(iTransportContext.IParameterProvider[GenuineParameter.ClosePersistentConnectionAfterInactivity]);
		string text = (isServer ? "Server" : "Client");
		string text2 = (isServer ? "Client" : "Server");
		IParameterProvider iParameterProvider = ITransportContext.IParameterProvider;
		string name2 = GenuineSharedMemoryChannel.ConstructSharedObjectName(ShareName + text + "ReadCompleted", iParameterProvider);
		string name3 = GenuineSharedMemoryChannel.ConstructSharedObjectName(ShareName + text + "WriteCompleted", iParameterProvider);
		string name4 = GenuineSharedMemoryChannel.ConstructSharedObjectName(ShareName + text2 + "ReadCompleted", iParameterProvider);
		string name5 = GenuineSharedMemoryChannel.ConstructSharedObjectName(ShareName + text2 + "WriteCompleted", iParameterProvider);
		if (isServer)
		{
			if (_shareSize < 20000 || _shareSize > 2000000)
			{
				throw GenuineExceptions.Get_Channel_InvalidParameter("SMShareSize");
			}
			LowLevel_CreateSharedMemory();
			_closed = 0;
			_writtenShareSize = _shareSize;
			_receiveOffset = 5;
			_sendOffset = (_shareSize - 5) / 2;
			_receiveSpaceSize = _sendOffset - 5 - 8;
			_sendSpaceSize = _shareSize - _sendOffset - 8;
			_namedEventReadCompleted = NamedEvent.CreateNamedEvent(name2, initialState: false, manualReset: true);
			_namedEventWriteCompleted = NamedEvent.CreateNamedEvent(name3, initialState: false, manualReset: true);
			_namedEventRemoteReadCompleted = NamedEvent.CreateNamedEvent(name4, initialState: false, manualReset: true);
			_namedEventRemoteWriteCompleted = NamedEvent.CreateNamedEvent(name5, initialState: false, manualReset: true);
		}
		else
		{
			OpenSharedMemory();
			if (_closed != 0)
			{
				throw GenuineExceptions.Get_Connect_CanNotConnectToRemoteHost(name, "Remote host has already closed the connection.");
			}
			_shareSize = _writtenShareSize;
			if (_shareSize < 20000 || _shareSize > 2000000)
			{
				throw GenuineExceptions.Get_Channel_InvalidParameter("SMShareSize");
			}
			_receiveOffset = (_shareSize - 5) / 2;
			_sendOffset = 5;
			_receiveSpaceSize = _shareSize - _receiveOffset - 8;
			_sendSpaceSize = _receiveOffset - 5 - 8;
			_namedEventReadCompleted = NamedEvent.OpenNamedEvent(name2);
			_namedEventWriteCompleted = NamedEvent.OpenNamedEvent(name3);
			_namedEventRemoteReadCompleted = NamedEvent.OpenNamedEvent(name4);
			_namedEventRemoteWriteCompleted = NamedEvent.OpenNamedEvent(name5);
		}
		_sendBuffer = new byte[_sendSpaceSize];
	}

	~SharedMemoryConnection()
	{
		ReleaseUnmanagedResources();
	}

	public void ReleaseUnmanagedResources()
	{
		if (_pointer != IntPtr.Zero)
		{
			if (_setCloseStatusOnExit)
			{
				_closed = 1;
			}
			WindowsAPI.UnmapViewOfFile(_pointer);
			WindowsAPI.CloseHandle(_mapHandle);
			_pointer = IntPtr.Zero;
			_mapHandle = IntPtr.Zero;
		}
	}

	public void LowLevel_CreateSharedMemory()
	{
		if (WindowsAPI.FailureReason != null)
		{
			throw OperationException.WrapException(WindowsAPI.FailureReason);
		}
		IParameterProvider iParameterProvider = ITransportContext.IParameterProvider;
		string lpName = GenuineSharedMemoryChannel.ConstructSharedObjectName(ShareName, iParameterProvider);
		_mapHandle = WindowsAPI.CreateFileMapping((IntPtr)(-1), WindowsAPI.AttributesWithNullDACL, 4u, 0u, (uint)_shareSize, lpName);
		if (_mapHandle == IntPtr.Zero)
		{
			throw GenuineExceptions.Get_Windows_CanNotCreateOrOpenSharedMemory(Marshal.GetLastWin32Error());
		}
		_pointer = WindowsAPI.MapViewOfFile(_mapHandle, 6u, 0u, 0u, 0u);
		if (_pointer == IntPtr.Zero)
		{
			int lastWin32Error = Marshal.GetLastWin32Error();
			WindowsAPI.CloseHandle(_mapHandle);
			throw GenuineExceptions.Get_Windows_SharedMemoryError(lastWin32Error);
		}
	}

	public void OpenSharedMemory()
	{
		IParameterProvider iParameterProvider = ITransportContext.IParameterProvider;
		string lpName = GenuineSharedMemoryChannel.ConstructSharedObjectName(ShareName, iParameterProvider);
		_mapHandle = WindowsAPI.OpenFileMapping(31u, 0, lpName);
		if (_mapHandle == IntPtr.Zero)
		{
			throw GenuineExceptions.Get_Windows_CanNotCreateOrOpenSharedMemory(Marshal.GetLastWin32Error());
		}
		_pointer = WindowsAPI.MapViewOfFile(_mapHandle, 6u, 0u, 0u, 0u);
		if (_pointer == IntPtr.Zero)
		{
			int lastWin32Error = Marshal.GetLastWin32Error();
			WindowsAPI.CloseHandle(_mapHandle);
			throw GenuineExceptions.Get_Windows_SharedMemoryError(lastWin32Error);
		}
	}

	public void LowLevel_SendSync(Stream content, int timeout)
	{
		if (ConnectionLevelSecurity != null)
		{
			GenuineChunkedStream genuineChunkedStream = new GenuineChunkedStream(releaseOnReadMode: true);
			ConnectionLevelSecurity.Encrypt(content, genuineChunkedStream);
			content = genuineChunkedStream;
		}
		int num;
		do
		{
			if (!IsValid)
			{
				throw GenuineExceptions.Get_Processing_TransportConnectionFailed();
			}
			if (!GenuineUtility.WaitOne(_namedEventRemoteReadCompleted.ManualResetEvent, GenuineUtility.GetMillisecondsLeft(timeout)))
			{
				throw GenuineExceptions.Get_Send_Timeout();
			}
			if (_closed != 0)
			{
				throw GenuineExceptions.Get_Receive_ConnectionClosed();
			}
			_namedEventRemoteReadCompleted.ManualResetEvent.Reset();
			num = content.Read(_sendBuffer, 0, _sendSpaceSize);
			BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.LowLevelTransport] > 0)
			{
				binaryLogWriter.WriteTransportContentEvent(LogCategory.LowLevelTransport, "GenuineSaredMemoryConnection.LowLevel_SendSync", LogMessageType.LowLevelTransport_AsyncSendingInitiating, null, null, Remote, (binaryLogWriter[LogCategory.LowLevelTransport] > 1) ? new MemoryStream(_sendBuffer, 0, num) : null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, DbgConnectionId, num, null, null, null, "Content sending.");
			}
			Marshal.Copy(_sendBuffer, 0, (IntPtr)(_pointer.ToInt32() + _sendOffset + 8), num);
			_MessageToSendTotalSize = num;
			_MessageToSendFinishFlag = ((num < _sendSpaceSize) ? 1 : 0);
			_namedEventWriteCompleted.ManualResetEvent.Set();
		}
		while (num >= _sendSpaceSize);
		UpdateLastTimeAMessageWasSent();
	}

	public Stream LowLevel_ReadSync(int finishTime)
	{
		if (!IsValid)
		{
			throw GenuineExceptions.Get_Processing_TransportConnectionFailed();
		}
		_currentPosition = _validLength;
		_messageRead = false;
		_readDeadline = finishTime;
		if (ConnectionLevelSecurity != null)
		{
			return ConnectionLevelSecurity.Decrypt(this);
		}
		return this;
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		int num = 0;
		int num2 = 0;
		while (true)
		{
			if (!IsValid)
			{
				throw GenuineExceptions.Get_Processing_TransportConnectionFailed();
			}
			if (_currentPosition < _validLength)
			{
				num = Math.Min(_validLength - _currentPosition, count);
				Marshal.Copy((IntPtr)(_pointer.ToInt32() + _receiveOffset + _currentPosition + 8), buffer, offset, num);
				_currentPosition += num;
				count -= num;
				num2 += num;
				offset += num;
			}
			if (count <= 0 || _messageRead)
			{
				break;
			}
			ReadNextPortion();
		}
		return num2;
	}

	public override int ReadByte()
	{
		if (_currentPosition < _validLength)
		{
			return Marshal.ReadByte(_pointer, _receiveOffset + _currentPosition++ + 8);
		}
		ReadNextPortion();
		if (_currentPosition < _validLength)
		{
			return Marshal.ReadByte(_pointer, _receiveOffset + _currentPosition++ + 8);
		}
		return -1;
	}

	private void ReadNextPortion()
	{
		if (!IsValid)
		{
			throw GenuineExceptions.Get_Processing_TransportConnectionFailed();
		}
		_namedEventReadCompleted.ManualResetEvent.Set();
		if (!GenuineUtility.WaitOne(_namedEventRemoteWriteCompleted.ManualResetEvent, GenuineUtility.GetMillisecondsLeft(_readDeadline)))
		{
			throw GenuineExceptions.Get_Send_Timeout();
		}
		_namedEventRemoteWriteCompleted.ManualResetEvent.Reset();
		_validLength = _MessageToReceiveTotalSize;
		_currentPosition = 0;
		_messageRead = _MessageToReceiveFinishFlag != 0;
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		if (binaryLogWriter != null && binaryLogWriter[LogCategory.LowLevelTransport] > 0)
		{
			bool flag = binaryLogWriter[LogCategory.LowLevelTransport] > 1;
			byte[] array = new byte[_validLength];
			Marshal.Copy((IntPtr)(_pointer.ToInt32() + _receiveOffset + _currentPosition + 8), array, 0, _validLength);
			binaryLogWriter.WriteTransportContentEvent(LogCategory.LowLevelTransport, "GenuineSaredMemoryConnection.ReadNextPortion", LogMessageType.LowLevelTransport_SyncReceivingCompleted, null, null, Remote, flag ? new MemoryStream(array, 0, _validLength) : null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, DbgConnectionId, _validLength, null, null, null, "Content received.");
		}
	}

	public override void Close()
	{
		SkipMessage();
	}

	public void SkipMessage()
	{
		while (!_messageRead)
		{
			ReadNextPortion();
		}
		_validLength = 0;
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		throw new NotSupportedException();
	}

	public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
	{
		throw new NotSupportedException();
	}

	public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
	{
		throw new NotSupportedException();
	}

	public override int EndRead(IAsyncResult asyncResult)
	{
		throw new NotSupportedException();
	}

	public override void EndWrite(IAsyncResult asyncResult)
	{
		throw new NotSupportedException();
	}

	public override void Flush()
	{
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotSupportedException();
	}

	public override void SetLength(long val)
	{
		throw new NotSupportedException();
	}

	public void SignalState(GenuineEventType genuineEventType, Exception reason, object additionalInfo)
	{
		lock (_connectionStateSignallerLock)
		{
			if (_connectionStateSignaller == null)
			{
				_connectionStateSignaller = new ConnectionStateSignaller(Remote, ITransportContext.IGenuineEventProvider);
			}
			_connectionStateSignaller.SetState(genuineEventType, reason, additionalInfo);
		}
	}

	public void Renew()
	{
		Remote.Renew(_closeAfterInactivity, canMakeShorter: true);
	}

	public void UpdateLastTimeAMessageWasSent()
	{
		lock (_accessToLocalMembers)
		{
			_lastTimeAMessageWasSent = GenuineUtility.TickCount;
		}
	}
}
