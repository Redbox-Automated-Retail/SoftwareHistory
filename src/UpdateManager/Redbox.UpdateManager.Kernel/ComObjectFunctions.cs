using System.Diagnostics;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Kernel;

public static class ComObjectFunctions
{
	[KernelFunction(Name = "kernel.ismoduleselfregistering")]
	internal static bool IsModuleSelfRegistering(string fileName)
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		if (service != null)
		{
			return SelfRegistrationTool.IsModuleSelfRegistering(service.ExpandProperties(fileName));
		}
		return false;
	}

	[KernelFunction(Name = "kernel.registercomobject")]
	internal static bool RegisterComObject(string fileName)
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		string text = service.ExpandProperties(fileName);
		if (SelfRegistrationTool.IsModuleSelfRegistering(text))
		{
			SelfRegistrationTool.Register(text);
			return true;
		}
		Process process = new Process();
		process.StartInfo = new ProcessStartInfo
		{
			CreateNoWindow = true,
			UseShellExecute = false,
			FileName = "regsvr32.exe",
			WindowStyle = ProcessWindowStyle.Hidden,
			Arguments = $"/s {text}"
		};
		Process process2 = process;
		process2.Start();
		process2.WaitForExit();
		return process2.ExitCode == 0;
	}

	[KernelFunction(Name = "kernel.unregistercomobject")]
	internal static bool UnregisterComObject(string fileName)
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		string text = service.ExpandProperties(fileName);
		if (SelfRegistrationTool.IsModuleSelfRegistering(text))
		{
			SelfRegistrationTool.Unregister(text);
			return true;
		}
		Process process = new Process();
		process.StartInfo = new ProcessStartInfo
		{
			CreateNoWindow = true,
			UseShellExecute = false,
			FileName = "regsvr32.exe",
			WindowStyle = ProcessWindowStyle.Hidden,
			Arguments = $"/s /u {text}"
		};
		Process process2 = process;
		process2.Start();
		process2.WaitForExit();
		return process2.ExitCode == 0;
	}
}
