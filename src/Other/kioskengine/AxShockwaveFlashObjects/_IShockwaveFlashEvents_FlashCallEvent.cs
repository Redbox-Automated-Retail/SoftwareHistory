using System.Runtime.InteropServices;

namespace AxShockwaveFlashObjects;

[ComVisible(true)]
public class _IShockwaveFlashEvents_FlashCallEvent
{
	public string request;

	public _IShockwaveFlashEvents_FlashCallEvent(string request)
	{
		this.request = request;
	}
}
