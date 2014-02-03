using System;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.DotNetRemotingLayer;

[Serializable]
internal class GenuineEventArgs : EventArgs
{
	public readonly GenuineEventType EventType;

	public readonly Exception SourceException;

	public readonly HostInformation HostInformation;

	public readonly object AdditionalInfo;

	public GenuineEventArgs(GenuineEventType genuineEventType, Exception sourceException, HostInformation hostInformation, object additionalInfo)
	{
		EventType = genuineEventType;
		SourceException = sourceException;
		HostInformation = hostInformation;
		AdditionalInfo = additionalInfo;
	}
}
