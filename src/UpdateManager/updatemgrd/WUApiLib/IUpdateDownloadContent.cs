using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WUApiLib;

[ComImport]
[Guid("54A2CB2D-9A0C-48B6-8A50-9ABB69EE2D02")]
[TypeLibType(4288)]
internal interface IUpdateDownloadContent
{
	[DispId(1610743809)]
	string DownloadUrl
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743809)]
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
	}
}
