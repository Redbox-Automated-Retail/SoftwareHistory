using System.IO;
using Redbox.Core;
using Redbox.Lua;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Kernel;

public static class InputFunctions
{
	[KernelFunction(Name = "kernel.inputyesno")]
	internal static string AskYesNoQuestion(string title, string message)
	{
		IInputService service = ServiceLocator.Instance.GetService<IInputService>();
		if (service != null)
		{
			return service.AskYesNoQuestion(title, message).ToString("G").ToLower();
		}
		return null;
	}

	[KernelFunction(Name = "kernel.inputfindfile")]
	internal static LuaTable FindFile()
	{
		IInputService service = ServiceLocator.Instance.GetService<IInputService>();
		if (service == null)
		{
			return null;
		}
		FileInfo fileInfo = service.LocateFile();
		if (fileInfo == null)
		{
			return null;
		}
		LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
		luaTable["name"] = fileInfo.Name;
		luaTable["length"] = fileInfo.Length;
		luaTable["directory"] = fileInfo.DirectoryName;
		return luaTable;
	}

	[KernelFunction(Name = "kernel.notifyinfo")]
	internal static void NotifyInfo(string title, string message)
	{
		IInputService service = ServiceLocator.Instance.GetService<IInputService>();
		service.NotifyInfo(title, message);
	}
}
