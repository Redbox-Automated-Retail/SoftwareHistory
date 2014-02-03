using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Redbox.HAL.Script.Framework;

internal class ProcessStarter : IDisposable
{
	public struct SECURITY_ATTRIBUTES
	{
		public int nLength;

		public IntPtr lpSecurityDescriptor;

		public int bInheritHandle;
	}

	public enum SECURITY_IMPERSONATION_LEVEL
	{
		SecurityAnonymous,
		SecurityIdentification,
		SecurityImpersonation,
		SecurityDelegation
	}

	public enum TOKEN_TYPE
	{
		TokenPrimary = 1,
		TokenImpersonation
	}

	public enum WTS_CONNECTSTATE_CLASS
	{
		WTSActive,
		WTSConnected,
		WTSConnectQuery,
		WTSShadow,
		WTSDisconnected,
		WTSIdle,
		WTSListen,
		WTSReset,
		WTSDown,
		WTSInit
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct STARTUPINFO
	{
		public int cb;

		public string lpReserved;

		public string lpDesktop;

		public string lpTitle;

		public int dwX;

		public int dwY;

		public int dwXSize;

		public int dwYSize;

		public int dwXCountChars;

		public int dwYCountChars;

		public int dwFillAttribute;

		public int dwFlags;

		public short wShowWindow;

		public short cbReserved2;

		public IntPtr lpReserved2;

		public IntPtr hStdInput;

		public IntPtr hStdOutput;

		public IntPtr hStdError;
	}

	internal struct PROCESS_INFORMATION
	{
		public IntPtr hProcess;

		public IntPtr hThread;

		public int dwProcessId;

		public int dwThreadId;
	}

	private struct WTS_SESSION_INFO
	{
		public int SessionID;

		[MarshalAs(UnmanagedType.LPStr)]
		public string pWinStationName;

		public WTS_CONNECTSTATE_CLASS State;
	}

	private const uint STANDARD_RIGHTS_REQUIRED = 983040u;

	private const uint STANDARD_RIGHTS_READ = 131072u;

	private const uint TOKEN_ASSIGN_PRIMARY = 1u;

	private const uint TOKEN_DUPLICATE = 2u;

	private const uint TOKEN_IMPERSONATE = 4u;

	private const uint TOKEN_QUERY = 8u;

	private const uint TOKEN_QUERY_SOURCE = 16u;

	private const uint TOKEN_ADJUST_PRIVILEGES = 32u;

	private const uint TOKEN_ADJUST_GROUPS = 64u;

	private const uint TOKEN_ADJUST_DEFAULT = 128u;

	private const uint TOKEN_ADJUST_SESSIONID = 256u;

	private const uint TOKEN_READ = 131080u;

	private const uint TOKEN_ALL_ACCESS = 983551u;

	private const uint NORMAL_PRIORITY_CLASS = 32u;

	private const uint CREATE_UNICODE_ENVIRONMENT = 1024u;

	private const uint MAX_PATH = 260u;

	private const uint CREATE_NO_WINDOW = 134217728u;

	private const uint INFINITE = uint.MaxValue;

	private PROCESS_INFORMATION m_processInfo;

	private readonly string Arguments;

	private readonly string FullPath;

	private readonly string ProcessName;

	public void Dispose()
	{
	}

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern uint WTSGetActiveConsoleSessionId();

