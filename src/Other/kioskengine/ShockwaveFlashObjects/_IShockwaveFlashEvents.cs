using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ShockwaveFlashObjects;

[ComImport]
[Guid("D27CDB6D-AE6D-11CF-96B8-444553540000")]
[TypeLibType(4112)]
[InterfaceType(2)]
public interface _IShockwaveFlashEvents
{
	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(-609)]
	void OnReadyStateChange(int newState);

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1958)]
	void OnProgress(int percentDone);

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(150)]
	void FSCommand([In][MarshalAs(UnmanagedType.BStr)] string command, [In][MarshalAs(UnmanagedType.BStr)] string args);

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(197)]
	void FlashCall([In][MarshalAs(UnmanagedType.BStr)] string request);
}
