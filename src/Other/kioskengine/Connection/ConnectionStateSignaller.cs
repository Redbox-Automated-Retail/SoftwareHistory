using System;
using Belikov.GenuineChannels.DotNetRemotingLayer;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.Connection;

internal class ConnectionStateSignaller
{
	private HostInformation _remote;

	private IGenuineEventProvider _iGenuineEventProvider;

	private GenuineEventType _currentState;

	public ConnectionStateSignaller(HostInformation remote, IGenuineEventProvider iGenuineEventProvider)
	{
		_remote = remote;
		_iGenuineEventProvider = iGenuineEventProvider;
		_currentState = GenuineEventType.GeneralConnectionIndeterminate;
	}

	public void SetState(GenuineEventType genuineEventType, Exception reason, object additionalInfo)
	{
		lock (this)
		{
			if (_currentState == genuineEventType)
			{
				return;
			}
			_currentState = genuineEventType;
		}
		_iGenuineEventProvider.Fire(new GenuineEventArgs(genuineEventType, reason, _remote, additionalInfo));
	}
}
