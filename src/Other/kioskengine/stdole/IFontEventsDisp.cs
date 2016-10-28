using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace stdole;

[ComImport]
[InterfaceType(2)]
[Guid("4EF6100A-AF88-11D0-9846-00C04FC29993")]
[ComVisible(true)]
public interface IFontEventsDisp
{
	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(9)]
	void FontChanged([In][MarshalAs(UnmanagedType.BStr)] string PropertyName);
}
