using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.IPC.Framework;

public abstract class BasePipeChannel : IDisposable
{
	protected bool Disposed;

	protected SafeFileHandle PipeHandle;

	protected readonly uint MessageHeaderSize;

	private readonly bool FlushOnWrite;

	public bool Connected { get; internal set; }

	public string ID { get; internal set; }

	public void Dispose()
	{
		DisposeChannel(fromDispose: true);
		GC.SuppressFinalize(this);
	}

	public abstract bool Connect();

	public bool SendMessage(byte[] message)
	{
		if (PipeHandle.IsInvalid)
		{
			return false;
		}
		uint lpNumberOfBytesWritten = 0u;
		byte[] bytes = BitConverter.GetBytes(message.Length);
		byte[] array = new byte[bytes.Length + message.Length];
		Array.Copy(bytes, array, bytes.Length);
		Array.Copy(message, 0, array, bytes.Length, message.Length);
		if (WriteFile(PipeHandle, array, (uint)array.Length, out lpNumberOfBytesWritten, IntPtr.Zero))
		{
			int lastWin32Error = Marshal.GetLastWin32Error();
			LogHelper.Instance.Log($"PipeClientSession.SafeWrite() returned false; error code = {lastWin32Error}", LogEntryType.Error);
			return false;
		}
		if (!FlushOnWrite)
		{
			return true;
		}
		return FlushFileBuffers(PipeHandle);
	}

	public byte[] ReceiveMessage()
	{
		byte[] array = ReadBytes(MessageHeaderSize);
		if (array == null)
		{
			return null;
		}
		uint count = (uint)BitConverter.ToInt32(array, 0);
		return ReadBytes(count);
	}

	protected abstract bool DisposeChannel(bool fromDispose);

	protected byte[] ReadBytes(uint count)
	{
		if (PipeHandle.IsInvalid)
		{
			return null;
		}
		uint lpNumberOfBytesRead = 0u;
		byte[] array = new byte[count];
		if (!ReadFile(PipeHandle, array, count, out lpNumberOfBytesRead, IntPtr.Zero))
		{
			int lastWin32Error = Marshal.GetLastWin32Error();
			LogHelper.Instance.Log($"PipeClientSession.ReadFile() returned false; error code = {lastWin32Error}", LogEntryType.Error);
			return null;
		}
		if (lpNumberOfBytesRead == count)
		{
			return array;
		}
		return null;
	}

	protected BasePipeChannel()
		: this(flushOnWrite: true)
	{
	}

	protected BasePipeChannel(bool flushOnWrite)
	{
		FlushOnWrite = flushOnWrite;
	}

	protected BasePipeChannel(SafeFileHandle handle, bool flushOnWrite)
	{
		PipeHandle = handle;
		MessageHeaderSize = (uint)Marshal.SizeOf(typeof(int));
		FlushOnWrite = flushOnWrite;
	}

	protected BasePipeChannel(SafeFileHandle pipeHandle)
		: this(pipeHandle, flushOnWrite: true)
	{
	}

	~BasePipeChannel()
	{
		DisposeChannel(fromDispose: false);
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	protected static extern SafeFileHandle CreateFile(string pipeName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplate);

	[DllImport("kernel32.dll", SetLastError = true)]
	protected static extern bool ReadFile(SafeFileHandle hFile, byte[] lpBuffer, uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, IntPtr lpOverlapped);

	[DllImport("kernel32.dll", SetLastError = true)]
	protected static extern bool WriteFile(SafeFileHandle hFile, byte[] lpBuffer, uint nNumberOfBytesToWrite, out uint lpNumberOfBytesWritten, IntPtr lpOverlapped);

	[DllImport("kernel32.dll", SetLastError = true)]
	protected static extern bool FlushFileBuffers(SafeFileHandle handle);
}
