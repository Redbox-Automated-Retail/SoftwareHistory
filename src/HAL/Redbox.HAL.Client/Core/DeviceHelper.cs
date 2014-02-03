using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Redbox.HAL.Core;

internal class DeviceHelper : IDisposable
{
	[StructLayout(LayoutKind.Sequential)]
	internal class SP_DEVINFO_DATA
	{
		internal int cbSize;

		internal Guid classGuid;

		internal uint devInst;

		internal ulong reserved;
	}

	internal struct SP_DEVICE_INTERFACE_DATA
	{
		internal int cbSize;

		internal Guid interfaceClassGuid;

		internal int flags;

		private UIntPtr reserved;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal class SP_DEVINSTALL_PARAMS
	{
		internal int cbSize;

		internal int Flags;

		internal int FlagsEx;

		internal IntPtr hwndParent;

		internal IntPtr InstallMsgHandler;

		internal IntPtr InstallMsgHandlerContext;

		internal IntPtr FileQueue;

		internal IntPtr ClassInstallReserved;

		internal int Reserved;

		[MarshalAs(UnmanagedType.LPTStr)]
		internal string DriverPath;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal class SP_PROPCHANGE_PARAMS
	{
		internal SP_CLASSINSTALL_HEADER ClassInstallHeader = new SP_CLASSINSTALL_HEADER();

		internal int StateChange;

		internal int Scope;

		internal int HwProfile;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal class SP_CLASSINSTALL_HEADER
	{
		internal int cbSize;

		internal int InstallFunction;
	}

	private const int ERROR_INSUFFICIENT_BUFFER = 122;

	private const int DIGCF_ALLCLASSES = 4;

	private const int DIGCF_PRESENT = 2;

	private const int INVALID_HANDLE_VALUE = -1;

	private const int MAX_DEV_LEN = 1000;

	private const int DEVICE_NOTIFY_WINDOW_HANDLE = 0;

	private const int DEVICE_NOTIFY_SERVICE_HANDLE = 1;

	private const int DEVICE_NOTIFY_ALL_INTERFACE_CLASSES = 4;

	private const int DBT_DEVTYP_DEVICEINTERFACE = 5;

	private const int DBT_DEVNODES_CHANGED = 7;

	private const int WM_DEVICECHANGE = 537;

	private const int DIF_PROPERTYCHANGE = 18;

	private const int DICS_FLAG_GLOBAL = 1;

	private const int DICS_FLAG_CONFIGSPECIFIC = 2;

	private const int DICS_ENABLE = 1;

	private const int DICS_DISABLE = 2;

	private const int SPDRP_DEVICEDESC = 0;

	private const int SPDRP_HARDWAREID = 1;

	private const int SPDRP_COMPATIBLEIDS = 2;

	private const int SPDRP_UNUSED0 = 3;

	private const int SPDRP_SERVICE = 4;

	private const int SPDRP_UNUSED1 = 5;

	private const int SPDRP_UNUSED2 = 6;

	private const int SPDRP_CLASS = 7;

	private const int SPDRP_CLASSGUID = 8;

	private const int SPDRP_DRIVER = 9;

	private const int SPDRP_CONFIGFLAGS = 10;

	private const int SPDRP_MFG = 11;

	private const int SPDRP_FRIENDLYNAME = 12;

	private const int SPDRP_LOCATION_INFORMATION = 13;

	private const int SPDRP_PHYSICAL_DEVICE_OBJECT_NAME = 14;

	private const int SPDRP_CAPABILITIES = 15;

	private const int SPDRP_UI_NUMBER = 16;

	private const int SPDRP_UPPERFILTERS = 17;

	private const int SPDRP_LOWERFILTERS = 18;

	private const int SPDRP_BUSTYPEGUID = 19;

	private const int SPDRP_LEGACYBUSTYPE = 20;

	private const int SPDRP_BUSNUMBER = 21;

	private const int SPDRP_ENUMERATOR_NAME = 22;

	private const int SPDRP_SECURITY = 23;

	private const int SPDRP_SECURITY_SDS = 24;

	private const int SPDRP_DEVTYPE = 25;

	private const int SPDRP_EXCLUSIVE = 26;

	private const int SPDRP_CHARACTERISTICS = 27;

	private const int SPDRP_ADDRESS = 28;

	private const int SPDRP_UI_NUMBER_DESC_FORMAT = 29;

	private const int SPDRP_DEVICE_POWER_DATA = 30;

	private const int SPDRP_REMOVAL_POLICY = 31;

	private const int SPDRP_REMOVAL_POLICY_HW_DEFAULT = 32;

	private const int SPDRP_REMOVAL_POLICY_OVERRIDE = 33;

	private const int SPDRP_INSTALL_STATE = 34;

	private const int SPDRP_LOCATION_PATHS = 35;

	private const int SPDRP_BASE_CONTAINERID = 36;

	private StringBuilder m_deviceBufferName;

	private StringBuilder m_classBufferName;

	private StringBuilder m_hardwareIDBuffer;

	private ManualResetEvent Waiter { get; set; }

	private int ResetPauseTime => 1500;

	private StringBuilder DeviceNameBuffer
	{
		get
		{
			if (m_deviceBufferName != null)
			{
				return m_deviceBufferName;
			}
			m_deviceBufferName = new StringBuilder("");
			m_deviceBufferName.Capacity = 1000;
			return m_deviceBufferName;
		}
	}

	private StringBuilder ClassNameBuffer
	{
		get
		{
			if (m_classBufferName != null)
			{
				return m_classBufferName;
			}
			m_classBufferName = new StringBuilder("");
			m_classBufferName.Capacity = 1000;
			return m_classBufferName;
		}
	}

	private StringBuilder HardwareIDBuffer
	{
		get
		{
			if (m_hardwareIDBuffer != null)
			{
				return m_hardwareIDBuffer;
			}
			m_hardwareIDBuffer = new StringBuilder("");
			m_hardwareIDBuffer.Capacity = 1000;
			return m_hardwareIDBuffer;
		}
	}

	public void Dispose()
	{
		m_deviceBufferName = (m_classBufferName = (m_hardwareIDBuffer = null));
	}

	public bool ChangeByHWID(DeviceDescriptor descriptor, DeviceState state)
	{
		return SetDeviceStateInner(descriptor, state);
	}

	public bool ResetDriver(DeviceDescriptor descriptor)
	{
		if (!SetDeviceStateInner(descriptor, DeviceState.Disable))
		{
			return false;
		}
		if (!SetDeviceStateInner(descriptor, DeviceState.Enable))
		{
			return false;
		}
		return SetDeviceStateInner(descriptor, DeviceState.Enable);
	}

	public bool ResetByName(string deviceName)
	{
		int resetPauseTime = ResetPauseTime;
		ManualResetEvent manualResetEvent = new ManualResetEvent(initialState: false);
		if (!SetDeviceStateInner(DeviceClass.Ports, DeviceState.Disable, deviceName, MatchType.DeviceDescription))
		{
			return false;
		}
		manualResetEvent.WaitOne(resetPauseTime);
		if (!SetDeviceStateInner(DeviceClass.USB, DeviceState.Disable, deviceName, MatchType.DeviceDescription))
		{
			return false;
		}
		manualResetEvent.WaitOne(resetPauseTime);
		if (!SetDeviceStateInner(DeviceClass.USB, DeviceState.Enable, deviceName, MatchType.DeviceDescription))
		{
			return false;
		}
		manualResetEvent.WaitOne(resetPauseTime);
		if (!SetDeviceStateInner(DeviceClass.Ports, DeviceState.Enable, deviceName, MatchType.DeviceDescription))
		{
			return false;
		}
		return true;
	}

	public void FindDeviceWithPid(DeviceClass clazz, string vid, string pid, MatchType matcher, List<string> errors, List<string> devMatches)
	{
		int lastError = 0;
		IntPtr intPtr = IntPtr.Zero;
		char[] separator = new char[1] { '&' };
		try
		{
			Guid gClass = Guid.Empty;
			intPtr = SetupDiGetClassDevs(ref gClass, 0u, IntPtr.Zero, 6u);
			if (intPtr.ToInt32() == -1)
			{
				return;
			}
			SP_DEVINFO_DATA sP_DEVINFO_DATA = new SP_DEVINFO_DATA();
			sP_DEVINFO_DATA.cbSize = 28;
			sP_DEVINFO_DATA.devInst = 0u;
			sP_DEVINFO_DATA.classGuid = Guid.Empty;
			sP_DEVINFO_DATA.reserved = 0uL;
			for (uint num = 0u; SetupDiEnumDeviceInfo(intPtr, num, sP_DEVINFO_DATA); num++)
			{
				if (!SetupDiGetDeviceRegistryProperty(intPtr, sP_DEVINFO_DATA, 7u, 0u, ClassNameBuffer, 1000u, IntPtr.Zero))
				{
					lastError = Marshal.GetLastWin32Error();
					errors.Add($"SetupDiGetDeviceRegisProperty returned {lastError} searching for class.");
					break;
				}
				if (clazz.ToString() != ClassNameBuffer.ToString())
				{
					continue;
				}
				string text = QueryHardwareId(intPtr, sP_DEVINFO_DATA, out lastError);
				if (!text.ToLower().StartsWith("vid"))
				{
					int num2 = text.IndexOf("vid");
					if (num2 == -1)
					{
						errors.Add("Invalid string.");
						break;
					}
					text = text.Substring(num2).ToLower();
				}
				string[] array = text.Split(separator, StringSplitOptions.RemoveEmptyEntries);
				if (array.Length < 2)
				{
					LogHelper.Instance.Log("The HWID '{0}' is malformed.", text);
					continue;
				}
				string text2 = array[0].Substring(4);
				string s = array[1].Substring(4);
				if (!(text2 == vid))
				{
					continue;
				}
				int num3 = int.Parse(s);
				int num4 = int.Parse(pid);
				switch (matcher)
				{
				case MatchType.GTEPid:
					if (num3 >= num4)
					{
						devMatches.Add(text);
					}
					break;
				case MatchType.ExactPID:
					if (num3 == num4)
					{
						devMatches.Add(text);
					}
					break;
				}
			}
		}
		catch (Exception)
		{
			errors.Add("Unable to enumerate device tree.");
		}
		finally
		{
			try
			{
				if (intPtr != IntPtr.Zero && intPtr.ToInt32() != -1)
				{
					int num5 = SetupDiDestroyDeviceInfoList(intPtr);
					if (num5 == 0)
					{
						errors.Add($"SetupDiDestroyDeviceInfoList returned {num5}");
					}
				}
			}
			catch (Exception)
			{
				errors.Add("Clearing the device info list caught an exception.");
			}
		}
	}

	public void EnumDevices(DeviceClass clazz, TextWriter writer)
	{
		IntPtr intPtr = IntPtr.Zero;
		int lastError = 0;
		Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
		string text = clazz.ToString();
		try
		{
			Guid gClass = Guid.Empty;
			intPtr = SetupDiGetClassDevs(ref gClass, 0u, IntPtr.Zero, 6u);
			if (intPtr.ToInt32() == -1)
			{
				return;
			}
			SP_DEVINFO_DATA sP_DEVINFO_DATA = new SP_DEVINFO_DATA();
			sP_DEVINFO_DATA.cbSize = 28;
			sP_DEVINFO_DATA.devInst = 0u;
			sP_DEVINFO_DATA.classGuid = Guid.Empty;
			sP_DEVINFO_DATA.reserved = 0uL;
			for (uint num = 0u; SetupDiEnumDeviceInfo(intPtr, num, sP_DEVINFO_DATA); num++)
			{
				if (!SetupDiGetDeviceRegistryProperty(intPtr, sP_DEVINFO_DATA, 7u, 0u, ClassNameBuffer, 1000u, IntPtr.Zero))
				{
					lastError = Marshal.GetLastWin32Error();
					return;
				}
				if (clazz == DeviceClass.None)
				{
					List<string> list = null;
					list = (dictionary.ContainsKey(ClassNameBuffer.ToString()) ? dictionary[ClassNameBuffer.ToString()] : (dictionary[ClassNameBuffer.ToString()] = new List<string>()));
					list.Add(QueryHardwareId(intPtr, sP_DEVINFO_DATA, out lastError));
				}
				else if (text == ClassNameBuffer.ToString())
				{
					string arg = QueryHardwareId(intPtr, sP_DEVINFO_DATA, out lastError);
					writer.WriteLine("Class {0} hwid {1}", ClassNameBuffer.ToString(), arg);
				}
			}
			if (clazz != DeviceClass.None)
			{
				return;
			}
			foreach (string key in dictionary.Keys)
			{
				List<string> list3 = dictionary[key];
				writer.WriteLine("Devices for class {0}", key);
				foreach (string item in list3)
				{
					writer.WriteLine("  HWID: {0}", item);
				}
			}
		}
		catch (Exception)
		{
			LogHelper.Instance.Log("Unable to enumerate device tree.");
		}
		finally
		{
			try
			{
				if (intPtr != IntPtr.Zero && intPtr.ToInt32() != -1)
				{
					int num2 = SetupDiDestroyDeviceInfoList(intPtr);
					if (num2 == 0)
					{
						LogHelper.Instance.Log($"SetupDiDestroyDeviceInfoList returned {num2}");
					}
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("Clearing the device info list caught an exception.", e);
			}
		}
	}

	public bool CameraActive()
	{
		return SetDeviceStateInner(DeviceClass.Image, DeviceState.Query, null, MatchType.None);
	}

	public DeviceHelper()
	{
		Waiter = new ManualResetEvent(initialState: false);
	}

	[DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
	internal static extern bool SetupDiEnumDeviceInterfaces(IntPtr deviceInfoSet, SP_DEVINFO_DATA deviceInfoData, ref Guid interfaceClassGuid, int memberIndex, SP_DEVICE_INTERFACE_DATA deviceInterfaceData);

	[DllImport("setupapi.dll", SetLastError = true)]
	internal static extern IntPtr SetupDiGetClassDevs(ref Guid gClass, uint iEnumerator, IntPtr hParent, uint nFlags);

	[DllImport("setupapi.dll", SetLastError = true)]
	internal static extern int SetupDiDestroyDeviceInfoList(IntPtr lpInfoSet);

	[DllImport("setupapi.dll", SetLastError = true)]
	internal static extern bool SetupDiEnumDeviceInfo(IntPtr lpInfoSet, uint dwIndex, SP_DEVINFO_DATA devInfoData);

	[DllImport("setupapi.dll", SetLastError = true)]
	internal static extern bool SetupDiGetDeviceRegistryProperty(IntPtr lpInfoSet, SP_DEVINFO_DATA DeviceInfoData, uint Property, uint PropertyRegDataType, StringBuilder PropertyBuffer, uint PropertyBufferSize, IntPtr RequiredSize);

	[DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
	internal static extern bool SetupDiSetClassInstallParams(IntPtr DeviceInfoSet, IntPtr DeviceInfoData, IntPtr ClassInstallParams, int ClassInstallParamsSize);

	[DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
	internal static extern bool SetupDiCallClassInstaller(uint InstallFunction, IntPtr DeviceInfoSet, IntPtr DeviceInfoData);

	private string QueryDeviceDescription(IntPtr devInfo, SP_DEVINFO_DATA deviceInfoData, out int lastError)
	{
		lastError = 0;
		if (!SetupDiGetDeviceRegistryProperty(devInfo, deviceInfoData, 0u, 0u, DeviceNameBuffer, 1000u, IntPtr.Zero))
		{
			lastError = Marshal.GetLastWin32Error();
			return null;
		}
		return DeviceNameBuffer.ToString().ToLower();
	}

	private string QueryHardwareId(IntPtr devInfo, SP_DEVINFO_DATA deviceInfoData, out int lastError)
	{
		lastError = 0;
		if (!SetupDiGetDeviceRegistryProperty(devInfo, deviceInfoData, 1u, 0u, HardwareIDBuffer, 1000u, IntPtr.Zero))
		{
			lastError = Marshal.GetLastWin32Error();
			return null;
		}
		return HardwareIDBuffer.ToString().ToLower();
	}

	private bool SetDeviceStateInner(DeviceClass devClass, DeviceState devState, string queryData, MatchType match)
	{
		if (devState == DeviceState.None)
		{
			return false;
		}
		IntPtr intPtr = IntPtr.Zero;
		int lastError = 0;
		try
		{
			Guid gClass = Guid.Empty;
			intPtr = SetupDiGetClassDevs(ref gClass, 0u, IntPtr.Zero, 6u);
			if (intPtr.ToInt32() == -1)
			{
				return false;
			}
			SP_DEVINFO_DATA sP_DEVINFO_DATA = new SP_DEVINFO_DATA();
			sP_DEVINFO_DATA.cbSize = 28;
			sP_DEVINFO_DATA.devInst = 0u;
			sP_DEVINFO_DATA.classGuid = Guid.Empty;
			sP_DEVINFO_DATA.reserved = 0uL;
			for (uint num = 0u; SetupDiEnumDeviceInfo(intPtr, num, sP_DEVINFO_DATA); num++)
			{
				if (!SetupDiGetDeviceRegistryProperty(intPtr, sP_DEVINFO_DATA, 7u, 0u, ClassNameBuffer, 1000u, IntPtr.Zero))
				{
					lastError = Marshal.GetLastWin32Error();
				}
				else if (!(devClass.ToString() != ClassNameBuffer.ToString()))
				{
					if (DeviceState.Query == devState)
					{
						string arg = QueryHardwareId(intPtr, sP_DEVINFO_DATA, out lastError);
						LogHelper.Instance.Log($"Hardware ID: {arg}");
						return true;
					}
					string text = ((match == MatchType.HardwareId) ? QueryHardwareId(intPtr, sP_DEVINFO_DATA, out lastError) : QueryDeviceDescription(intPtr, sP_DEVINFO_DATA, out lastError));
					if (text.Equals(queryData, StringComparison.CurrentCultureIgnoreCase))
					{
						bool flag = ChangeState(intPtr, sP_DEVINFO_DATA, devState);
						LogHelper.Instance.Log($"Change state on class {devClass.ToString()} ({devState.ToString()}) returned {flag}");
						return flag;
					}
				}
			}
			return false;
		}
		catch (Exception)
		{
			LogHelper.Instance.Log("Unable to enumerate device tree.");
			return false;
		}
		finally
		{
			try
			{
				if (intPtr != IntPtr.Zero && intPtr.ToInt32() != -1)
				{
					int num2 = SetupDiDestroyDeviceInfoList(intPtr);
					if (num2 == 0)
					{
						LogHelper.Instance.Log($"SetupDiDestroyDeviceInfoList returned {num2}");
					}
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("Clearing the device info list caught an exception.", e);
			}
		}
	}

	private bool SetDeviceStateInner(DeviceDescriptor descriptor, DeviceState state)
	{
		IntPtr intPtr = IntPtr.Zero;
		int lastError = 0;
		int num = 0;
		try
		{
			Guid gClass = Guid.Empty;
			intPtr = SetupDiGetClassDevs(ref gClass, 0u, IntPtr.Zero, 6u);
			if (intPtr.ToInt32() == -1)
			{
				return false;
			}
			SP_DEVINFO_DATA sP_DEVINFO_DATA = new SP_DEVINFO_DATA();
			sP_DEVINFO_DATA.cbSize = 28;
			sP_DEVINFO_DATA.devInst = 0u;
			sP_DEVINFO_DATA.classGuid = Guid.Empty;
			sP_DEVINFO_DATA.reserved = 0uL;
			for (uint num2 = 0u; SetupDiEnumDeviceInfo(intPtr, num2, sP_DEVINFO_DATA); num2++)
			{
				if (!SetupDiGetDeviceRegistryProperty(intPtr, sP_DEVINFO_DATA, 7u, 0u, ClassNameBuffer, 1000u, IntPtr.Zero))
				{
					lastError = Marshal.GetLastWin32Error();
					continue;
				}
				string text = QueryHardwareId(intPtr, sP_DEVINFO_DATA, out lastError);
				if (text != null && text.Contains(descriptor.ToString()))
				{
					LogHelper.Instance.Log("Found HWID {0} on class {1}", text, ClassNameBuffer);
					bool flag = ChangeState(intPtr, sP_DEVINFO_DATA, state);
					LogHelper.Instance.Log("Change state {0} returned {1}", state.ToString(), flag ? "OK" : "FAIL");
					if (!flag)
					{
						num++;
					}
				}
			}
			return num == 0;
		}
		catch (Exception)
		{
			LogHelper.Instance.Log("Unable to enumerate device tree.");
			return false;
		}
		finally
		{
			try
			{
				if (intPtr != IntPtr.Zero && intPtr.ToInt32() != -1)
				{
					int num3 = SetupDiDestroyDeviceInfoList(intPtr);
					if (num3 == 0)
					{
						LogHelper.Instance.Log($"SetupDiDestroyDeviceInfoList returned {num3}");
					}
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("Clearing the device info list caught an exception.", e);
			}
		}
	}

	private bool ChangeState(IntPtr hDevInfo, SP_DEVINFO_DATA devInfoData, DeviceState devState)
	{
		IntPtr intPtr = IntPtr.Zero;
		IntPtr intPtr2 = IntPtr.Zero;
		try
		{
			SP_PROPCHANGE_PARAMS sP_PROPCHANGE_PARAMS = new SP_PROPCHANGE_PARAMS();
			int cb;
			int cb2;
			if (DeviceState.Enable == devState)
			{
				sP_PROPCHANGE_PARAMS.ClassInstallHeader.cbSize = Marshal.SizeOf(typeof(SP_CLASSINSTALL_HEADER));
				sP_PROPCHANGE_PARAMS.ClassInstallHeader.InstallFunction = 18;
				sP_PROPCHANGE_PARAMS.StateChange = 1;
				sP_PROPCHANGE_PARAMS.Scope = 1;
				sP_PROPCHANGE_PARAMS.HwProfile = 0;
				cb = Marshal.SizeOf(sP_PROPCHANGE_PARAMS);
				intPtr2 = Marshal.AllocHGlobal(cb);
				Marshal.StructureToPtr(sP_PROPCHANGE_PARAMS, intPtr2, fDeleteOld: true);
				cb2 = Marshal.SizeOf(devInfoData);
				intPtr = Marshal.AllocHGlobal(cb2);
				if (SetupDiSetClassInstallParams(hDevInfo, intPtr, intPtr2, Marshal.SizeOf(typeof(SP_PROPCHANGE_PARAMS))))
				{
					SetupDiCallClassInstaller(18u, hDevInfo, intPtr);
				}
				sP_PROPCHANGE_PARAMS.ClassInstallHeader.cbSize = Marshal.SizeOf(typeof(SP_CLASSINSTALL_HEADER));
				sP_PROPCHANGE_PARAMS.ClassInstallHeader.InstallFunction = 18;
				sP_PROPCHANGE_PARAMS.StateChange = 1;
				sP_PROPCHANGE_PARAMS.Scope = 2;
				sP_PROPCHANGE_PARAMS.HwProfile = 0;
			}
			else
			{
				sP_PROPCHANGE_PARAMS.ClassInstallHeader.cbSize = Marshal.SizeOf(typeof(SP_CLASSINSTALL_HEADER));
				sP_PROPCHANGE_PARAMS.ClassInstallHeader.InstallFunction = 18;
				sP_PROPCHANGE_PARAMS.StateChange = 2;
				sP_PROPCHANGE_PARAMS.Scope = 2;
				sP_PROPCHANGE_PARAMS.HwProfile = 0;
			}
			cb = Marshal.SizeOf(sP_PROPCHANGE_PARAMS);
			intPtr2 = Marshal.AllocHGlobal(cb);
			Marshal.StructureToPtr(sP_PROPCHANGE_PARAMS, intPtr2, fDeleteOld: true);
			cb2 = Marshal.SizeOf(devInfoData);
			intPtr = Marshal.AllocHGlobal(cb2);
			Marshal.StructureToPtr(devInfoData, intPtr, fDeleteOld: true);
			if (!SetupDiSetClassInstallParams(hDevInfo, intPtr, intPtr2, Marshal.SizeOf(typeof(SP_PROPCHANGE_PARAMS))))
			{
				LogHelper.Instance.Log("SetupDiSetClassInstallParams returned false.");
				return false;
			}
			if (!SetupDiCallClassInstaller(18u, hDevInfo, intPtr))
			{
				LogHelper.Instance.Log("SetupDiCallClassInstaller returned false.");
				return false;
			}
			return true;
		}
		catch (Exception)
		{
			return false;
		}
		finally
		{
			if (intPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(intPtr);
			}
			if (intPtr2 != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(intPtr2);
			}
		}
	}
}
