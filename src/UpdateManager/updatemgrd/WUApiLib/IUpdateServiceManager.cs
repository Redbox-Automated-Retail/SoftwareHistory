using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WUApiLib;

[ComImport]
[Guid("23857E3C-02BA-44A3-9423-B1C900805F37")]
[TypeLibType(4288)]
internal interface IUpdateServiceManager
{
	[DispId(1610743809)]
	IUpdateServiceCollection Services
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743809)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610743810)]
	[return: MarshalAs(UnmanagedType.Interface)]
	IUpdateService AddService([In][MarshalAs(UnmanagedType.BStr)] string ServiceID, [In][MarshalAs(UnmanagedType.BStr)] string authorizationCabPath);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610743811)]
	void RegisterServiceWithAU([In][MarshalAs(UnmanagedType.BStr)] string ServiceID);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610743812)]
	void RemoveService([In][MarshalAs(UnmanagedType.BStr)] string ServiceID);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610743813)]
	void UnregisterServiceWithAU([In][MarshalAs(UnmanagedType.BStr)] string ServiceID);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610743814)]
	[return: MarshalAs(UnmanagedType.Interface)]
	IUpdateService AddScanPackageService([In][MarshalAs(UnmanagedType.BStr)] string serviceName, [In][MarshalAs(UnmanagedType.BStr)] string scanFileLocation, [In] int flags = 0);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610678279)]
	void SetOption([In][MarshalAs(UnmanagedType.BStr)] string optionName, [In][MarshalAs(UnmanagedType.Struct)] object optionValue);
}
