using System;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Lua;

namespace Redbox.KioskServices.Kernel;

internal static class MerchantServicesFunctions
{
	[KernelFunction(Name = "kernel.merchsvcs_reconcile")]
	internal static void Reconcile(string sessionId, LuaFunction callback)
	{
		IMerchantServices service = ServiceLocator.Instance.GetService<IMerchantServices>();
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
		service.Reconcile(sessionId, callback2);
	}

	[KernelFunction(Name = "kernel.merchsvcs_preauth")]
	internal static void PreAuth(string sessionId, LuaFunction completeCallback)
	{
		IMerchantServices service = ServiceLocator.Instance.GetService<IMerchantServices>();
		if (service == null)
		{
			return;
		}
		ICallbackService callbackService = ServiceLocator.Instance.GetService<ICallbackService>();
		if (callbackService != null)
		{
			service.PreAuth(sessionId, delegate(IRemoteServiceResult r)
			{
				callbackService.EnqueueCallback(new ResultCallbackEntry
				{
					Function = completeCallback,
					Parameters = new LuaTable[1] { r.AsTableExtended() }
				});
			});
		}
	}

	[KernelFunction(Name = "kernel.merchsvcs_authorize")]
	internal static void Authorize(string sessionId, LuaFunction completeCallback)
	{
		IMerchantServices service = ServiceLocator.Instance.GetService<IMerchantServices>();
		if (service == null)
		{
			return;
		}
		ICallbackService callbackService = ServiceLocator.Instance.GetService<ICallbackService>();
		if (callbackService != null)
		{
			service.Authorize(sessionId, delegate(IRemoteServiceResult r)
			{
				callbackService.EnqueueCallback(new ResultCallbackEntry
				{
					Function = completeCallback,
					Parameters = new LuaTable[1] { r.AsTableExtended() }
				});
			});
		}
	}

	[KernelFunction(Name = "kernel.merchsvcs_gcauthorize")]
	internal static void GCAuthorize(string sessionId, LuaFunction completeCallback)
	{
		IMerchantServices service = ServiceLocator.Instance.GetService<IMerchantServices>();
		if (service == null)
		{
			return;
		}
		ICallbackService callbackService = ServiceLocator.Instance.GetService<ICallbackService>();
		if (callbackService != null)
		{
			service.GCAuthorize(sessionId, delegate(IRemoteServiceResult r)
			{
				callbackService.EnqueueCallback(new ResultCallbackEntry
				{
					Function = completeCallback,
					Parameters = new LuaTable[1] { r.AsTableExtended() }
				});
			});
		}
	}

	[KernelFunction(Name = "kernel.merchsvcs_resvauthorize")]
	internal static void ReservationAuthorize(string sessionId, string referenceNumber, LuaFunction completeCallback)
	{
		IMerchantServices service = ServiceLocator.Instance.GetService<IMerchantServices>();
		if (service == null)
		{
			return;
		}
		ICallbackService callbackService = ServiceLocator.Instance.GetService<ICallbackService>();
		if (callbackService != null)
		{
			service.ReservationAuthorize(sessionId, long.Parse(referenceNumber), delegate(IRemoteServiceResult r)
			{
				callbackService.EnqueueCallback(new ResultCallbackEntry
				{
					Function = completeCallback,
					Parameters = new LuaTable[1] { r.AsTableExtended() }
				});
			});
		}
	}

	[KernelFunction(Name = "kernel.merchsvcs_validatepickup")]
	internal static void ValidatePickup(string sessionId, LuaTable referenceNumbers, LuaFunction completeCallback)
	{
		IMerchantServices service = ServiceLocator.Instance.GetService<IMerchantServices>();
		if (service == null)
		{
			return;
		}
		ICallbackService callbackService = ServiceLocator.Instance.GetService<ICallbackService>();
		if (callbackService == null)
		{
			return;
		}
		List<long> list = new List<long>();
		foreach (object value in referenceNumbers.Values)
		{
			list.Add(Convert.ToInt64(value));
		}
		service.ValidatePickup(sessionId, list, delegate(IRemoteServiceResult r)
		{
			callbackService.EnqueueCallback(new ResultCallbackEntry
			{
				Function = completeCallback,
				Parameters = new LuaTable[1] { r.AsTable() }
			});
		});
	}

	[KernelFunction(Name = "kernel.merchsvcs_emaillookup")]
	internal static void EmailLookup(string storeNumber, string emailAddress, LuaFunction completeCallback)
	{
		IMarketingOptinServices service = ServiceLocator.Instance.GetService<IMarketingOptinServices>();
		if (service == null)
		{
			return;
		}
		ICallbackService callbackService = ServiceLocator.Instance.GetService<ICallbackService>();
		if (callbackService != null)
		{
			service.SendEmailConfirm(storeNumber, emailAddress, delegate(IRemoteServiceResult r)
			{
				callbackService.EnqueueCallback(new ResultCallbackEntry
				{
					Function = completeCallback,
					Parameters = new LuaTable[1] { r.AsTable() }
				});
			});
		}
	}
}
