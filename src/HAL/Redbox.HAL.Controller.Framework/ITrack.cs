using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal interface ITrack
{
	IControlResponse Close();

	IControlResponse Open();
}
