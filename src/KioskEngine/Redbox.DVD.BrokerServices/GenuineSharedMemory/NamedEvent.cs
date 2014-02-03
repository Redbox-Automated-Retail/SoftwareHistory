using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Belikov.GenuineChannels.GenuineSharedMemory;

internal class NamedEvent
{
	private ManualResetEvent _manualResetEvent;

	public ManualResetEvent ManualResetEvent => _manualResetEvent;

	private NamedEvent(IntPtr handle)
	{
		_manualResetEvent = new ManualResetEvent(initialState: false);
		_manualResetEvent.Handle = handle;
	}

	public static NamedEvent CreateNamedEvent(string name, bool initialState, bool manualReset)
	{
		if (WindowsAPI.FailureReason != null)
		{
			throw OperationException.WrapException(WindowsAPI.FailureReason);
		}
		IntPtr intPtr = WindowsAPI.CreateEvent(WindowsAPI.AttributesWithNullDACL, manualReset ? 1 : 0, initialState ? 1 : 0, name);
		if (intPtr == IntPtr.Zero)
		{
			throw GenuineExceptions.Get_Windows_CanNotCreateOrOpenNamedEvent(Marshal.GetLastWin32Error());
		}
		return new NamedEvent(intPtr);
	}

	public static NamedEvent OpenNamedEvent(string name)
	{
		IntPtr intPtr = WindowsAPI.OpenEvent(2031619, 0, name);
		if (intPtr == IntPtr.Zero)
		{
			throw GenuineExceptions.Get_Windows_CanNotCreateOrOpenNamedEvent(Marshal.GetLastWin32Error());
		}
		return new NamedEvent(intPtr);
	}
}
