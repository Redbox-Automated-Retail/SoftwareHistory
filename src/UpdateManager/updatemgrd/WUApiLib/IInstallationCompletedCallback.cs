using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WUApiLib;

[ComImport]
[TypeLibType(384)]
[InterfaceType(1)]
[Guid("45F4F6F3-D602-4F98-9A8A-3EFA152AD2D3")]
internal interface IInstallationCompletedCallback
{
	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void Invoke([In][MarshalAs(UnmanagedType.Interface)] IInstallationJob installationJob, [In][MarshalAs(UnmanagedType.Interface)] IInstallationCompletedCallbackArgs callbackArgs);
}
