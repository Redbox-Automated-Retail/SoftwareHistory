using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WUApiLib;

[ComImport]
[DefaultMember("Message")]
[TypeLibType(4288)]
[Guid("A376DD5E-09D4-427F-AF7C-FED5B6E1C1D6")]
internal interface IUpdateException
{
	[DispId(0)]
	string Message
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(0)]
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
	}

	[DispId(1610743809)]
	int HResult
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743809)]
		get;
	}

	[DispId(1610743810)]
	[ComAliasName("WUApiLib.UpdateExceptionContext")]
	UpdateExceptionContext Context
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743810)]
		[return: ComAliasName("WUApiLib.UpdateExceptionContext")]
		get;
	}
}
