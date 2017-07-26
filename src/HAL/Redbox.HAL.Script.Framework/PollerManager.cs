using System;
using System.Collections.Generic;
using System.Reflection;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal sealed class PollerManager : IPollerService
{
	private readonly Dictionary<string, ScriptPoller> PollerMap = new Dictionary<string, ScriptPoller>();

	public ScriptPoller Locate(string name)
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

	public void InitializePollers()
	{
		using ExecutionTimer executionTimer = new ExecutionTimer();
		string text = ServiceLocator.Instance.GetService<IRuntimeService>().RuntimePath(Assembly.GetExecutingAssembly().GetName().Name + ".dll");
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
			return true;
		});
	}

	internal PollerManager()
	{
	}

	private void DoForAll(Predicate<ScriptPoller> predicate)
	{
		foreach (string key in PollerMap.Keys)
		{
			ScriptPoller obj = PollerMap[key];
			if (!predicate(obj))
			{
				break;
			}
		}
	}
}
