using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace stdole;

[ComImport]
[Guid("4EF6100A-AF88-11D0-9846-00C04FC29993")]
[TypeLibType(4112)]
[InterfaceType(2)]
[ComVisible(true)]
public interface FontEvents
{
	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(9)]
	void FontChanged([In][MarshalAs(UnmanagedType.BStr)] string PropertyName);
}
