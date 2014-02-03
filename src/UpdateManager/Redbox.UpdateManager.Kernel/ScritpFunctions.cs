using System.IO;
using System.Linq;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Kernel;

public static class ScritpFunctions
{
	[KernelFunction(Name = "kernel.markscriptcomplete")]
	internal static void SetScriptComplete()
	{
		KernelService.Instance.SetScriptComplete();
	}

	[KernelFunction(Name = "kernel.runscript")]
	internal static object[] RunScript(string path)
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		string chunk = File.ReadAllText(service.ExpandProperties(path));
		return KernelService.Instance.LuaRuntime.DoString(chunk).ToArray();
	}
}
