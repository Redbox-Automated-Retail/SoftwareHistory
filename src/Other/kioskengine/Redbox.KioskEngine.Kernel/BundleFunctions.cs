using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Xml;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.REDS.Framework;

namespace Redbox.KioskEngine.Kernel;

internal static class BundleFunctions
{
	[KernelFunction(Name = "kernel.activatebundle")]
	internal static void ActivateBundle(string productName, string productVersion)
	{
		try
		{
			IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
			service.Activate(productName, productVersion);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Error in Kernel Function activatebundle.", e);
		}
	}

	[KernelFunction(Name = "kernel.activatedefaultbundle")]
	internal static void ActivateBundleDefault()
	{
		try
		{
			IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
			service.ActivateDefaultBundle();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Error in Kernel Function activatedefaultbundle.", e);
		}
	}

	[KernelFunction(Name = "kernel.getresourcestoragename")]
	internal static string GetResourceStorageName(string name)
	{
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		if (service == null)
		{
			return null;
		}
		if (string.IsNullOrEmpty(name))
		{
			return null;
		}
		return service.GetResource(name)?.StorageEntry.Path;
	}

	[KernelFunction(Name = "kernel.getresourceaspectproperty")]
	internal static string GetResourceAspectProperty(string resourceName, string aspectName, string propertyName)
	{
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		if (service == null)
		{
			return null;
		}
		if (string.IsNullOrEmpty(resourceName) || string.IsNullOrEmpty(aspectName) || string.IsNullOrEmpty(propertyName))
		{
			return null;
		}
		IResource resource = service.GetResource(resourceName);
		if (resource == null)
		{
			return null;
		}
		IAspect aspect = resource.GetAspect(aspectName);
		if (aspect == null)
		{
			return null;
		}
		if (!aspect.Properties.ContainsKey(propertyName))
		{
			return null;
		}
		return aspect.Properties[propertyName];
	}

	[KernelFunction(Name = "kernel.isresourceoverridden")]
	internal static bool IsResourceOverridden(string name, object exclusions)
	{
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		if (service == null)
		{
			return false;
		}
		if (string.IsNullOrEmpty(name))
		{
			return false;
		}
		List<string> list = new List<string>();
		if (exclusions is string item)
		{
			list.Add(item);
		}
		if (exclusions is LuaTable luaTable)
		{
			foreach (object value in luaTable.Values)
			{
				if (value != null)
				{
					list.Add(value.ToString());
				}
			}
		}
		return service.GetResource(name)?.IsOverriden(list.AsReadOnly()) ?? false;
	}

	[KernelFunction(Name = "kernel.executescript")]
	internal static object[] ExecuteScript(string name)
	{
		List<object> list = new List<object>();
		ReadOnlyCollection<object> readOnlyCollection = KernelService.Instance.ExecuteScript(name);
		if (readOnlyCollection != null)
		{
			list.AddRange(readOnlyCollection);
		}
		return list.ToArray();
	}

	[KernelFunction(Name = "kernel.getresourceproperty")]
	internal static object GetResourceProperty(string resourceName, string propertyName)
	{
		return ServiceLocator.Instance.GetService<IResourceBundleService>()?.GetResource(resourceName)?[propertyName];
	}

	[KernelFunction(Name = "kernel.gettable")]
	internal static LuaTable GetTable(string name)
	{
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		XmlNode xml = service.GetXml(name);
		if (xml == null)
		{
			return null;
		}
		XmlNodeList xmlNodeList = xml.SelectNodes("row");
		if (xmlNodeList == null)
		{
			return null;
		}
		LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
		decimal num = 1m;
		foreach (XmlNode item in xmlNodeList)
		{
			XmlNodeList xmlNodeList2 = item.SelectNodes("column");
			if (xmlNodeList2 == null)
			{
				continue;
			}
			LuaTable luaTable2 = new LuaTable(KernelService.Instance.LuaRuntime);
			foreach (XmlNode item2 in xmlNodeList2)
			{
				string attributeValue = XmlNodeExtensions.GetAttributeValue<string>(item2, "name");
				object value = XmlNodeExtensions.GetNodeValue(item2, string.Empty);
				string attributeValue2 = XmlNodeExtensions.GetAttributeValue<string>(item2, "type");
				if (!string.IsNullOrEmpty(attributeValue2))
				{
					Type type = StringExtensions.ToType(attributeValue2);
					if (type != null)
					{
						value = ConversionHelper.ChangeType(value, type);
					}
				}
				luaTable2[attributeValue] = value;
			}
			luaTable[num++] = luaTable2;
		}
		return luaTable;
	}

	[KernelFunction(Name = "kernel.getdictionary")]
	internal static LuaTable GetDictionary(string name)
	{
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		XmlNode xml = service.GetXml(name);
		if (xml == null)
		{
			return null;
		}
		XmlNodeList xmlNodeList = xml.SelectNodes("row");
		if (xmlNodeList == null)
		{
			return null;
		}
		LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
		foreach (XmlNode item in xmlNodeList)
		{
			XmlNodeList xmlNodeList2 = item.SelectNodes("column");
			if (xmlNodeList2 == null)
			{
				continue;
			}
			foreach (XmlNode item2 in xmlNodeList2)
			{
				string attributeValue = XmlNodeExtensions.GetAttributeValue<string>(item2, "name");
				object value = XmlNodeExtensions.GetNodeValue(item2, string.Empty);
				string attributeValue2 = XmlNodeExtensions.GetAttributeValue<string>(item2, "type");
				if (!string.IsNullOrEmpty(attributeValue2))
				{
					Type type = StringExtensions.ToType(attributeValue2);
					if (type != null)
					{
						value = ConversionHelper.ChangeType(value, type);
					}
				}
				luaTable[attributeValue] = value;
			}
		}
		return luaTable;
	}

