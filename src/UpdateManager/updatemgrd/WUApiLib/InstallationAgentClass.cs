using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WUApiLib;

[ComImport]
[ClassInterface(0)]
[TypeLibType(2)]
[Guid("317E92FC-1679-46FD-A0B5-F08914DD8623")]
internal class InstallationAgentClass : IInstallationAgent, InstallationAgent
{
	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610743809)]
	public virtual extern void RecordInstallationResult([In][MarshalAs(UnmanagedType.BStr)] string installationResultCookie, [In] int HResult, [In][MarshalAs(UnmanagedType.Interface)] StringCollection extendedReportingData);
}
