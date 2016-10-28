using System;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Lua;
using Redbox.REDS.Framework;

namespace Redbox.KioskServices.Kernel;

internal static class HealthServicesFunctions
{
	[KernelFunction(Name = "kernel.healthsvcs_ping")]
	internal static void Ping(string storeNumber)
	{
		ServiceLocator.Instance.GetService<IHealthServices>()?.Ping(storeNumber);
	}

	[KernelFunction(Name = "kernel.healthsvcs_sendalert")]
	internal static void SendAlert(string storeNumber, string alertType, string message)
	{
		InnerSendAlert(storeNumber, alertType, null, message, null);
	}

	[KernelFunction(Name = "kernel.healthsvcs_sendalert2")]
	internal static void SendAlert(string storeNumber, string alertType, string subType, string message)
	{
		InnerSendAlert(storeNumber, alertType, subType, message, null);
	}

	[KernelFunction(Name = "kernel.healthsvcs_sendalert3")]
	internal static void SendAlert(string storeNumber, string alertType, string subType, string message, LuaFunction callback)
	{
		InnerSendAlert(storeNumber, alertType, subType, message, callback);
	}

	[KernelFunction(Name = "kernel.healthsvcs_sendauditevent")]
	internal static void SendAuditEvent(string storeNumber, string eventType, string userName, string source, string message)
	{
		InnerSendAuditEvent(storeNumber, eventType, userName, source, message, null);
	}

	[KernelFunction(Name = "kernel.healthsvcs_sendauditevent2")]
	internal static void SendAuditEvent(string storeNumber, string eventType, string userName, string source, string message, LuaFunction callback)
	{
		InnerSendAuditEvent(storeNumber, eventType, userName, source, message, callback);
	}

	[KernelFunction(Name = "kernel.healthsvcs_enqueuestatistic")]
	internal static void EnqueueStatistic(string name, string value)
	{
		ServiceLocator.Instance.GetService<IHealthServices>()?.EnqueueStatistic(name, value);
	}

	[KernelFunction(Name = "kernel.healthsvcs_enqueueproperty")]
	internal static void EnqueueProperty(string name, string value)
	{
		ServiceLocator.Instance.GetService<IHealthServices>()?.EnqueueProperty(name, value);
	}

	[KernelFunction(Name = "kernel.healthsvcs_isonline")]
	internal static bool IsOnline()
	{
		return ServiceLocator.Instance.GetService<IHealthServices>()?.IsOnline ?? false;
	}

	[KernelFunction(Name = "kernel.healthsvcs_takeoffline")]
	internal static void TakeOffline()
	{
		IHealthServices service = ServiceLocator.Instance.GetService<IHealthServices>();
		if (service != null)
		{
			service.IsOnline = false;
		}
	}

	[KernelFunction(Name = "kernel.healthsvcs_bringonline")]
	internal static void BringOnline()
	{
		IHealthServices service = ServiceLocator.Instance.GetService<IHealthServices>();
		if (service != null)
		{
			service.IsOnline = true;
		}
	}

	[KernelFunction(Name = "kernel.healthsvcs_queuedepth")]
	internal static int QueueDepth()
	{
		IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
		if (service == null)
		{
			return 0;
		}
		service.GetDepth(out var count);
		return count;
	}

	[KernelFunction(Name = "kernel.getsoftwareversions")]
	internal static LuaTable GetSoftwareVersions()
	{
		LuaTable luaTable = (LuaTable)ServiceLocator.Instance.GetService<IKernelService>().NewTable();
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		if (service != null)
		{
			luaTable["Kiosk Engine"] = service["EngineVersion"];
			luaTable["Hal Service"] = service["HALServiceVersion"] ?? "Not Found";
			luaTable["Hal Client"] = service["HALVersion"] ?? "Not Found";
		}
		IKernelExtensionService service2 = ServiceLocator.Instance.GetService<IKernelExtensionService>();
		if (service2 != null)
		{
			foreach (IKernelExtension extension in service2.Extensions)
			{
				luaTable["[Extension] " + extension.Name] = extension.Version;
			}
		}
		IResourceBundleService service3 = ServiceLocator.Instance.GetService<IResourceBundleService>();
		if (service3 != null)
		{
			foreach (IBundleSpecifier bundle in service3.Bundles)
			{
				string key = "[" + bundle.Type.ToString() + " bundle] " + bundle.Name;
				luaTable[key] = bundle.Version.ToString();
			}
		}
		return luaTable;
	}

	private static void InnerSendAlert(string storeNumber, string alertType, string subType, string message, LuaFunction callback)
	{
		IHealthServices service = ServiceLocator.Instance.GetService<IHealthServices>();
		if (service == null)
		{
			return;
		}
		ICallbackService callbackService = ServiceLocator.Instance.GetService<ICallbackService>();
		if (callbackService == null)
		{
			return;
		}
		RemoteServiceCallback callback2 = null;
		if (callback != null)
		{
			callback2 = delegate(IRemoteServiceResult r)
			{
				callbackService.EnqueueCallback(new ResultCallbackEntry
				{
					Function = callback,
					Parameters = new LuaTable[1] { r.AsTable() }
				});
			};
		}
		service.SendAlert(storeNumber, alertType, subType, message, DateTime.Now, callback2);
	}

	private static void InnerSendAuditEvent(string storeNumber, string eventType, string userName, string source, string message, LuaFunction callback)
	{
		IHealthServices service = ServiceLocator.Instance.GetService<IHealthServices>();
		if (service == null)
		{
			return;
		}
		ICallbackService callbackService = ServiceLocator.Instance.GetService<ICallbackService>();
		if (callbackService == null)
		{
			return;
		}
		RemoteServiceCallback callback2 = null;
		if (callback != null)
		{
			callback2 = delegate(IRemoteServiceResult r)
			{
				callbackService.EnqueueCallback(new ResultCallbackEntry
				{
					Function = callback,
					Parameters = new LuaTable[1] { r.AsTable() }
				});
			};
		}
		service.SendAuditEvent(storeNumber, eventType, DateTime.Now, userName, message, source, callback2);
	}
}
