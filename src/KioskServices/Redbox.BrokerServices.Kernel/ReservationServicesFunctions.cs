using Redbox.BrokerServices.Proxy.ComponentModel;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;

namespace Redbox.BrokerServices.Kernel;

internal static class ReservationServicesFunctions
{
	[KernelFunction(Name = "kernel.resvsvcs_reset")]
	internal static void Reset()
	{
		IReservationServices service = ServiceLocator.Instance.GetService<IReservationServices>();
		if (service != null)
		{
			LogHelper.Instance.Log("> Broker Connection Reset/Close Requested");
			service.Reset();
		}
	}

	[KernelFunction(Name = "kernel.resvsvcs_cancel")]
	internal static void Cancel(string storeNumber, long referenceNumber, LuaFunction callback)
	{
		IReservationServices service = ServiceLocator.Instance.GetService<IReservationServices>();
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
		service.Cancel(storeNumber, referenceNumber, callback2);
	}
}
