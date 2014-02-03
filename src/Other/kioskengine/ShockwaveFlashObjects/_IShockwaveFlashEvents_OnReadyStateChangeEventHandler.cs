using System.Runtime.InteropServices;

namespace ShockwaveFlashObjects;

[ComVisible(false)]
[TypeLibType(16)]
public delegate void _IShockwaveFlashEvents_OnReadyStateChangeEventHandler(int newState);
