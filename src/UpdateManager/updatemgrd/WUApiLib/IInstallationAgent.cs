using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WUApiLib;

[ComImport]
[TypeLibType(4288)]
[Guid("925CBC18-A2EA-4648-BF1C-EC8BADCFE20A")]
internal interface IInstallationAgent
{
	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610743809)]
	void RecordInstallationResult([In][MarshalAs(UnmanagedType.BStr)] string installationResultCookie, [In] int HResult, [In][MarshalAs(UnmanagedType.Interface)] StringCollection extendedReportingData);
}
