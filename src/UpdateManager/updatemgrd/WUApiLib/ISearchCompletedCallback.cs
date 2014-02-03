using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WUApiLib;

[ComImport]
[TypeLibType(384)]
[InterfaceType(1)]
[Guid("88AEE058-D4B0-4725-A2F1-814A67AE964C")]
internal interface ISearchCompletedCallback
{
	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void Invoke([In][MarshalAs(UnmanagedType.Interface)] ISearchJob searchJob, [In][MarshalAs(UnmanagedType.Interface)] ISearchCompletedCallbackArgs callbackArgs);
}
