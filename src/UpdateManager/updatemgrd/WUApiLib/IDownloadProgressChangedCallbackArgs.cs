using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WUApiLib;

[ComImport]
[TypeLibType(4288)]
[Guid("324FF2C6-4981-4B04-9412-57481745AB24")]
internal interface IDownloadProgressChangedCallbackArgs
{
	[DispId(1610743809)]
	IDownloadProgress Progress
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743809)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}
}
