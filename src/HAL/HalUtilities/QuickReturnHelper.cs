using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Redbox.HAL.Client;
using Redbox.HAL.Core;

namespace HALUtilities;

internal sealed class QuickReturnHelper
{
	[StructLayout(LayoutKind.Sequential)]
	private class SP_DEVINFO_DATA
	{
		internal int cbSize;

		internal Guid classGuid;

		internal uint devInst;

		internal ulong reserved;
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

	private const int DIF_PROPERTYCHANGE = 18;

	private const int DIGCF_ALLCLASSES = 4;

	private const int DIGCF_PRESENT = 2;

	private const int DICS_FLAG_GLOBAL = 1;

	private const int DICS_FLAG_CONFIGSPECIFIC = 2;

	private const int DICS_ENABLE = 1;

	private const int DICS_DISABLE = 2;

	private const int INVALID_HANDLE_VALUE = -1;

	private const int MAX_DEV_LEN = 1000;

	private const int SPDRP_DEVICEDESC = 0;

	private const int SPDRP_HARDWAREID = 1;

	private const int SPDRP_CLASS = 7;

	private StringBuilder m_deviceBufferName;

	private StringBuilder m_classBufferName;

	private StringBuilder m_hardwareIDBuffer;

	private readonly string[] QuickReturnCounters = new string[3] { "QuickReturnAttention", "QuickReturnNoInsertDisk", "QuickReturnSuccessful" };

	private string TimeStamp => $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}";

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

	private string LogFileName => "QuickReturnStatus.log";

	private string EEPROMUpdateLog => "UpdateEEPROMError.log";

	private string QRStatusLogFile => "qr-status.log";

	internal bool Active(out string id)
	{
		return SearchDevicesForID(DeviceClass.Ports, "ftdibus\\comport&vid_0403&pid_6001", out id);
	}

	internal void DumpCounters(HardwareService service, bool reset)
	{
		string path = Path.Combine(RuntimeService.Instance.AssemblyDirectory, "QRHelper.log");
		using StreamWriter streamWriter = new StreamWriter(File.Open(path, FileMode.Append, FileAccess.Write, FileShare.Read));
		if (service == null)
		{
			streamWriter.WriteLine("Can't talk to HAL.");
			return;
		}
		string path2 = Path.Combine(RuntimeService.Instance.AssemblyDirectory, "QRstats.log");
		if (File.Exists(path2))
		{
			try
			{
				FileHelper.CreateBackup(path2, BackupAction.Move);
			}
			catch (Exception ex)
			{
				streamWriter.WriteLine("Unable to backup statistics file: {0}", ex.Message);
				return;
			}
		}
		CounterHelper counterHelper = new CounterHelper();
		using (StreamWriter streamWriter2 = new StreamWriter(path2))
		{
			streamWriter2.WriteLine("QuickReturn stats of {0}", DateTime.Now.ToShortDateString());
			bool flag = ProperlyConfiguredInner(service, new List<string>());
			streamWriter2.WriteLine("Quick Return {0} configured.", flag ? "is" : "is not");
			string arg = PollStatus(service);
			streamWriter2.WriteLine("Quick Return poll status is {0}.", arg);
			string[] quickReturnCounters = QuickReturnCounters;
			foreach (string text in quickReturnCounters)
			{
				if (counterHelper.GetCounterValue(service, text, out var v))
				{
					streamWriter2.WriteLine("{0}:{1}", text, v);
				}
				else
				{
					streamWriter.WriteLine("Unable to get counter value for {0}", text);
				}
			}
			FindCommunicationErrors(streamWriter2);
		}
		if (reset)
		{
			string[] quickReturnCounters2 = QuickReturnCounters;
			foreach (string name in quickReturnCounters2)
			{
				counterHelper.ResetCounter(service, name);
			}
		}
	}

	internal void Cleanup()
	{
		List<string> list = new List<string>();
		string[] array = new string[3] { "c:\\Program Files\\Redbox\\QRHelper", "c:\\Program Files\\Redbox\\QRStatus", "c:\\Program Files\\Redbox\\QRInstalled" };
		string[] array2 = new string[1] { "c:\\QRStatus.zip" };
		string[] array3 = array;
		foreach (string text in array3)
		{
			try
			{
				Directory.Delete(text, recursive: true);
			}
			catch (Exception ex)
			{
				list.Add($"Could not delete directory '{text}': exception {ex.Message}");
			}
		}
		string[] array4 = array2;
		foreach (string text2 in array4)
		{
			try
			{
				File.Delete(text2);
			}
			catch (Exception ex2)
			{
				list.Add($"Could not delete directory '{text2}': exception {ex2.Message}");
			}
		}
		LogErrors("Cleanup.log", list);
	}

