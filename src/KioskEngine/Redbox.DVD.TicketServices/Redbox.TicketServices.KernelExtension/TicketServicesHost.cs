using System;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.TicketServices.Kernel;
using Redbox.TicketServices.Proxy;
using Redbox.TicketServices.Proxy.ComponentModel;

namespace Redbox.TicketServices.KernelExtension;

public class TicketServicesHost : IKernelExtensionHost, IDisposable
{
	public IKernelExtension Extension { get; set; }

	public void Dispose()
	{
	}

	public bool CanSwitch(out ErrorList errors)
	{
		errors = new ErrorList();
		LogHelper.Instance.Log("TicketService CanSwitch: True");
		return true;
	}

	public ErrorList PreLoad()
	{
		return new ErrorList();
	}

	public ErrorList Load()
	{
		ErrorList errorList = new ErrorList();
		LogHelper.Instance.Log("Ticket Services Kernel Extension Load:");
		ServiceLocator.Instance.AddService(typeof(ITicketServices), TicketServiceProxy.Instance);
		IPreferenceService service = ServiceLocator.Instance.GetService<IPreferenceService>();
		if (service != null)
		{
			service.AddPreferencePage("TicketServices_General", "Remote Services\\Ticket Services", "Ticket Services/General", PreferencePageTarget.LocalSystem, new GeneralPrefPage());
			service.AddPreferencePage("TicketServices_Email", "Remote Services\\Ticket Services", "Ticket Services/Email Messages", PreferencePageTarget.LocalSystem, new EmailPrefPage());
			service.AddPreferencePage("TicketServices_Locking", "Remote Services\\Ticket Services", "Ticket Services/Locking", PreferencePageTarget.LocalSystem, new LockingPrefPage());
			service.AddPreferencePage("TicketServices_PlaceOrder", "Remote Services\\Ticket Services", "Ticket Services/Place Order", PreferencePageTarget.LocalSystem, new PlaceOrderPrefPage());
			service.AddPreferencePage("TicketServices_GetTickets", "Remote Services\\Ticket Services", "Ticket Services/Get Tickets", PreferencePageTarget.LocalSystem, new GetTicketsPrefPage());
		}
		errorList.AddRange(TicketDataService.Instance.Initialize());
		LogHelper.Instance.Log("...Loaded.");
		return errorList;
	}

	public ErrorList Reset()
	{
		ErrorList result = new ErrorList();
		LogHelper.Instance.Log("Ticket Services Kernel Extension Reset:");
		IKernelFunctionRegistryService service = ServiceLocator.Instance.GetService<IKernelFunctionRegistryService>();
		if (service != null)
		{
			LogHelper.Instance.Log("...Register Ticket Services kernel functions.");
			service.RegisterKernelFunctions(typeof(TicketServicesFunctions).Assembly, "Ticket Services");
			LogHelper.Instance.Log("...Register Ticket Data Service kernel functions.");
			service.RegisterKernelFunctions(typeof(TicketDataServiceFunctions).Assembly, "Ticket Data Service");
		}
		return result;
	}

	public ErrorList Activate()
	{
		ErrorList errorList = new ErrorList();
		LogHelper.Instance.Log("Ticket Services Kernel Extension Activate:");
		errorList.AddRange(TicketDataService.Instance.Start());
		LogHelper.Instance.Log("...Activated.");
		return errorList;
	}

	public ErrorList Deactivate()
	{
		ErrorList errorList = new ErrorList();
		LogHelper.Instance.Log("Ticket Services Kernel Extension Deactivate:");
		errorList.AddRange(TicketDataService.Instance.Stop());
		LogHelper.Instance.Log("...Deactivated.");
		return errorList;
	}

	public void HandleHostCrash(Exception e)
	{
		LogHelper.Instance.Log("Ticket Services Kernel Extension Host Crash.");
	}
}
