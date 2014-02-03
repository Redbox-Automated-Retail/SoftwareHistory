using System.Runtime.InteropServices;

namespace ShockwaveFlashObjects;

[ComVisible(false)]
[TypeLibType(16)]
[ComEventInterface(typeof(_IShockwaveFlashEvents), typeof(_IShockwaveFlashEvents_EventProvider))]
public interface _IShockwaveFlashEvents_Event
{
	event _IShockwaveFlashEvents_OnReadyStateChangeEventHandler OnReadyStateChange;

	event _IShockwaveFlashEvents_OnProgressEventHandler OnProgress;

	event _IShockwaveFlashEvents_FSCommandEventHandler FSCommand;

	event _IShockwaveFlashEvents_FlashCallEventHandler FlashCall;
}
