using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Redbox.Core;

namespace Redbox.IPC.Framework;

[Redbox.IPC.Framework.Command("command-repository")]
[Description("The COMMAND-REPOSITORY command scans the execution directory of the cache service for .NET assembly files (.dll and .exe) that contain types marked with the CommandAttribute class.")]
internal class CommandRepository
{
	private static readonly IDictionary<string, Redbox.IPC.Framework.CommandInstance> m_commandInstanceCache = new Dictionary<string, Redbox.IPC.Framework.CommandInstance>();

	public static string[] AssemblyPatterns { get; set; }

	internal static ICollection<string> AllCommands => m_commandInstanceCache.Keys;

	public void Default(Redbox.IPC.Framework.CommandContext context)
	{
		List<string> list = DiscoverInstalledCommands();
		foreach (string item in list)
		{
			context.Messages.Add("Installed Command '" + item + "'.");
		}
	}

	public static void InstallPerformanceCounters(string groupName)
	{
		List<string> list = new List<string>();
		DiscoverInstalledCommands();
		foreach (KeyValuePair<string, Redbox.IPC.Framework.CommandInstance> item in m_commandInstanceCache)
		{
			LogHelper.Instance.Log("Creating performance counters for command {0}.", item.Key);
			foreach (string key in item.Value.FormMethodCache.Keys)
			{
				list.Add($"{item.Key.ToUpper()} {key.ToLower()}");
			}
		}
		Redbox.IPC.Framework.PerformanceCounterSetup.Initialize(groupName, list);
	}

	public static void UninstallPerformanceCounters(string groupName)
	{
		if (PerformanceCounterCategory.Exists(groupName))
		{
			PerformanceCounterCategory.Delete(groupName);
		}
	}

	internal static List<string> DiscoverInstalledCommands()
	{
		using ExecutionTimer executionTimer = new ExecutionTimer();
		List<string> list = new List<string>();
		string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		List<string> list2 = new List<string>();
		if (AssemblyPatterns == null || AssemblyPatterns.Length == 0)
		{
			list2.AddRange(Directory.GetFiles(directoryName, "*.dll"));
			list2.AddRange(Directory.GetFiles(directoryName, "*.exe"));
		}
		else
		{
			string[] assemblyPatterns = AssemblyPatterns;
			foreach (string text in assemblyPatterns)
			{
				if (text.IndexOf("*") == -1 && text.IndexOf("?") == -1)
				{
					list2.Add(Path.Combine(directoryName, text));
				}
				else
				{
					list2.AddRange(Directory.GetFiles(directoryName, text));
				}
			}
		}
		foreach (string item in list2)
		{
			if (!File.Exists(item))
			{
				continue;
			}
			try
			{
				Assembly assembly = Assembly.LoadFrom(item);
				if (assembly != null)
				{
					LogHelper.Instance.Log("Load commands from assembly: '{0}'", assembly);
					list.AddRange(FindCommands(assembly.GetTypes()));
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log($"Unable to load assembly '{item}' to scan for commands.", e);
			}
		}
		LogHelper.Instance.Log($"Time to scan for commands: {executionTimer.Elapsed}", LogEntryType.Info);
		return list;
	}

	internal static Redbox.IPC.Framework.CommandInstance GetCommand(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return null;
		}
		string key = name.ToUpper();
		if (!m_commandInstanceCache.ContainsKey(key))
		{
			return null;
		}
		return m_commandInstanceCache[key];
	}

	private static List<string> FindCommands(IEnumerable<Type> types)
	{
		List<string> list = new List<string>();
		foreach (Type type in types)
		{
			Redbox.IPC.Framework.CommandAttribute[] array = (Redbox.IPC.Framework.CommandAttribute[])type.GetCustomAttributes(typeof(Redbox.IPC.Framework.CommandAttribute), inherit: false);
			Redbox.IPC.Framework.CommandAttribute[] array2 = array;
			foreach (Redbox.IPC.Framework.CommandAttribute commandAttribute in array2)
			{
				if (commandAttribute.Name != null)
				{
					string text = commandAttribute.Name.ToUpper();
					list.Add(text);
					m_commandInstanceCache[text] = Redbox.IPC.Framework.CommandInstance.GetCommandInstance(type);
				}
			}
		}
		return list;
	}
}
