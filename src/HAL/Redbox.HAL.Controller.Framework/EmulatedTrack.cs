using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class EmulatedTrack : ITrack
{
	public TrackState TrackState => TrackState.Closed;

	public IControlResponse Close()
	{
		return CoreResponse.TimedOutResponse();
	}

	public IControlResponse Open()
	{
		return CoreResponse.TimedOutResponse();
	}

	public IControlResponse Cycle()
	{
		return CoreResponse.TimedOutResponse();
	}
}
