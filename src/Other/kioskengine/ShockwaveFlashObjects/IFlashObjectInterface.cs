using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using stdole;

namespace ShockwaveFlashObjects;

[ComImport]
[TypeLibType(4096)]
[Guid("D27CDB72-AE6D-11CF-96B8-444553540000")]
public interface IFlashObjectInterface : IDispatchEx
{
	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	new void GetDispID([In][MarshalAs(UnmanagedType.BStr)] string bstrName, [In] uint grfdex, out int pid);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	new void RemoteInvokeEx([In] int id, [In] uint lcid, [In] uint dwFlags, [In] ref stdole.DISPPARAMS pdp, [MarshalAs(UnmanagedType.Struct)] out object pvarRes, out stdole.EXCEPINFO pei, [In][MarshalAs(UnmanagedType.Interface)] IServiceProvider pspCaller, [In] uint cvarRefArg, [In] ref uint rgiRefArg, [In][Out][MarshalAs(UnmanagedType.Struct)] ref object rgvarRefArg);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	new void DeleteMemberByName([In][MarshalAs(UnmanagedType.BStr)] string bstrName, [In] uint grfdex);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	new void DeleteMemberByDispID([In] int id);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	new void GetMemberProperties([In] int id, [In] uint grfdexFetch, out uint pgrfdex);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	new void GetMemberName([In] int id, [MarshalAs(UnmanagedType.BStr)] out string pbstrName);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	new void GetNextDispID([In] uint grfdex, [In] int id, out int pid);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	new void GetNameSpaceParent([MarshalAs(UnmanagedType.IUnknown)] out object ppunk);
}
