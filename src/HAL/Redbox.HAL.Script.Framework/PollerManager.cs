using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal sealed class PollerManager
{
	private delegate bool PollersPredicate(ScriptPoller poller);

	private readonly Dictionary<string, ScriptPoller> PollerMap = new Dictionary<string, ScriptPoller>();

	internal static PollerManager Instance => Singleton<PollerManager>.Instance;

	internal void Shutdown()
	{
		DoForAll(delegate(ScriptPoller poller)
		{
			poller.Shutdown();
			return true;
		});
	}

	internal void OnEngineModeChange(EngineModes newMode)
	{
		DoForAll(delegate(ScriptPoller poller)
		{
			poller.OnEngineModeChange(newMode);
			return true;
		});
	}

	internal void ConfigurationUpdateStart()
	{
		DoForAll(delegate(ScriptPoller poller)
		{
			poller.OnConfigChangeStart();
			return true;
		});
	}

	internal void ConfigurationUpdateFinish()
	{
		DoForAll(delegate(ScriptPoller poller)
		{
			poller.OnConfigChangeComplete();
			return true;
		});
	}

	internal ScriptPoller Locate(string name)
	{
		foreach (string key in PollerMap.Keys)
		{
			if (key == name)
			{
				return PollerMap[key];
			}
		}
		return null;
	}

	internal void RegisterPollers()
	{
		using ExecutionTimer executionTimer = new ExecutionTimer();
		string text = Path.Combine(RuntimeService.Instance.AssemblyDirectory, Assembly.GetExecutingAssembly().GetName().Name + ".dll");
		Assembly assembly = null;
		try
		{
			assembly = Assembly.LoadFrom(text);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"[PollerManager] Unable to load assembly '{text}' to scan for commands.", e);
			return;
		}
		Type[] types = assembly.GetTypes();
		foreach (Type type in types)
		{
			PollerAttribute[] array = (PollerAttribute[])type.GetCustomAttributes(typeof(PollerAttribute), inherit: false);
			if (array != null && array.Length != 0 && !string.IsNullOrEmpty(array[0].Name))
			{
				try
				{
					ScriptPoller value = Activator.CreateInstance(type, nonPublic: true) as ScriptPoller;
					PollerMap[array[0].Name] = value;
					LogHelper.Instance.Log("[PollerManager] Registered poller {0}", array[0].Name);
				}
				catch (Exception e2)
				{
					LogHelper.Instance.Log($"[PollerManager] Unable to create type {type.ToString()}", e2);
				}
			}
		}
		LogHelper.Instance.Log($"[PollerManager] Time to scan for {PollerMap.Keys.Count} pollers: {executionTimer.Elapsed}");
		DoForAll(delegate(ScriptPoller poller)
		{
			poller.Initialize();
			poller.Start();
			return true;
		});
	}

	private void DoForAll(PollersPredicate predicate)
	{
		foreach (string key in PollerMap.Keys)
		{
			ScriptPoller poller = PollerMap[key];
			if (!predicate(poller))
			{
				break;
			}
		}
	}

	private PollerManager()
	{
	}
}
