using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WUApiLib;

[ComImport]
[Guid("85713FA1-7796-4FA2-BE3B-E2D6124DD373")]
[TypeLibType(4304)]
internal interface IWindowsUpdateAgentInfo
{
	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610743809)]
	[return: MarshalAs(UnmanagedType.Struct)]
	object GetInfo([In][MarshalAs(UnmanagedType.Struct)] object varInfoIdentifier);
}
