using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WUApiLib;

[ComImport]
[Guid("E01402D5-F8DA-43BA-A012-38894BD048F1")]
[InterfaceType(1)]
[TypeLibType(384)]
internal interface IInstallationProgressChangedCallback
{
	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void Invoke([In][MarshalAs(UnmanagedType.Interface)] IInstallationJob installationJob, [In][MarshalAs(UnmanagedType.Interface)] IInstallationProgressChangedCallbackArgs callbackArgs);
}
