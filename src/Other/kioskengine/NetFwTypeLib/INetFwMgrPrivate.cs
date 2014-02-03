using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NetFwTypeLib;

[ComImport]
[Guid("4FBE7FE9-4AD1-4D70-BB77-66963016FD09")]
[TypeLibType(4160)]
public interface INetFwMgrPrivate : INetFwMgr
{
	[DispId(1)]
	new INetFwPolicy LocalPolicy
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}

	[DispId(2)]
	new NET_FW_PROFILE_TYPE_ CurrentProfileType
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(2)]
		get;
	}

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(3)]
	new void RestoreDefaults();

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(4)]
	new void IsPortAllowed([In][MarshalAs(UnmanagedType.BStr)] string imageFileName, [In] NET_FW_IP_VERSION_ IpVersion, [In] int portNumber, [In][MarshalAs(UnmanagedType.BStr)] string localAddress, [In] NET_FW_IP_PROTOCOL_ ipProtocol, [MarshalAs(UnmanagedType.Struct)] out object allowed, [MarshalAs(UnmanagedType.Struct)] out object restricted);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(5)]
	new void IsIcmpTypeAllowed([In] NET_FW_IP_VERSION_ IpVersion, [In][MarshalAs(UnmanagedType.BStr)] string localAddress, [In] byte Type, [MarshalAs(UnmanagedType.Struct)] out object allowed, [MarshalAs(UnmanagedType.Struct)] out object restricted);
}
