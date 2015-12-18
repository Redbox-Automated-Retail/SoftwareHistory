using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NetFwTypeLib;

[ComImport]
[TypeLibType(4160)]
[Guid("8267BBE3-F890-491C-B7B6-2DB1EF0E5D2B")]
public interface INetFwServiceRestriction
{
	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1)]
	void RestrictService([In][MarshalAs(UnmanagedType.BStr)] string serviceName, [In][MarshalAs(UnmanagedType.BStr)] string appName, [In] bool RestrictService, [In] bool serviceSidRestricted);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(2)]
	bool ServiceRestricted([In][MarshalAs(UnmanagedType.BStr)] string serviceName, [In][MarshalAs(UnmanagedType.BStr)] string appName);

	[DispId(3)]
	INetFwRules Rules
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(3)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}
}
