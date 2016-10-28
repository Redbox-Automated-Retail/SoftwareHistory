using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using stdole;

namespace ShockwaveFlashObjects;

[ComImport]
[Guid("A6EF9860-C720-11D0-9337-00A0C90DCAA9")]
[TypeLibType(4096)]
[ComVisible(true)]
public interface IDispatchEx
{
	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void GetDispID([In][MarshalAs(UnmanagedType.BStr)] string bstrName, [In] uint grfdex, out int pid);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void RemoteInvokeEx([In] int id, [In] uint lcid, [In] uint dwFlags, [In] ref stdole.DISPPARAMS pdp, [MarshalAs(UnmanagedType.Struct)] out object pvarRes, out stdole.EXCEPINFO pei, [In][MarshalAs(UnmanagedType.Interface)] IServiceProvider pspCaller, [In] uint cvarRefArg, [In] ref uint rgiRefArg, [In][Out][MarshalAs(UnmanagedType.Struct)] ref object rgvarRefArg);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void DeleteMemberByName([In][MarshalAs(UnmanagedType.BStr)] string bstrName, [In] uint grfdex);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void DeleteMemberByDispID([In] int id);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void GetMemberProperties([In] int id, [In] uint grfdexFetch, out uint pgrfdex);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void GetMemberName([In] int id, [MarshalAs(UnmanagedType.BStr)] out string pbstrName);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void GetNextDispID([In] uint grfdex, [In] int id, out int pid);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void GetNameSpaceParent([MarshalAs(UnmanagedType.IUnknown)] out object ppunk);
}
