using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WUApiLib;

[ComImport]
[Guid("D9A59339-E245-4DBD-9686-4D5763E39624")]
[TypeLibType(4288)]
internal interface IInstallationBehavior
{
	[DispId(1610743809)]
	bool CanRequestUserInput
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743809)]
		get;
	}

	[ComAliasName("WUApiLib.InstallationImpact")]
	[DispId(1610743810)]
	InstallationImpact Impact
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743810)]
		[return: ComAliasName("WUApiLib.InstallationImpact")]
		get;
	}

	[ComAliasName("WUApiLib.InstallationRebootBehavior")]
	[DispId(1610743811)]
	InstallationRebootBehavior RebootBehavior
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743811)]
		[return: ComAliasName("WUApiLib.InstallationRebootBehavior")]
		get;
	}

	[DispId(1610743812)]
	bool RequiresNetworkConnectivity
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743812)]
		get;
	}
}
