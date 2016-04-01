using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class EmulatedVendDoor : IVendDoor
{
	public IControlResponse Rent()
	{
		return new CoreResponse(AddressSelector.H002);
	}

	public IControlResponse Close()
	{
		return new CoreResponse(AddressSelector.H002);
	}

	public VendDoorState ReadState()
	{
		return VendDoorState.Closed;
	}
}
