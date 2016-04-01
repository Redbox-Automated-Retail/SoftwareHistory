using System;
using System.Collections.Specialized;
using System.IO;
using System.Runtime.InteropServices;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core;

internal sealed class RuntimeService : IRuntimeService
{
	private struct SYSTEM_INFO
	{
		internal _PROCESSOR_INFO_UNION uProcessorInfo;

		public uint dwPageSize;

		public IntPtr lpMinimumApplicationAddress;

		public IntPtr lpMaximumApplicationAddress;

		public IntPtr dwActiveProcessorMask;

		public uint dwNumberOfProcessors;

		public uint dwProcessorType;

		public uint dwAllocationGranularity;

		public ushort dwProcessorLevel;

		public ushort dwProcessorRevision;
	}

	[StructLayout(LayoutKind.Explicit)]
	private struct _PROCESSOR_INFO_UNION
	{
		[FieldOffset(0)]
		internal uint dwOemId;

		[FieldOffset(0)]
		internal ushort wProcessorArchitecture;

		[FieldOffset(2)]
		internal ushort wReserved;
	}

	public const string RunningPath = "RunningPath";

	private readonly StringDictionary Values = new StringDictionary();

	private static RuntimeService m_instance = new RuntimeService();

	public int PageSize { get; private set; }

	public string AssemblyDirectory { get; private set; }

	public string InstallRoot { get; private set; }

	private Platform Platform { get; set; }

	public string RuntimePath(string fileName)
	{
		return Path.Combine(AssemblyDirectory, fileName);
	}

	public string InstallPath(string subfolder)
	{
		return Path.Combine(InstallRoot, subfolder);
	}

	public Platform GetPlatform()
	{
		if (Platform != Platform.None)
		{
			return Platform;
		}
		Platform = Platform.Unknown;
		OperatingSystem oSVersion = Environment.OSVersion;
		Version version = oSVersion.Version;
		if (oSVersion.Platform != PlatformID.Win32NT)
		{
			return Platform;
		}
		switch (version.Major)
		{
		case 5:
			if (version.Minor != 0)
			{
				Platform = Platform.WindowsXP;
			}
			break;
		case 6:
			if (version.Minor != 0)
			{
				Platform = Platform.Windows7;
			}
			break;
		}
		return Platform;
	}

	public string ExpandRuntimeMacros(string value)
	{
		return ExpandMacros(value, "${", "}");
	}

	public string ExpandConstantMacros(string value)
	{
		return ExpandMacros(value, "$(", ")");
	}

	public RuntimeService()
	{
		AssemblyDirectory = Path.GetDirectoryName(typeof(RuntimeService).Assembly.Location);
		try
		{
			DirectoryInfo parent = Directory.GetParent(AssemblyDirectory);
			InstallRoot = parent.FullName;
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("[RuntimeService] Unable to find parent folder.", ex.Message);
			InstallRoot = AssemblyDirectory;
		}
		Platform = Platform.None;
		Values["RunningPath"] = AssemblyDirectory;
		SYSTEM_INFO lpSystemInfo = default(SYSTEM_INFO);
		GetSystemInfo(ref lpSystemInfo);
		PageSize = (int)lpSystemInfo.dwPageSize;
	}

	private string ExpandMacros(string value, string preamble, string postamble)
	{
		if (value == null)
		{
			return null;
		}
		while (true)
		{
			string text = StringExtensions.ExtractCodeFromBrackets(value, preamble, postamble);
			if (string.IsNullOrEmpty(text))
			{
				break;
			}
			string text2 = Values[text];
			if (text2 == null)
			{
				text2 = string.Empty;
			}
			value = value.Replace($"{preamble}{text}{postamble}", text2);
		}
		return value;
	}

	[DllImport("kernel32.dll")]
	private static extern void GetSystemInfo([MarshalAs(UnmanagedType.Struct)] ref SYSTEM_INFO lpSystemInfo);
}
