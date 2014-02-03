using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WUApiLib;

[ComImport]
[Guid("F8D253D9-89A4-4DAA-87B6-1168369F0B21")]
[TypeLibType(2)]
[ClassInterface(0)]
internal class UpdateServiceManagerClass : IUpdateServiceManager2, UpdateServiceManager
{
	[DispId(1610743809)]
	public virtual extern IUpdateServiceCollection Services
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743809)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}

	[DispId(1610809345)]
	public virtual extern string ClientApplicationID
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610809345)]
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610809345)]
		[param: In]
		[param: MarshalAs(UnmanagedType.BStr)]
		set;
	}

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610743810)]
	[return: MarshalAs(UnmanagedType.Interface)]
	public virtual extern IUpdateService AddService([In][MarshalAs(UnmanagedType.BStr)] string ServiceID, [In][MarshalAs(UnmanagedType.BStr)] string authorizationCabPath);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610743811)]
	public virtual extern void RegisterServiceWithAU([In][MarshalAs(UnmanagedType.BStr)] string ServiceID);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610743812)]
	public virtual extern void RemoveService([In][MarshalAs(UnmanagedType.BStr)] string ServiceID);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610743813)]
	public virtual extern void UnregisterServiceWithAU([In][MarshalAs(UnmanagedType.BStr)] string ServiceID);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610743814)]
	[return: MarshalAs(UnmanagedType.Interface)]
	public virtual extern IUpdateService AddScanPackageService([In][MarshalAs(UnmanagedType.BStr)] string serviceName, [In][MarshalAs(UnmanagedType.BStr)] string scanFileLocation, [In] int flags = 0);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610678279)]
	public virtual extern void SetOption([In][MarshalAs(UnmanagedType.BStr)] string optionName, [In][MarshalAs(UnmanagedType.Struct)] object optionValue);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610809346)]
	[return: MarshalAs(UnmanagedType.Interface)]
	public virtual extern IUpdateServiceRegistration QueryServiceRegistration([In][MarshalAs(UnmanagedType.BStr)] string ServiceID);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610809347)]
	[return: MarshalAs(UnmanagedType.Interface)]
	public virtual extern IUpdateServiceRegistration AddService2([In][MarshalAs(UnmanagedType.BStr)] string ServiceID, [In] int flags, [In][MarshalAs(UnmanagedType.BStr)] string authorizationCabPath);
}
