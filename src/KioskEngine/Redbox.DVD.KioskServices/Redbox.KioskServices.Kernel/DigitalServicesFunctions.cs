using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Lua;

namespace Redbox.KioskServices.Kernel;

internal static class DigitalServicesFunctions
{
	[KernelFunction(Name = "kernel.digitalsvcs_getcustomercredits")]
	internal static void GetCustomerCredits(string sessionId, LuaFunction callback)
	{
		IDigitalServices service = ServiceLocator.Instance.GetService<IDigitalServices>();
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
		service.GetCustomerCredits(sessionId, callback2);
	}

	[KernelFunction(Name = "kernel.digitalsvcs_getcustomer")]
	internal static void GetCustomer(string sessionId, string cardId, LuaFunction callback)
	{
		IDigitalServices service = ServiceLocator.Instance.GetService<IDigitalServices>();
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
		service.GetCustomer(sessionId, cardId, callback2);
	}

	[KernelFunction(Name = "kernel.digitalsvcs_createrbifreetrial")]
	internal static void CreateRBIFreeTrial(string sessionId, string emailAddress, string status, LuaFunction callback)
	{
		IDigitalServices service = ServiceLocator.Instance.GetService<IDigitalServices>();
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
		service.CreateRBIFreeTrial(sessionId, emailAddress, status, callback2);
	}
}
