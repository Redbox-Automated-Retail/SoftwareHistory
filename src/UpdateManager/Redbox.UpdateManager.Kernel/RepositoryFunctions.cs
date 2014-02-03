using System;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.Lua;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Kernel;

public static class RepositoryFunctions
{
	[KernelFunction(Name = "kernel.repositoryverify")]
	internal static LuaTable Verify(string name)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		ErrorList errorList = service.Verify(name);
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		LuaTable luaTable = CreateResultTable(errorList);
		luaTable["corrupt"] = errorList.ContainsError();
		return luaTable;
	}

	[KernelFunction(Name = "kernel.repositoryreset")]
	internal static LuaTable Reset(string name)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		service.Reset(name, out var defferedDelete);
		LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
		luaTable["should_restart"] = defferedDelete;
		return luaTable;
	}

	[KernelFunction(Name = "kernel.repositoryrepair")]
	internal static LuaTable Repair(string name)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		ErrorList errorList = service.Repair(name, out var deferredMove);
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		LuaTable luaTable = CreateResultTable(errorList);
		luaTable["should_restart"] = deferredMove;
		return luaTable;
	}

	[KernelFunction(Name = "kernel.repositorygetunfinishedchanges")]
	internal static LuaTable GetOutstandingChanges(string name)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		List<IRevLog> unfinishedChanges = service.GetUnfinishedChanges(name);
		return ChangeSetToFileList(unfinishedChanges);
	}

	[KernelFunction(Name = "kernel.repositoryformatstagedfile")]
	internal static string FormatStagedFile(string path)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		return service.FormatStagedFileName(path);
	}

	[KernelFunction(Name = "kernel.repositoryactivateto")]
	internal static LuaTable ActivateTo(string name, string hash)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		ErrorList errorList = service.ActivateTo(name, hash, out var deferredMove);
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		LuaTable luaTable = CreateResultTable(errorList);
		luaTable["should_restart"] = deferredMove;
		return luaTable;
	}

	[KernelFunction(Name = "kernel.repositoryactivate")]
	internal static LuaTable Activate(string name)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		ErrorList errorList = service.ActivateToHead(name, out var deferredMove);
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		LuaTable luaTable = CreateResultTable(errorList);
		luaTable["should_restart"] = deferredMove;
		return luaTable;
	}

	[KernelFunction(Name = "kernel.repositoryupdateto")]
	internal static LuaTable UpdateTo(string name, string hash)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		ErrorList errorList = service.UpdateTo(name, hash);
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		return CreateResultTable(errorList);
	}

	[KernelFunction(Name = "kernel.repositoryupdate")]
	internal static LuaTable Update(string name)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		ErrorList errorList = service.UpdateToHead(name);
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		return CreateResultTable(errorList);
	}

	[KernelFunction(Name = "kernel.repositorypendingchanges")]
	internal static LuaTable GetPendingChanges(string name)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		List<IRevLog> pendingChanges = service.GetPendingChanges(name);
		return ChangeSetToFileList(pendingChanges);
	}

	[KernelFunction(Name = "kernel.repositorypendingactivations")]
	internal static LuaTable GetPendingActivations(string name)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		List<IRevLog> logs = new List<IRevLog>();
		try
		{
			logs = service.GetPendingActivations(name);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Unhandled exception in kernel.repositorypendingactivations.", e);
		}
		return ChangeSetToFileList(logs);
	}

	[KernelFunction(Name = "kernel.repositorylist")]
	internal static LuaTable ListRepositories()
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		List<string> allRepositories = service.GetAllRepositories();
		LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
		int num = 1;
		foreach (string item in allRepositories)
		{
			luaTable[num] = item;
			num++;
		}
		return luaTable;
	}

	[KernelFunction(Name = "kernel.repositoryisactivated")]
	internal static bool IsRepositoryActivated(string name)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		if (service == null)
		{
			return false;
		}
		try
		{
			return service.GetActiveRevision(name) != "0000000000000000000000000000000000000000";
		}
		catch (Exception)
		{
			return false;
		}
	}

	[KernelFunction(Name = "kernel.repositorygetheadrevision")]
	internal static string GetHeadRevision(string name)
	{
		return ServiceLocator.Instance.GetService<IRepositoryService>().GetHeadRevision(name);
	}

	[KernelFunction(Name = "kernel.repositorytrim")]
	internal static void Trim(string name)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		service.TrimRepository(name);
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

	private static LuaTable ChangeSetToFileList(IEnumerable<IRevLog> logs)
	{
		LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
		HashSet<string> hashSet = new HashSet<string>();
		int num = 1;
		foreach (IRevLog log in logs)
		{
			foreach (ChangeItem change in log.Changes)
			{
				string text = change.FormatFileName();
				if (!hashSet.Contains(text))
				{
					hashSet.Add(text);
					luaTable[num] = text;
					num++;
				}
			}
		}
		return luaTable;
	}
}
