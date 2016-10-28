using System;
using System.Runtime.InteropServices;

namespace Belikov.GenuineChannels.BroadcastEngine;

[ComVisible(true)]
public struct DispatcherFilterKeeper(IMulticastFilter iMulticastFilter) : IDisposable
{
	private IMulticastFilter _previousIMulticastFilter = Dispatcher.SetCurrentFilter(iMulticastFilter);

	public void Dispose()
	{
		Dispatcher.SetCurrentFilter(_previousIMulticastFilter);
	}
}
