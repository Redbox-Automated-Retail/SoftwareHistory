using System;
using System.Diagnostics;
using System.Reflection;
using Redbox.Core;
using Redbox.Lua;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Kernel;

public static class FileVersionFunctions
{
	[KernelFunction(Name = "kernel.getfileversion")]
	internal static string GetFileVersion(string fileName)
	{
		try
		{
			IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
			FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(service.ExpandProperties(fileName));
			return versionInfo.FileVersion;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Error getting file version.", e);
			return string.Empty;
		}
	}

	[KernelFunction(Name = "kernel.getfileversiontable")]
	internal static LuaTable GetFileVersionInfo(string fileName)
	{
		try
		{
			IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
			FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(service.ExpandProperties(fileName));
			LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
			luaTable["file_version"] = versionInfo.FileVersion;
			luaTable["company_name"] = versionInfo.CompanyName;
			luaTable["comments"] = versionInfo.Comments;
			luaTable["build_version"] = versionInfo.FileBuildPart;
			luaTable["major_version"] = versionInfo.FileMajorPart;
			luaTable["minor_version"] = versionInfo.FileMinorPart;
			luaTable["private_version"] = versionInfo.FilePrivatePart;
			luaTable["copyright"] = versionInfo.LegalCopyright;
			return luaTable;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Error getting file version.", e);
			return null;
		}
	}

	[KernelFunction(Name = "kernel.getdotnetassemblyversion")]
	internal static string GetAssemblyVersion(string file)
	{
		string result = string.Empty;
		try
		{
			IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
			result = AssemblyInfoHelper.GetVersion(Assembly.LoadFile(service.ExpandProperties(file)));
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Error getting file version.", e);
		}
		return result;
	}

	[KernelFunction(Name = "kernel.getfrontendversion")]
	internal static string GetFrontendVersion()
	{
		return Assembly.GetExecutingAssembly().GetName().Version.ToString();
	}
}
