using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal interface ITrack
{
	TrackState TrackState { get; }

	IControlResponse Close();

	IControlResponse Open();

	IControlResponse Cycle();
}
