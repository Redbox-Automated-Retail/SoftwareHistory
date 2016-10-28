using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ShockwaveFlashObjects;

[ComImport]
[Guid("6D5140C1-7436-11CE-8034-00AA006009FA")]
[InterfaceType(1)]
[ComVisible(true)]
public interface IServiceProvider
{
	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void RemoteQueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppvObject);
}
