using System;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Lua;
using Redbox.Rental.Model;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.KioskServices.Kernel;

internal static class MerchantServicesFunctions
{
	[KernelFunction(Name = "kernel.merchsvcs_reconcile")]
	internal static void Reconcile(string sessionId, string customerProfileNumber, LuaFunction callback)
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
				ICallbackService callbackService2 = callbackService;
				ResultCallbackEntry resultCallbackEntry = new ResultCallbackEntry
				{
					Function = callback
				};
				object[] parameters = new LuaTable[1] { r.AsTable() };
				resultCallbackEntry.Parameters = parameters;
				callbackService2.EnqueueCallback(resultCallbackEntry);
			};
		}
		ServiceLocator.Instance.GetService<IRentalShoppingCartService>().GetRentalShoppingCart(new Guid(sessionId)).ReValidatePromoCodeAfterVend();
		service.Reconcile(sessionId, customerProfileNumber, callback2);
	}

	[KernelFunction(Name = "kernel.merchsvcs_resvauthorize")]
	internal static void ReservationAuthorize(string sessionId, string customerProfileNumber, string referenceNumber, LuaFunction completeCallback)
	{
		IMerchantServices service = ServiceLocator.Instance.GetService<IMerchantServices>();
		if (service == null)
		{
			return;
		}
		ICallbackService callbackService = ServiceLocator.Instance.GetService<ICallbackService>();
		if (callbackService != null)
		{
			_ = ServiceLocator.Instance.GetService<IStoreManager>()?.KioskId;
			service.ReservationAuthorize(sessionId, customerProfileNumber, long.Parse(referenceNumber), delegate(IRemoteServiceResult r)
			{
				ICallbackService callbackService2 = callbackService;
				ResultCallbackEntry resultCallbackEntry = new ResultCallbackEntry
				{
					Function = completeCallback
				};
				object[] parameters = new LuaTable[1] { r.AsTableExtended() };
				resultCallbackEntry.Parameters = parameters;
				callbackService2.EnqueueCallback(resultCallbackEntry);
			});
		}
	}

	[KernelFunction(Name = "kernel.merchsvcs_validatepickup")]
	internal static void ValidatePickup(string sessionId, string customerProfileNumber, LuaTable referenceNumbers, LuaFunction completeCallback)
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
		_ = ServiceLocator.Instance.GetService<IStoreManager>()?.KioskId;
		service.ValidatePickup(sessionId, list, delegate(IRemoteServiceResult r)
		{
			ICallbackService callbackService2 = callbackService;
			ResultCallbackEntry resultCallbackEntry = new ResultCallbackEntry
			{
				Function = completeCallback
			};
			object[] parameters = new LuaTable[1] { r.AsTable() };
			resultCallbackEntry.Parameters = parameters;
			callbackService2.EnqueueCallback(resultCallbackEntry);
		});
	}

	[KernelFunction(Name = "kernel.merchsvcs_emaillookup")]
	internal static void EmailLookup(string storeNumber, string emailAddress, string accountNumber, LuaFunction completeCallback)
	{
		IMarketingOptinServices service = ServiceLocator.Instance.GetService<IMarketingOptinServices>();
		if (service == null)
		{
			return;
		}
		ICallbackService callbackService = ServiceLocator.Instance.GetService<ICallbackService>();
		if (callbackService != null)
		{
			service.SendEmailConfirm(storeNumber, emailAddress, accountNumber, delegate(IRemoteServiceResult r)
			{
				ICallbackService callbackService2 = callbackService;
				ResultCallbackEntry resultCallbackEntry = new ResultCallbackEntry
				{
					Function = completeCallback
				};
				object[] parameters = new LuaTable[1] { r.AsTable() };
				resultCallbackEntry.Parameters = parameters;
				callbackService2.EnqueueCallback(resultCallbackEntry);
			});
		}
	}

	[KernelFunction(Name = "kernel.merchsvcs_pickupauthorize")]
	internal static void PickupAuthorize(string sessionId, string customerProfileNumber, long referenceNumber, bool cartWasModified, LuaFunction callback)
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
		RemoteServiceCallback completeCallback = null;
		if (callback != null)
		{
			completeCallback = delegate(IRemoteServiceResult r)
			{
				callbackService.EnqueueCallback(new ResultCallbackEntry
				{
					Function = callback,
					Parameters = new object[2]
					{
						referenceNumber,
						r.AsTable()
					}
				});
			};
		}
		service.PickupAuthorize(sessionId, customerProfileNumber, referenceNumber, cartWasModified, completeCallback);
	}
}
