using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Kernel;

public class MacroFunctions
{
	[KernelFunction(Name = "kernel.getmacrovalue")]
	internal static string GetMacroValue(string name)
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		return service[name];
	}

	[KernelFunction(Name = "kernel.expandconstantmacro")]
	internal static string ExpandConstantMacro(string value)
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		return service.ExpandProperties(value);
	}

	[KernelFunction(Name = "kernel.expandruntimemacro")]
	internal static string ExpandRuntimeMacro(string value)
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		return service.ExpandProperties(value);
	}

	[KernelFunction(Name = "kernel.setmacrovalue")]
	internal static void SetMacroValue(string name, string value)
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		service[name] = value;
	}
}
