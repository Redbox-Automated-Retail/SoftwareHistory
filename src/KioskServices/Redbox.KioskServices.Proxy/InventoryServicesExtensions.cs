using Redbox.Core;
using Redbox.KioskServices.Proxy.ComponentModel;

namespace Redbox.KioskServices.Proxy;

public static class InventoryServicesExtensions
{
	public static void AddInventoryServices(this ServiceLocator locator)
	{
		locator.AddService(typeof(IInventoryServices), new InventoryServicesProxy());
	}
}
