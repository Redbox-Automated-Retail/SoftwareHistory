using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Belikov.GenuineChannels.GenuineSharedMemory;

[ComVisible(true)]
public class WindowsAPI
{
	[StructLayout(LayoutKind.Sequential)]
	internal class _SECURITY_ATTRIBUTES
	{
		public int nLength;

		public IntPtr lpSecurityDescriptor;

		public int bInheritHandle;
	}

	internal const int PAGE_READWRITE = 4;

	internal const uint SECTION_QUERY = 1u;

	internal const uint SECTION_MAP_WRITE = 2u;

	internal const uint SECTION_MAP_READ = 4u;

	internal const uint SECTION_MAP_EXECUTE = 8u;

	internal const uint SECTION_EXTEND_SIZE = 16u;

	internal const uint FILE_MAP_ALL_ACCESS = 31u;

	internal const uint FORMAT_MESSAGE_ALLOCATE_BUFFER = 256u;

	internal const uint FORMAT_MESSAGE_IGNORE_INSERTS = 512u;

	internal const uint FORMAT_MESSAGE_FROM_STRING = 1024u;

	internal const uint FORMAT_MESSAGE_FROM_HMODULE = 2048u;

	internal const uint FORMAT_MESSAGE_FROM_SYSTEM = 4096u;

	internal const uint FORMAT_MESSAGE_ARGUMENT_ARRAY = 8192u;

	internal const uint FORMAT_MESSAGE_MAX_WIDTH_MASK = 255u;

	internal const uint WAIT_TIMEOUT = 258u;

	internal const int INFINITE = -1;

	internal const int SYNCHRONIZE = 1048576;

	internal const int STANDARD_RIGHTS_REQUIRED = 983040;

	internal const int EVENT_ALL_ACCESS = 2031619;

	private const int SECURITY_DESCRIPTOR_MIN_LENGTH = 20;

	private const int SECURITY_DESCRIPTOR_REVISION = 1;

	private const int MUTEX_ALL_ACCESS = 2031617;

	private const int MUTEX_MODIFY_STATE = 1;

	private const int ERROR_ALREADY_EXISTS = 183;

	private static IntPtr NullAttribute;

	internal static _SECURITY_ATTRIBUTES AttributesWithNullDACL;

	internal static Exception FailureReason;

	[DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
	internal static extern IntPtr CreateFileMapping(IntPtr hFile, _SECURITY_ATTRIBUTES lpSecurityAttributes, uint flProtect, uint dwMaximumSizeHigh, uint dwMaximumSizeLow, string lpName);

	[DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
	internal static extern IntPtr OpenFileMapping(uint dwDesiredAccess, int bInheritHandle, string lpName);

	[DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
	internal static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject, uint dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow, uint dwNumberOfBytesToMap);

	[DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
	internal static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);

	[DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
	internal static extern bool CloseHandle(IntPtr hObject);

	[DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
	internal static extern int FormatMessage(uint dwFlags, IntPtr lpSource, uint dwMessageId, uint dwLanguageId, IntPtr lpBuffer, uint nSize, IntPtr pArguments);

	[DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
	internal static extern IntPtr LocalFree(IntPtr hMem);

	[DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
	internal static extern IntPtr CreateEvent(_SECURITY_ATTRIBUTES lpEventAttributes, int bManualReset, int bInitialState, string lpName);

	[DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
	internal static extern bool SetEvent(IntPtr hEvent);

	[DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
	internal static extern bool ResetEvent(IntPtr hEvent);

	[DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
	internal static extern uint WaitForSingleObject(IntPtr hHandle, int dwMilliseconds);

	[DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
	internal static extern IntPtr OpenEvent(int dwDesiredAccess, int bInheritHandle, string lpName);

	[DllImport("Advapi32", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern bool InitializeSecurityDescriptor(IntPtr pSecurityDescriptor, int dwRevision);

	[DllImport("Advapi32", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern bool SetSecurityDescriptorDacl(IntPtr pSecurityDescriptor, bool bDaclPresent, IntPtr pDacl, bool bDaclDefaulted);

	[DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr CreateMutex(_SECURITY_ATTRIBUTES lpMutexAttributes, bool bInitialOwner, string lpName);

	[DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr OpenMutex(int dwDesiredAccess, bool bInheritHandle, string lpName);

	static WindowsAPI()
	{
		NullAttribute = IntPtr.Zero;
		NullAttribute = Marshal.AllocHGlobal(20);
		if (NullAttribute == IntPtr.Zero)
		{
			return;
		}
		try
		{
			if (!InitializeSecurityDescriptor(NullAttribute, 1))
			{
				throw GenuineExceptions.Get_Windows_SharedMemoryError(Marshal.GetLastWin32Error());
			}
			if (!SetSecurityDescriptorDacl(NullAttribute, bDaclPresent: true, IntPtr.Zero, bDaclDefaulted: false))
			{
				throw GenuineExceptions.Get_Windows_SharedMemoryError(Marshal.GetLastWin32Error());
			}
			AttributesWithNullDACL = new _SECURITY_ATTRIBUTES();
			AttributesWithNullDACL.nLength = 12;
			AttributesWithNullDACL.lpSecurityDescriptor = NullAttribute;
			AttributesWithNullDACL.bInheritHandle = 1;
		}
		catch (Exception failureReason)
		{
			FailureReason = failureReason;
			try
			{
				Marshal.FreeHGlobal(NullAttribute);
			}
			catch (Exception)
			{
			}
			NullAttribute = IntPtr.Zero;
		}
	}

	internal static Mutex OpenMutex(string name)
	{
		IntPtr intPtr = OpenMutex(2031617, bInheritHandle: false, name);
		if (intPtr == IntPtr.Zero)
		{
			throw GenuineExceptions.Get_Windows_SharedMemoryError(Marshal.GetLastWin32Error());
		}
		Mutex mutex = new Mutex();
		mutex.Handle = intPtr;
		return mutex;
	}

	internal static Mutex CreateMutex(string name)
	{
		IntPtr intPtr = CreateMutex(AttributesWithNullDACL, bInitialOwner: false, name);
		if (intPtr == IntPtr.Zero)
		{
			throw GenuineExceptions.Get_Windows_SharedMemoryError(Marshal.GetLastWin32Error());
		}
		if (Marshal.GetLastWin32Error() == 183)
		{
			throw GenuineExceptions.Get_Windows_SharedMemoryError(Marshal.GetLastWin32Error());
		}
		Mutex mutex = new Mutex();
		mutex.Handle = intPtr;
		return mutex;
	}
}
