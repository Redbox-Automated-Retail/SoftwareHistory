using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WUApiLib;

[ComImport]
[Guid("C1C2F21A-D2F4-4902-B5C6-8A081C19A890")]
[TypeLibType(4288)]
[DefaultMember("Title")]
internal interface IUpdate5 : IUpdate4
{
	[DispId(0)]
	new string Title
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(0)]
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
	}

	[DispId(1610743809)]
	new bool AutoSelectOnWebSites
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743809)]
		get;
	}

	[DispId(1610743810)]
	new UpdateCollection BundledUpdates
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743810)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}

	[DispId(1610743811)]
	new bool CanRequireSource
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743811)]
		get;
	}

	[DispId(1610743812)]
	new ICategoryCollection Categories
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743812)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}

	[DispId(1610743813)]
	new object Deadline
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743813)]
		[return: MarshalAs(UnmanagedType.Struct)]
		get;
	}

	[DispId(1610743814)]
	new bool DeltaCompressedContentAvailable
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743814)]
		get;
	}

	[DispId(1610743815)]
	new bool DeltaCompressedContentPreferred
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743815)]
		get;
	}

	[DispId(1610743816)]
	new string Description
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743816)]
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
	}

	[DispId(1610743817)]
	new bool EulaAccepted
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743817)]
		get;
	}

	[DispId(1610743818)]
	new string EulaText
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743818)]
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
	}

	[DispId(1610743819)]
	new string HandlerID
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743819)]
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
	}

	[DispId(1610743820)]
	new IUpdateIdentity Identity
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743820)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}

	[DispId(1610743821)]
	new IImageInformation Image
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743821)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}

	[DispId(1610743822)]
	new IInstallationBehavior InstallationBehavior
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743822)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}

	[DispId(1610743823)]
	new bool IsBeta
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743823)]
		get;
	}

	[DispId(1610743824)]
	new bool IsDownloaded
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743824)]
		get;
	}

	[DispId(1610743825)]
	new bool IsHidden
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743825)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743825)]
		[param: In]
		set;
	}

	[DispId(1610743826)]
	new bool IsInstalled
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743826)]
		get;
	}

	[DispId(1610743827)]
	new bool IsMandatory
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743827)]
		get;
	}

	[DispId(1610743828)]
	new bool IsUninstallable
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743828)]
		get;
	}

	[DispId(1610743829)]
	new StringCollection Languages
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743829)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}

	[DispId(1610743830)]
	new DateTime LastDeploymentChangeTime
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743830)]
		get;
	}

	[DispId(1610743831)]
	new decimal MaxDownloadSize
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743831)]
		get;
	}

	[DispId(1610743832)]
	new decimal MinDownloadSize
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743832)]
		get;
	}

	[DispId(1610743833)]
	new StringCollection MoreInfoUrls
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743833)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}

	[DispId(1610743834)]
	new string MsrcSeverity
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743834)]
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
	}

	[DispId(1610743835)]
	new int RecommendedCpuSpeed
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743835)]
		get;
	}

	[DispId(1610743836)]
	new int RecommendedHardDiskSpace
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743836)]
		get;
	}

	[DispId(1610743837)]
	new int RecommendedMemory
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743837)]
		get;
	}

	[DispId(1610743838)]
	new string ReleaseNotes
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743838)]
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
	}

	[DispId(1610743839)]
	new StringCollection SecurityBulletinIDs
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743839)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}

	[DispId(1610743841)]
	new StringCollection SupersededUpdateIDs
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743841)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}

	[DispId(1610743842)]
	new string SupportUrl
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743842)]
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
	}

	[DispId(1610743843)]
	[ComAliasName("WUApiLib.UpdateType")]
	new UpdateType Type
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743843)]
		[return: ComAliasName("WUApiLib.UpdateType")]
		get;
	}

	[DispId(1610743844)]
	new string UninstallationNotes
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743844)]
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
	}

	[DispId(1610743845)]
	new IInstallationBehavior UninstallationBehavior
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743845)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}

	[DispId(1610743846)]
	new StringCollection UninstallationSteps
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743846)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}

	[DispId(1610743848)]
	new StringCollection KBArticleIDs
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743848)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610743847)]
	new void AcceptEula();

	[ComAliasName("WUApiLib.DeploymentAction")]
	[DispId(1610743849)]
	new DeploymentAction DeploymentAction
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743849)]
		[return: ComAliasName("WUApiLib.DeploymentAction")]
		get;
	}

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610743850)]
	new void CopyFromCache([In][MarshalAs(UnmanagedType.BStr)] string path, [In] bool toExtractCabFiles);

	[ComAliasName("WUApiLib.DownloadPriority")]
	[DispId(1610743851)]
	new DownloadPriority DownloadPriority
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743851)]
		[return: ComAliasName("WUApiLib.DownloadPriority")]
		get;
	}

	[DispId(1610743852)]
	new IUpdateDownloadContentCollection DownloadContents
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743852)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}

	[DispId(1610809345)]
	new bool RebootRequired
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610809345)]
		get;
	}

	[DispId(1610809347)]
	new bool IsPresent
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610809347)]
		get;
	}

	[DispId(1610809348)]
	new StringCollection CveIDs
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610809348)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610809346)]
	new void CopyToCache([In][MarshalAs(UnmanagedType.Interface)] StringCollection pFiles);

	[DispId(1610874881)]
	new bool BrowseOnly
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610874881)]
		get;
	}

	[DispId(1610940417)]
	new bool PerUser
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610940417)]
		get;
	}

	[ComAliasName("WUApiLib.AutoSelectionMode")]
	[DispId(1611005953)]
	AutoSelectionMode AutoSelection
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1611005953)]
		[return: ComAliasName("WUApiLib.AutoSelectionMode")]
		get;
	}

	[DispId(1611005954)]
	[ComAliasName("WUApiLib.AutoDownloadMode")]
	AutoDownloadMode AutoDownload
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1611005954)]
		[return: ComAliasName("WUApiLib.AutoDownloadMode")]
		get;
	}
}
