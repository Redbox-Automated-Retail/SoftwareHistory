using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WUApiLib;

[ComImport]
[ClassInterface(0)]
[Guid("C2E88C2F-6F5B-4AAA-894B-55C847AD3A2D")]
[TypeLibType(2)]
internal class WindowsUpdateAgentInfoClass : IWindowsUpdateAgentInfo, WindowsUpdateAgentInfo
{
	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610743809)]
	[return: MarshalAs(UnmanagedType.Struct)]
	public virtual extern object GetInfo([In][MarshalAs(UnmanagedType.Struct)] object varInfoIdentifier);
}