	internal bool ProperlyConfigured(HardwareService service)
	{
		List<string> errors = new List<string>();
		try
		{
			return ProperlyConfiguredInner(service, errors);
		}
		finally
		{
			LogErrors(QRStatusLogFile, errors);
		}
	}

	internal bool ProperlyConfigured(HardwareService service, TextWriter writer)
	{
		List<string> list = new List<string>();
		try
		{
			return ProperlyConfiguredInner(service, list);
		}
		finally
		{
			if (list.Count > 0)
			{
				foreach (string item in list)
				{
					writer.WriteLine("{0} {1}", TimeStamp, item);
				}
			}
			list.Clear();
		}
	}

	internal void UpdateEEPROM(ClientHelper helper, int waitTime)
	{
		List<string> errors = new List<string>();
		UpdateEEPROMInner(helper, errors, waitTime);
		LogEEPROMErrors(errors);
	}

	internal string PollStatus(HardwareService service)
	{
		_ = string.Empty;
		HardwareCommandResult hardwareCommandResult = service.ExecuteImmediate("QUICKRETURN POLLSTATUS", out var job);
		if (!hardwareCommandResult.Success)
		{
			return "Unable to ascertain polling status.";
		}
		if (job.GetStack(out var stack).Success)
		{
			return stack.Pop();
		}
		return "Unable to ascertain polling status.";
	}

	internal string IsQuickReturnInstalled(HardwareService service)
	{
		if (service == null)
		{
			return "Service down";
		}
		HardwareCommandResult hardwareCommandResult = service.ExecuteImmediate("QUICKRETURN ISINSTALLED", out var job);
		if (!hardwareCommandResult.Success)
		{
			return "Service down";
		}
		HardwareCommandResult stack = job.GetStack(out var stack2);
		if (!stack.Success)
		{
			return "Service down";
		}
		if (!(stack2.Peek() == "TRUE"))
		{
			return "NOT INSTALLED";
		}
		return "INSTALLED";
	}

	internal string FormatTime(DateTime t)
	{
		return $"{t.ToShortDateString()} {t.ToShortTimeString()}";
	}

	internal QuickReturnHelper()
	{
	}

	private void LogErrors(string fileName, List<string> errors)
	{
		string path = Path.Combine(RuntimeService.Instance.AssemblyDirectory, fileName);
		if (File.Exists(path))
		{
			try
			{
				File.Delete(path);
			}
			catch
			{
			}
		}
		if (errors.Count == 0)
		{
			return;
		}
		using (StreamWriter streamWriter = new StreamWriter(path))
		{
			foreach (string error in errors)
			{
				streamWriter.WriteLine("{0} {1}", TimeStamp, error);
			}
		}
		errors.Clear();
	}

	private bool UpdateEEPROMInner(ClientHelper helper, List<string> errors, int waitTime)
	{
		HardwareService hardwareService = helper.AutoConnect();
		if (hardwareService == null)
		{
			errors.Add("Unable to talk to service.");
			return false;
		}
		using UpdateQREEPROMJob updateQREEPROMJob = new UpdateQREEPROMJob(hardwareService);
		updateQREEPROMJob.Run();
		if (HardwareJobStatus.Completed == updateQREEPROMJob.EndStatus)
		{
			return true;
		}
		foreach (Error error in updateQREEPROMJob.Errors)
		{
			errors.Add($"[{error.Code}] {error.Description} :\n{error.Details}");
		}
		return false;
	}

	private void LogFormatted(string msg, params object[] items)
	{
		msg = string.Format(msg, items);
		LogHelper.Instance.Log(msg);
	}

	private string QueryDeviceDescription(IntPtr devInfo, SP_DEVINFO_DATA deviceInfoData, out int lastError)
	{
		lastError = 0;
		if (!SetupDiGetDeviceRegistryProperty(devInfo, deviceInfoData, 0u, 0u, DeviceNameBuffer, 1000u, IntPtr.Zero))
		{
			lastError = Marshal.GetLastWin32Error();
			LogHelper.Instance.Log($"SetupDiGetDeviceRegisProperty returned {lastError} searching for name property.", LogEntryType.Error);
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
			LogHelper.Instance.Log($"SetupDiGetDeviceRegisProperty returned {lastError} searching for hardware id property.", LogEntryType.Error);
			return null;
		}
		return HardwareIDBuffer.ToString().ToLower();
	}

