using System.Runtime.InteropServices;

namespace ShockwaveFlashObjects;

[TypeLibType(16)]
[ComVisible(false)]
public delegate void _IShockwaveFlashEvents_FSCommandEventHandler([In][MarshalAs(UnmanagedType.BStr)] string command, [In][MarshalAs(UnmanagedType.BStr)] string args);
