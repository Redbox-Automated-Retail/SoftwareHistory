using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WUApiLib;

[ComImport]
[DefaultMember("RegistrationState")]
[TypeLibType(4288)]
[Guid("DDE02280-12B3-4E0B-937B-6747F6ACB286")]
internal interface IUpdateServiceRegistration
{
	[DispId(0)]
	[ComAliasName("WUApiLib.UpdateServiceRegistrationState")]
	UpdateServiceRegistrationState RegistrationState
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(0)]
		[return: ComAliasName("WUApiLib.UpdateServiceRegistrationState")]
		get;
	}

	[DispId(1610743809)]
	string ServiceID
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743809)]
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
	}

	[DispId(1610743810)]
	bool IsPendingRegistrationWithAU
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743810)]
		get;
	}

	[DispId(1610743811)]
	IUpdateService2 Service
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743811)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}
}
