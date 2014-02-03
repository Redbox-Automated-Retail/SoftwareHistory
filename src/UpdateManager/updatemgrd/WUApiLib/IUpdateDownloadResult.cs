using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WUApiLib;

[ComImport]
[TypeLibType(4288)]
[Guid("BF99AF76-B575-42AD-8AA4-33CBB5477AF1")]
internal interface IUpdateDownloadResult
{
	[DispId(1610743809)]
	int HResult
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743809)]
		get;
	}

	[ComAliasName("WUApiLib.OperationResultCode")]
	[DispId(1610743810)]
	OperationResultCode ResultCode
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743810)]
		[return: ComAliasName("WUApiLib.OperationResultCode")]
		get;
	}
}
