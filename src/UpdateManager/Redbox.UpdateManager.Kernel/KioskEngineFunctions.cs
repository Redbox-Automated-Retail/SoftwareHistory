using Redbox.Core;
using Redbox.Lua;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Kernel;

public static class KioskEngineFunctions
{
	[KernelFunction(Name = "kernel.kioskengineexportqueue")]
	internal static LuaTable ExportQueue(string path)
	{
		IKioskEngineService service = ServiceLocator.Instance.GetService<IKioskEngineService>();
		ErrorList errorList = service.ExportQueue(path);
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		return CreateResultTable(errorList);
	}

	[KernelFunction(Name = "kernel.kioskengineexportsession")]
	internal static LuaTable ExportSession(string path)
	{
		IKioskEngineService service = ServiceLocator.Instance.GetService<IKioskEngineService>();
		ErrorList errorList = service.ExportSessions(path);
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		return CreateResultTable(errorList);
	}

	[KernelFunction(Name = "kernel.kioskenginestart")]
	internal static LuaTable StartEngine()
	{
		IKioskEngineService service = ServiceLocator.Instance.GetService<IKioskEngineService>();
		ErrorList errorList = service.StartEngine();
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		return CreateResultTable(errorList);
	}

	[KernelFunction(Name = "kernel.kioskengineisrunning")]
	internal static LuaTable IsEngineRunning()
	{
		IKioskEngineService service = ServiceLocator.Instance.GetService<IKioskEngineService>();
		ErrorList errorList = service.IsEngineRunning(out var isRunning);
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		LuaTable luaTable = CreateResultTable(errorList);
		luaTable["isrunning"] = isRunning;
		return luaTable;
	}

	[KernelFunction(Name = "kernel.kioskenginestartwithbundle")]
	internal static LuaTable StartEngine(string bundle)
	{
		IKioskEngineService service = ServiceLocator.Instance.GetService<IKioskEngineService>();
		ErrorList errorList = service.StartEngine(bundle);
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		return CreateResultTable(errorList);
	}

	[KernelFunction(Name = "kernel.kioskenginechangebundle")]
	internal static LuaTable ChangeBundle(string bundle)
	{
		IKioskEngineService service = ServiceLocator.Instance.GetService<IKioskEngineService>();
		ErrorList errorList = service.ChangeBundle(bundle);
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		return CreateResultTable(errorList);
	}

	[KernelFunction(Name = "kernel.kioskengineshutdownblocking")]
	public static LuaTable ShutdownBlocking(int timeout, int tries)
	{
		IKioskEngineService service = ServiceLocator.Instance.GetService<IKioskEngineService>();
		ErrorList errorList = service.Shutdown(timeout, tries);
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		return CreateResultTable(errorList);
	}

	[KernelFunction(Name = "kernel.kioskengineshutdown")]
	internal static LuaTable Shutdown()
	{
		IKioskEngineService service = ServiceLocator.Instance.GetService<IKioskEngineService>();
		ErrorList errorList = service.Shutdown();
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		return CreateResultTable(errorList);
	}

	[KernelFunction(Name = "kernel.kioskenginebringtofront")]
	internal static LuaTable BringToFront()
	{
		IKioskEngineService service = ServiceLocator.Instance.GetService<IKioskEngineService>();
		ErrorList errorList = service.BringToFront();
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		return CreateResultTable(errorList);
	}

	[KernelFunction(Name = "kernel.kioskengineexecutescript")]
	internal static LuaTable ExecuteScript(string path)
	{
		IKioskEngineService service = ServiceLocator.Instance.GetService<IKioskEngineService>();
		ErrorList errorList = service.ExecuteScript(path);
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		return CreateResultTable(errorList);
	}

	[KernelFunction(Name = "kernel.kioskenginereloadbundle")]
	internal static LuaTable ReloadBundle()
	{
		IKioskEngineService service = ServiceLocator.Instance.GetService<IKioskEngineService>();
		ErrorList errorList = service.ReloadBundle();
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		return CreateResultTable(errorList);
	}

	[KernelFunction(Name = "kernel.kioskenginegetmemoryusage")]
	internal static long GetMemoryUsage(string path)
	{
		IKioskEngineService service = ServiceLocator.Instance.GetService<IKioskEngineService>();
		ErrorList memoryUsage = service.GetMemoryUsage(out var total);
		if (memoryUsage.ContainsError())
		{
			memoryUsage.ForEach(delegate(Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		return total;
	}

	[KernelFunction(Name = "kernel.kioskengineshowofflinescreen")]
	internal static LuaTable ShowOfflineScreen()
	{
		IKioskEngineService service = ServiceLocator.Instance.GetService<IKioskEngineService>();
		ErrorList errorList = service.ShowOfflineScreen();
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		return CreateResultTable(errorList);
	}

	private static LuaTable CreateResultTable(ErrorList errors)
	{
		LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
		luaTable["success"] = !errors.ContainsError();
		LuaTable luaTable2 = new LuaTable(KernelService.Instance.LuaRuntime);
		foreach (Error error in errors)
		{
			luaTable2[error.Code] = error.Description;
		}
		luaTable["errors"] = luaTable2;
		return luaTable;
	}
}
