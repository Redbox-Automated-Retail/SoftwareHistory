using System.Runtime.InteropServices;

namespace stdole;

[ComVisible(false)]
public delegate void FontEvents_FontChangedEventHandler([In][MarshalAs(UnmanagedType.BStr)] string PropertyName);
