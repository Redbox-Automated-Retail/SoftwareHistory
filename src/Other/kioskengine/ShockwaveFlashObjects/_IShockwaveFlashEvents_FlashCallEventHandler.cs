using System.Runtime.InteropServices;

namespace ShockwaveFlashObjects;

[ComVisible(false)]
[TypeLibType(16)]
public delegate void _IShockwaveFlashEvents_FlashCallEventHandler([In][MarshalAs(UnmanagedType.BStr)] string request);
