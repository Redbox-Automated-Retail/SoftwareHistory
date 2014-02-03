using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using Redbox.Core;
using Redbox.Lua;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Kernel;

public static class RixFunctions
{
	[KernelFunction(Name = "kernel.rixinstall")]
	internal static int InstallPackage(string path, LuaTable parameters)
	{
		string text = "--force --silent";
		foreach (object key in parameters.Keys)
		{
			text += $" -D:{key}=\"{parameters[key]}\"";
		}
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		Process process = new Process();
		process.StartInfo = new ProcessStartInfo(service.ExpandProperties(path), text)
		{
			CreateNoWindow = true,
			WindowStyle = ProcessWindowStyle.Hidden,
			WorkingDirectory = path
		};
		Process process2 = process;
		process2.Start();
		process2.WaitForExit();
		return process2.ExitCode;
	}

	[KernelFunction(Name = "kernel.rixuninstall")]
	internal static int UninstallPackage(string path, LuaTable parameters)
	{
		string text = "--force --silent";
		foreach (object key in parameters.Keys)
		{
			text += $" -D:{key}=\"{parameters[key]}\"";
		}
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		Process process = new Process();
		process.StartInfo = new ProcessStartInfo(service.ExpandProperties(path), text)
		{
			CreateNoWindow = true,
			WindowStyle = ProcessWindowStyle.Hidden,
			WorkingDirectory = path
		};
		Process process2 = process;
		process2.Start();
		process2.WaitForExit();
		return process2.ExitCode;
	}

	[KernelFunction(Name = "kernel.rixisproductinstalled")]
	internal static bool IsRixPackageInstalled(string productGuid)
	{
		if (string.IsNullOrEmpty(productGuid))
		{
			return false;
		}
		string keyName = $"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{productGuid}";
		string value = (string)Registry.GetValue(keyName, "SilentUninstallString", null);
		return !string.IsNullOrEmpty(value);
	}

	[KernelFunction(Name = "kernel.rixfindinstalldirectory")]
	internal static string FindInstalledDirectory(string productGuid)
	{
		if (string.IsNullOrEmpty(productGuid))
		{
			return null;
		}
		string keyName = $"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{productGuid}";
		string text = (string)Registry.GetValue(keyName, "InstallPath", null);
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		return null;
	}

	[KernelFunction(Name = "kernel.rixuninstallproduct")]
	internal static int UninstallPackage(string productGuid)
	{
		if (string.IsNullOrEmpty(productGuid))
		{
			return -1;
		}
		string keyName = $"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{productGuid}";
		string text = (string)Registry.GetValue(keyName, "SilentUninstallString", null);
		if (string.IsNullOrEmpty(text))
		{
			return -1;
		}
		int num = text.IndexOf(".exe");
		string text2 = text.Substring(0, num + 5);
		string arguments = text.Substring(num + 5);
		if (!File.Exists(text2))
		{
			LogHelper.Instance.Log("In rixuninstallproduct, {0} does not exists from product {1}.", text2, productGuid);
			return -1;
		}
		Process process = new Process();
		process.StartInfo = new ProcessStartInfo
		{
			FileName = text2,
			Arguments = arguments,
			CreateNoWindow = true,
			UseShellExecute = false,
			WindowStyle = ProcessWindowStyle.Hidden
		};
		Process process2 = process;
		process2.Start();
		process2.WaitForExit();
		return process2.ExitCode;
	}
}