	[KernelFunction(Name = "kernel.getfilter")]
	internal static string GetFilterProperty(string name)
	{
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		return service.Filter[name];
	}

	[KernelFunction(Name = "kernel.setfilter")]
	internal static void SetFilterProperty(string name, string value)
	{
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		service.Filter[name] = value;
	}

	[Obsolete("Kernel function is deprecated in favor of kernel.getfilter.")]
	[KernelFunction(Name = "kernel.getvendor")]
	internal static string GetVendor()
	{
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		return service.Filter["vendor"];
	}

	[Obsolete("Kernel function is deprecated in favor of kernel.setfilter.")]
	[KernelFunction(Name = "kernel.setvendor")]
	internal static void SetVendor(string vendor)
	{
		ServiceLocator.Instance.GetService<IFlashService>()?.ClearCache();
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		service.Filter["vendor"] = vendor;
		ResetCurrentView();
	}

	[KernelFunction(Name = "kernel.getmarket")]
	[Obsolete("Kernel function is deprecated in favor of kernel.getfilter.")]
	internal static string GetMarket()
	{
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		return service.Filter["market"];
	}

	[KernelFunction(Name = "kernel.setmarket")]
	[Obsolete("Kernel function is deprecated in favor of kernel.setfilter.")]
	internal static void SetMarket(string market)
	{
		ServiceLocator.Instance.GetService<IFlashService>()?.ClearCache();
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		service.Filter["market"] = market;
		ResetCurrentView();
	}

	[KernelFunction(Name = "kernel.getstorenumber")]
	[Obsolete("Kernel function is deprecated in favor of kernel.getfilter.")]
	internal static string GetStoreNumber()
	{
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		return service.Filter["store_number"];
	}

	[KernelFunction(Name = "kernel.setstorenumber")]
	[Obsolete("Kernel function is deprecated in favor of kernel.setfilter.")]
	internal static void SetStoreNumber(string storeNumber)
	{
		ServiceLocator.Instance.GetService<IFlashService>()?.ClearCache();
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		service.Filter["store_number"] = storeNumber;
		ResetCurrentView();
	}

	[KernelFunction(Name = "kernel.getculture")]
	[Obsolete("Kernel function is deprecated in favor of kernel.getfilter.")]
	internal static string GetCulture()
	{
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		return service.Filter["culture"];
	}

	[KernelFunction(Name = "kernel.setculture")]
	[Obsolete("Kernel function is deprecated in favor of kernel.setfilter.")]
	internal static void SetCulture(string culture)
	{
		ServiceLocator.Instance.GetService<IFlashService>()?.ClearCache();
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		service.Filter["culture"] = culture;
		ResetCurrentView();
	}

	[Obsolete("Kernel function is deprecated in favor of kernel.getfilter.")]
	[KernelFunction(Name = "kernel.getenvironment")]
	internal static string GetEnvironment()
	{
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		return service.Filter["environment"];
	}

	[Obsolete("Kernel function is deprecated in favor of kernel.setfilter.")]
	[KernelFunction(Name = "kernel.setenvironment")]
	internal static void SetEnvironment(string environment)
	{
		ServiceLocator.Instance.GetService<IFlashService>()?.ClearCache();
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		service.Filter["environment"] = environment;
		ResetCurrentView();
	}

	[Obsolete("Kernel function is deprecated in favor of kernel.getfilter.")]
	[KernelFunction(Name = "kernel.getruntime")]
	internal static string GetRuntime()
	{
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		return service.Filter["runtime"];
	}

	[KernelFunction(Name = "kernel.getplatform")]
	[Obsolete("Kernel function is deprecated in favor of kernel.getfilter.")]
	internal static string GetPlatform()
	{
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		return service.Filter["platform"];
	}

	[KernelFunction(Name = "kernel.getapplication")]
	[Obsolete("Kernel function is deprecated in favor of kernel.getfilter.")]
	internal static string GetApplication()
	{
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		return service.Filter["application"];
	}

	[KernelFunction(Name = "kernel.getswitchstatusmessage")]
	internal static string GetSwitchStatusMessage()
	{
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		return service.SwitchStatusMessage;
	}

	[KernelFunction(Name = "kernel.setswitchstatusmessage")]
	internal static void SetSwitchStatusMessage(string message)
	{
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		service.SwitchStatusMessage = message;
	}

	[KernelFunction(Name = "kernel.isculturevalid")]
	internal static bool IsCultureValid(string culture)
	{
		return CultureInfo.GetCultures(CultureTypes.SpecificCultures).Any((CultureInfo c) => c.Name == culture);
	}

	private static void ResetCurrentView()
	{
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		service.ClearCache();
		service.Show();
	}
}