	private bool ProperlyConfiguredInner(HardwareService service, List<string> errors)
	{
		if (service == null)
		{
			errors.Add("Service is down.");
			return false;
		}
		string text = IsQuickReturnInstalled(service);
		string id = string.Empty;
		bool flag = Active(out id);
		string text2 = PollStatus(service);
		if (text == "INSTALLED" && flag && text2 == "ACTIVE")
		{
			return true;
		}
		errors.Add($"The device installed status is {text}");
		errors.Add(string.Format("The device shows in device manager: {0} ID: {1}", flag, flag ? id : "None"));
		errors.Add($"The poll status is {text2}");
		return false;
	}

	private bool SearchDevicesForID(DeviceClass devClass, string id, out string foundID)
	{
		IntPtr intPtr = IntPtr.Zero;
		int lastError = 0;
		foundID = "None";
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
			int num = 0;
			for (uint num2 = 0u; SetupDiEnumDeviceInfo(intPtr, num2, sP_DEVINFO_DATA); num2++)
			{
				if (!SetupDiGetDeviceRegistryProperty(intPtr, sP_DEVINFO_DATA, 7u, 0u, ClassNameBuffer, 1000u, IntPtr.Zero))
				{
					lastError = Marshal.GetLastWin32Error();
					LogHelper.Instance.Log($"SetupDiGetDeviceRegisProperty returned {lastError} searching for class.", LogEntryType.Error);
					return false;
				}
				if (!(devClass.ToString() != ClassNameBuffer.ToString()))
				{
					string text = QueryHardwareId(intPtr, sP_DEVINFO_DATA, out lastError);
					if (text.Equals(id, StringComparison.CurrentCultureIgnoreCase))
					{
						num++;
						foundID = (string)text.Clone();
						break;
					}
				}
			}
			return num > 0;
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

	private void Log(string msg)
	{
		LogHelper.Instance.Log(msg);
	}

	private void LogEEPROMErrors(List<string> errors)
	{
		LogErrors(EEPROMUpdateLog, errors);
	}

	private void LogConfigurationErrors(List<string> errors)
	{
		LogErrors(LogFileName, errors);
	}

	private void FindCommunicationErrors(TextWriter writer)
	{
		Process process = new Process();
		ProcessStartInfo startInfo = process.StartInfo;
		startInfo.FileName = "find.exe";
		startInfo.UseShellExecute = false;
		startInfo.RedirectStandardError = true;
		startInfo.RedirectStandardOutput = true;
		startInfo.WorkingDirectory = "c:\\Program Files\\Redbox\\KioskLogs\\ErrorLogs";
		startInfo.Arguments = "/c \"communicate\" *.*";
		process.Start();
		string arg = process.StandardOutput.ReadToEnd();
		process.WaitForExit();
		writer.WriteLine("Result = {0}", arg);
	}

	[DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
	internal static extern bool SetupDiSetClassInstallParams(IntPtr DeviceInfoSet, IntPtr DeviceInfoData, IntPtr ClassInstallParams, int ClassInstallParamsSize);

	[DllImport("setupapi.dll", CharSet = CharSet.Auto)]
	internal static extern bool SetupDiCallClassInstaller(uint InstallFunction, IntPtr DeviceInfoSet, IntPtr DeviceInfoData);

	[DllImport("setupapi.dll", SetLastError = true)]
	private static extern int SetupDiDestroyDeviceInfoList(IntPtr lpInfoSet);

	[DllImport("setupapi.dll", SetLastError = true)]
	private static extern bool SetupDiEnumDeviceInfo(IntPtr lpInfoSet, uint dwIndex, SP_DEVINFO_DATA devInfoData);

	[DllImport("setupapi.dll", SetLastError = true)]
	private static extern bool SetupDiGetDeviceRegistryProperty(IntPtr lpInfoSet, SP_DEVINFO_DATA DeviceInfoData, uint Property, uint PropertyRegDataType, StringBuilder PropertyBuffer, uint PropertyBufferSize, IntPtr RequiredSize);

	[DllImport("setupapi.dll", SetLastError = true)]
	private static extern IntPtr SetupDiGetClassDevs(ref Guid gClass, uint iEnumerator, IntPtr hParent, uint nFlags);
}