	[DllImport("wtsapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern bool WTSQueryUserToken(int sessionId, out IntPtr tokenHandle);

	[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	public static extern bool DuplicateTokenEx(IntPtr existingToken, uint desiredAccess, IntPtr tokenAttributes, SECURITY_IMPERSONATION_LEVEL impersonationLevel, TOKEN_TYPE tokenType, out IntPtr newToken);

	[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern bool CreateProcessAsUser(IntPtr token, string applicationName, string commandLine, ref SECURITY_ATTRIBUTES processAttributes, ref SECURITY_ATTRIBUTES threadAttributes, bool inheritHandles, uint creationFlags, IntPtr environment, string currentDirectory, ref STARTUPINFO startupInfo, out PROCESS_INFORMATION processInformation);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern bool CloseHandle(IntPtr handle);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern int GetLastError();

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern int WaitForSingleObject(IntPtr token, uint timeInterval);

	[DllImport("wtsapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern int WTSEnumerateSessions(IntPtr hServer, int Reserved, int Version, ref IntPtr ppSessionInfo, ref int pCount);

	[DllImport("userenv.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern bool CreateEnvironmentBlock(out IntPtr lpEnvironment, IntPtr hToken, bool bInherit);

	[DllImport("wtsapi32.dll", ExactSpelling = true)]
	public static extern void WTSFreeMemory(IntPtr memory);

	[DllImport("userenv.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool DestroyEnvironmentBlock(IntPtr lpEnvironment);

	internal ProcessStarter(string processName, string fullExeName, string arguments)
	{
		Arguments = arguments;
		FullPath = fullExeName;
		ProcessName = processName;
	}

	internal void Run()
	{
		IntPtr currentUserToken = GetCurrentUserToken();
		if (!(currentUserToken == IntPtr.Zero))
		{
			STARTUPINFO startupInfo = default(STARTUPINFO);
			m_processInfo = default(PROCESS_INFORMATION);
			startupInfo.cb = Marshal.SizeOf(startupInfo);
			SECURITY_ATTRIBUTES processAttributes = default(SECURITY_ATTRIBUTES);
			SECURITY_ATTRIBUTES threadAttributes = default(SECURITY_ATTRIBUTES);
			string text = "\"" + FullPath + "\"";
			if (Arguments != null && Arguments.Length != 0)
			{
				text = text + " " + Arguments;
			}
			IntPtr lpEnvironment = IntPtr.Zero;
			if (!CreateEnvironmentBlock(out lpEnvironment, currentUserToken, bInherit: false))
			{
				GetLastError();
			}
			CreateProcessAsUser(currentUserToken, null, text, ref processAttributes, ref threadAttributes, inheritHandles: false, 134218784u, lpEnvironment, null, ref startupInfo, out m_processInfo);
			DestroyEnvironmentBlock(lpEnvironment);
			CloseHandle(currentUserToken);
		}
	}

	public void Stop()
	{
		Process[] processes = Process.GetProcesses();
		Process[] array = processes;
		foreach (Process process in array)
		{
			if (process.ProcessName == ProcessName)
			{
				process.Kill();
			}
		}
	}

	public int WaitForExit()
	{
		WaitForSingleObject(m_processInfo.hProcess, uint.MaxValue);
		return GetLastError();
	}

	private IntPtr GetCurrentUserToken()
	{
		IntPtr tokenHandle = IntPtr.Zero;
		IntPtr newToken = IntPtr.Zero;
		IntPtr zero = IntPtr.Zero;
		int sessionId = 0;
		_ = IntPtr.Zero;
		_ = IntPtr.Zero;
		IntPtr ppSessionInfo = IntPtr.Zero;
		int pCount = 0;
		WTSEnumerateSessions(zero, 0, 1, ref ppSessionInfo, ref pCount);
		int num = Marshal.SizeOf(typeof(WTS_SESSION_INFO));
		int num2 = (int)ppSessionInfo;
		for (int i = 0; i < pCount; i++)
		{
			WTS_SESSION_INFO wTS_SESSION_INFO = (WTS_SESSION_INFO)Marshal.PtrToStructure((IntPtr)num2, typeof(WTS_SESSION_INFO));
			if (wTS_SESSION_INFO.State == WTS_CONNECTSTATE_CLASS.WTSActive)
			{
				sessionId = wTS_SESSION_INFO.SessionID;
				break;
			}
			num2 += num;
		}
		WTSFreeMemory(ppSessionInfo);
		if (!WTSQueryUserToken(sessionId, out tokenHandle))
		{
			return IntPtr.Zero;
		}
		if (!DuplicateTokenEx(tokenHandle, 983551u, IntPtr.Zero, SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation, TOKEN_TYPE.TokenPrimary, out newToken))
		{
			return IntPtr.Zero;
		}
		return newToken;
	}
}
