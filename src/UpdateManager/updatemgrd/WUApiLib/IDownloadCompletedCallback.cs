using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WUApiLib;

[ComImport]
[InterfaceType(1)]
[TypeLibType(384)]
[Guid("77254866-9F5B-4C8E-B9E2-C77A8530D64B")]
internal interface IDownloadCompletedCallback
{
	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void Invoke([In][MarshalAs(UnmanagedType.Interface)] IDownloadJob downloadJob, [In][MarshalAs(UnmanagedType.Interface)] IDownloadCompletedCallbackArgs callbackArgs);
}
