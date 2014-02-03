using System;

namespace Belikov.GenuineChannels.BroadcastEngine;

internal struct DispatcherFilterKeeper(IMulticastFilter iMulticastFilter) : IDisposable
{
	private IMulticastFilter _previousIMulticastFilter = Dispatcher.SetCurrentFilter(iMulticastFilter);

	public void Dispose()
	{
		Dispatcher.SetCurrentFilter(_previousIMulticastFilter);
	}
}
