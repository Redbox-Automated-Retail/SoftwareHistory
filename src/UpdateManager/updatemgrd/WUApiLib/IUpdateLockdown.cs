using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WUApiLib;

[ComImport]
[TypeLibType(384)]
[InterfaceType(1)]
[Guid("A976C28D-75A1-42AA-94AE-8AF8B872089A")]
internal interface IUpdateLockdown
{
	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void LockDown([In] int flags);
}
