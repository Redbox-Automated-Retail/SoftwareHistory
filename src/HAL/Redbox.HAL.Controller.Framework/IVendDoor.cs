using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal interface IVendDoor
{
	IControlResponse Rent();

	IControlResponse Close();

	VendDoorState ReadState();
}
