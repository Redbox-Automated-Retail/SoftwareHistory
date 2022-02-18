using System;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Lua;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService;

namespace Redbox.KioskServices.Kernel;

internal static class MerchantServicesFunctions
{
	[KernelFunction(Name = "kernel.merchsvcs_resvauthorize")]
	internal static void ReservationAuthorize(string sessionId, string customerProfileNumber, string referenceNumber, LuaFunction completeCallback)
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
		IKioskClientService service2 = ServiceLocator.Instance.GetService<IKioskClientService>();
		IConfiguration service3 = ServiceLocator.Instance.GetService<IConfiguration>();
		_ = ServiceLocator.Instance.GetService<IStoreManager>()?.KioskId;
		if (service3.SendReservationAuthorizeToKioskClientService)
		{
			service2.Transaction.ReservationAuthorize(sessionId, customerProfileNumber, long.Parse(referenceNumber), delegate(IRemoteServiceResult r)
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
		else
		{
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
		IConfiguration service = ServiceLocator.Instance.GetService<IConfiguration>();
		IMerchantServices service2 = ServiceLocator.Instance.GetService<IMerchantServices>();
		if (service2 == null)
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
		if (service.SendPickupValidationToKioskClientService)
		{
			ServiceLocator.Instance.GetService<IKioskClientService>().Transaction.ValidatePickup(sessionId, list, delegate(IRemoteServiceResult r)
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
		else
		{
			service2.ValidatePickup(sessionId, list, delegate(IRemoteServiceResult r)
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
		IKioskClientService service2 = ServiceLocator.Instance.GetService<IKioskClientService>();
		IConfiguration service3 = ServiceLocator.Instance.GetService<IConfiguration>();
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
		if (service3.SendPickupAuthorizeToKioskClientSerivce)
		{
			service2.Transaction.PickupAuthorize(sessionId, customerProfileNumber, referenceNumber, cartWasModified, completeCallback);
		}
		else
		{
			service.PickupAuthorize(sessionId, customerProfileNumber, referenceNumber, cartWasModified, completeCallback);
		}
	}
}
