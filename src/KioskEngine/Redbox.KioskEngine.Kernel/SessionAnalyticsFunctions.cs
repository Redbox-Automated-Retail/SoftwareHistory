using System.Collections.Generic;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;

namespace Redbox.KioskEngine.Kernel;

internal static class SessionAnalyticsFunctions
{
	[KernelFunction(Name = "kernel.analyticssessionexists")]
	internal static bool AnalyticsSessionExists(string sessionId)
	{
		return ServiceLocator.Instance.GetService<ISessionAnalyticsService>()?.SessionExists(sessionId) ?? false;
	}

	[KernelFunction(Name = "kernel.analyticsaddview")]
	internal static void AnalyticsAddView(string sessionId, string viewName)
	{
		ServiceLocator.Instance.GetService<ISessionAnalyticsService>()?.AddView(sessionId, viewName);
	}

	[KernelFunction(Name = "kernel.analyticsclose")]
	internal static void AnalyticsClose(string sessionId, string isSessionAbandoned)
	{
		ISessionAnalyticsService service = ServiceLocator.Instance.GetService<ISessionAnalyticsService>();
		if (service != null && service.SessionExists(sessionId))
		{
			service.AddElement("session", "abandon", isSessionAbandoned ?? "True", sessionId);
			service.Close(sessionId);
		}
	}

	[KernelFunction(Name = "kernel.analyticsdisposesession")]
	internal static void AnalyticsDisposeSession(string sessionId)
	{
		ServiceLocator.Instance.GetService<ISessionAnalyticsService>()?.DisposeSession(sessionId);
	}

	[KernelFunction(Name = "kernel.analyticsaddnode")]
	internal static void AnalyticsAddNode(string parentNodeName, string nodeName, string sessionId)
	{
		ISessionAnalyticsService service = ServiceLocator.Instance.GetService<ISessionAnalyticsService>();
		if (service != null && service.SessionExists(sessionId))
		{
			service.AddNode(parentNodeName, nodeName, sessionId);
		}
	}

	[KernelFunction(Name = "kernel.analyticsaddnodewithattributes")]
	internal static void AnalyticsAddNode(string parentNodeName, string nodeName, LuaTable table, string sessionId)
	{
		ISessionAnalyticsService service = ServiceLocator.Instance.GetService<ISessionAnalyticsService>();
		if (service != null && service.SessionExists(sessionId) && table != null)
		{
			Dictionary<string, string> attributes = ConvertToDictionary(table);
			service.AddNode(parentNodeName, nodeName, attributes, sessionId);
		}
	}

	[KernelFunction(Name = "kernel.analyticsaddelement")]
	internal static void AnalyticsAddElement(string parentNodeName, string elementName, string elementValue, string sessionId)
	{
		ISessionAnalyticsService service = ServiceLocator.Instance.GetService<ISessionAnalyticsService>();
		if (service != null && service.SessionExists(sessionId))
		{
			service.AddElement(parentNodeName, elementName, elementValue, sessionId);
		}
	}

	[KernelFunction(Name = "kernel.analyticsaddelements")]
	internal static void AnalyticsAddElements(string parentNodeName, LuaTable table, string sessionId)
	{
		ISessionAnalyticsService service = ServiceLocator.Instance.GetService<ISessionAnalyticsService>();
		if (service != null && service.SessionExists(sessionId))
		{
			Dictionary<string, string> elements = ConvertToDictionary(table);
			service.AddElements(parentNodeName, elements, sessionId);
		}
	}

	[KernelFunction(Name = "kernel.analyticsaddorupdateelement")]
	internal static void AnalyticsAddOrUpdateElement(string parentNodeName, string elementName, string elementValue, string sessionId)
	{
		ISessionAnalyticsService service = ServiceLocator.Instance.GetService<ISessionAnalyticsService>();
		if (service != null && service.SessionExists(sessionId))
		{
			service.AddOrUpdateElement(parentNodeName, elementName, elementValue, sessionId);
		}
	}

	[KernelFunction(Name = "kernel.analyticsaddauthtransactionInfo")]
	internal static void AnalyticsAddAuthTransactionInfo(string customerProfileNumber, string accountNumber, string transactionId, string sessionId)
	{
		ISessionAnalyticsService service = ServiceLocator.Instance.GetService<ISessionAnalyticsService>();
		if (service != null && service.SessionExists(sessionId))
		{
			service.AddAuthTransactionInfo(customerProfileNumber, accountNumber, transactionId, sessionId);
		}
	}

	[KernelFunction(Name = "kernel.analyticsremoveelement")]
	internal static void AnalyticsRemoveElement(string parentNodeName, string elementName, string sessionId)
	{
		ISessionAnalyticsService service = ServiceLocator.Instance.GetService<ISessionAnalyticsService>();
		if (service != null && service.SessionExists(sessionId))
		{
			service.RemoveElement(parentNodeName, elementName, sessionId);
		}
	}

	[KernelFunction(Name = "kernel.analyticsaddattribute")]
	internal static void AnalyticsAddAttribute(string parentNodeName, string attributeName, string attributeValue, string sessionId)
	{
		ISessionAnalyticsService service = ServiceLocator.Instance.GetService<ISessionAnalyticsService>();
		if (service != null && service.SessionExists(sessionId))
		{
			service.AddAttribute(parentNodeName, attributeName, attributeValue, sessionId);
		}
	}

	[KernelFunction(Name = "kernel.analyticsaddattributes")]
	internal static void AnalyticsAddAttributes(string parentNodeName, LuaTable table, string sessionId)
	{
		ISessionAnalyticsService service = ServiceLocator.Instance.GetService<ISessionAnalyticsService>();
		if (service != null && service.SessionExists(sessionId))
		{
			Dictionary<string, string> attributes = ConvertToDictionary(table);
			service.AddAttributes(parentNodeName, attributes, sessionId);
		}
	}

	[KernelFunction(Name = "kernel.analyticsaddviewevent")]
	internal static void AnalyticsAddViewEvent(string subItemType, string subItemValue)
	{
		ServiceLocator.Instance.GetService<ISessionAnalyticsService>()?.AddViewEvent(subItemType, subItemValue);
	}

	[KernelFunction(Name = "kernel.analyticsaddviewevent2")]
	internal static void AnalyticsAddViewEvent2(string subItemType, string subItemValue, string sessionId)
	{
		ServiceLocator.Instance.GetService<ISessionAnalyticsService>()?.AddViewEvent2(subItemType, subItemValue, sessionId);
	}

	private static Dictionary<string, string> ConvertToDictionary(LuaTable table)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (object key in table.Keys)
		{
			string text = (key as string) ?? string.Empty;
			string text2 = (table[key] as string) ?? string.Empty;
			if (text != string.Empty && text2 != string.Empty)
			{
				dictionary.Add(text, text2);
			}
		}
		return dictionary;
	}
}
