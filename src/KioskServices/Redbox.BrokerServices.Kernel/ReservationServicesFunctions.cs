using Redbox.BrokerServices.Proxy.ComponentModel;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService;

namespace Redbox.BrokerServices.Kernel;

internal static class ReservationServicesFunctions
{
	[KernelFunction(Name = "kernel.resvsvcs_cancel")]
	internal static void Cancel(string storeNumber, long referenceNumber, string reasonCode, LuaFunction callback)
	{
		IReservationServices service = ServiceLocator.Instance.GetService<IReservationServices>();
		if (service != null && ServiceLocator.Instance.GetService<ICallbackService>() != null)
		{
			RemoteServiceCallback callback2 = null;
			if (ServiceLocator.Instance.GetService<IConfiguration>().SendExpireAndCancelReservationToKioskClientService)
			{
				ServiceLocator.Instance.GetService<IKioskClientService>().Transaction.CancelReservation(storeNumber, referenceNumber, reasonCode, callback2);
			}
			else
			{
				service.Cancel(storeNumber, referenceNumber, callback2);
			}
		}
	}
}
