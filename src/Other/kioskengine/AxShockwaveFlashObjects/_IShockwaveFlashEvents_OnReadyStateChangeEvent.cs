using System.Runtime.InteropServices;

namespace AxShockwaveFlashObjects;

[ComVisible(true)]
public class _IShockwaveFlashEvents_OnReadyStateChangeEvent
{
	public int newState;

	public _IShockwaveFlashEvents_OnReadyStateChangeEvent(int newState)
	{
		this.newState = newState;
	}
}
