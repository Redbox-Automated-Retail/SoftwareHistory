using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WUApiLib;

[ComImport]
[TypeLibType(4288)]
[DefaultMember("Message")]
[Guid("A37D00F5-7BB0-4953-B414-F9E98326F2E8")]
internal interface IInvalidProductLicenseException : IUpdateException
{
	[DispId(0)]
	new string Message
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(0)]
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
	}

	[DispId(1610743809)]
	new int HResult
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743809)]
		get;
	}

	[ComAliasName("WUApiLib.UpdateExceptionContext")]
	[DispId(1610743810)]
	new UpdateExceptionContext Context
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743810)]
		[return: ComAliasName("WUApiLib.UpdateExceptionContext")]
		get;
	}

	[DispId(1610809345)]
	string Product
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610809345)]
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
	}
}
