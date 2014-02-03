using System;
using System.Diagnostics;

namespace Redbox.HAL.Core;

internal class ProcessHelper
{
	public const string HalServiceProcess = "halsvc-win32.exe";

	public const string TesterProcess = "MSHALTester.exe";

	public const string ConsoleProcess = "halsvc-console.exe";

	public static ProcessHelper Instance => Singleton<ProcessHelper>.Instance;

	public ProcessStartInfo SetupDefaultProcessInfo(string processName)
	{
		ProcessStartInfo processStartInfo = new ProcessStartInfo(processName);
		processStartInfo.UserName = null;
		processStartInfo.Password = null;
		return processStartInfo;
	}

	public bool KillProcess(string name)
	{
		Process[] processesByName = Process.GetProcessesByName(name);
		if (processesByName.Length == 0)
		{
			return false;
		}
		Process[] array = processesByName;
		foreach (Process process in array)
		{
			Console.WriteLine("process = {0}", process.ProcessName);
			try
			{
				process.Kill();
			}
			catch
			{
			}
		}
		return true;
	}

	public bool HALProcessRunning()
	{
		return ProcessIsRunningInner("halsvc-win32.exe");
	}

	public bool ProcessIsRunning(string name)
	{
		return ProcessIsRunningInner(name);
	}

	private ProcessHelper()
	{
	}

	private string ProcessName(string name)
	{
		int num = name.LastIndexOf(".exe");
		if (-1 != num)
		{
			return name.Substring(0, num);
		}
		return name;
	}

	private bool ProcessIsRunningInner(string name)
	{
		name = ProcessName(name);
		Process[] processesByName = Process.GetProcessesByName(name);
		if (processesByName.Length != 0)
		{
			return true;
		}
		return false;
	}
}
