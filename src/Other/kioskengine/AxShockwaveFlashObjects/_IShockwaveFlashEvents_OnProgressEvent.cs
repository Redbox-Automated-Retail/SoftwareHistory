using System.Runtime.InteropServices;

namespace AxShockwaveFlashObjects;

[ComVisible(true)]
public class _IShockwaveFlashEvents_OnProgressEvent
{
	public int percentDone;

	public _IShockwaveFlashEvents_OnProgressEvent(int percentDone)
	{
		this.percentDone = percentDone;
	}
}
