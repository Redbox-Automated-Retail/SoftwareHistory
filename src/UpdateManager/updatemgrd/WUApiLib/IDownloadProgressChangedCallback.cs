using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WUApiLib;

[ComImport]
[InterfaceType(1)]
[Guid("8C3F1CDD-6173-4591-AEBD-A56A53CA77C1")]
[TypeLibType(384)]
internal interface IDownloadProgressChangedCallback
{
	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void Invoke([In][MarshalAs(UnmanagedType.Interface)] IDownloadJob downloadJob, [In][MarshalAs(UnmanagedType.Interface)] IDownloadProgressChangedCallbackArgs callbackArgs);
}
