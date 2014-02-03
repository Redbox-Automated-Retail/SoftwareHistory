using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WUApiLib;

[ComImport]
[TypeLibType(4288)]
[Guid("E7A4D634-7942-4DD9-A111-82228BA33901")]
internal interface IAutomaticUpdatesResults
{
	[DispId(1610743809)]
	object LastSearchSuccessDate
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743809)]
		[return: MarshalAs(UnmanagedType.Struct)]
		get;
	}

	[DispId(1610743810)]
	object LastInstallationSuccessDate
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743810)]
		[return: MarshalAs(UnmanagedType.Struct)]
		get;
	}
}
