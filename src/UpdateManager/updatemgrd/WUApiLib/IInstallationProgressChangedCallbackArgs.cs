using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WUApiLib;

[ComImport]
[TypeLibType(4288)]
[Guid("E4F14E1E-689D-4218-A0B9-BC189C484A01")]
internal interface IInstallationProgressChangedCallbackArgs
{
	[DispId(1610743809)]
	IInstallationProgress Progress
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743809)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}
}
