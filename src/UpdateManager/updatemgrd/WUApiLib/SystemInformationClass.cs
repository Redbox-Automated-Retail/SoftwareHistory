using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WUApiLib;

[ComImport]
[TypeLibType(2)]
[ClassInterface(0)]
[Guid("C01B9BA0-BEA7-41BA-B604-D0A36F469133")]
internal class SystemInformationClass : ISystemInformation, SystemInformation
{
	[DispId(1610743809)]
	public virtual extern string OemHardwareSupportLink
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743809)]
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
	}

	[DispId(1610743810)]
	public virtual extern bool RebootRequired
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743810)]
		get;
	}
}
