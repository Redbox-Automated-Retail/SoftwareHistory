using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WUApiLib;

[ComImport]
[TypeLibType(4288)]
[Guid("6ABC136A-C3CA-4384-8171-CB2B1E59B8DC")]
internal interface IAutomaticUpdatesSettings2 : IAutomaticUpdatesSettings
{
	[DispId(1610743809)]
	[ComAliasName("WUApiLib.AutomaticUpdatesNotificationLevel")]
	new AutomaticUpdatesNotificationLevel NotificationLevel
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743809)]
		[return: ComAliasName("WUApiLib.AutomaticUpdatesNotificationLevel")]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743809)]
		[param: In]
		[param: ComAliasName("WUApiLib.AutomaticUpdatesNotificationLevel")]
		set;
	}

	[DispId(1610743810)]
	new bool ReadOnly
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743810)]
		get;
	}

	[DispId(1610743811)]
	new bool Required
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743811)]
		get;
	}

	[DispId(1610743812)]
	[ComAliasName("WUApiLib.AutomaticUpdatesScheduledInstallationDay")]
	new AutomaticUpdatesScheduledInstallationDay ScheduledInstallationDay
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743812)]
		[return: ComAliasName("WUApiLib.AutomaticUpdatesScheduledInstallationDay")]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743812)]
		[param: In]
		[param: ComAliasName("WUApiLib.AutomaticUpdatesScheduledInstallationDay")]
		set;
	}

	[DispId(1610743813)]
	new int ScheduledInstallationTime
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743813)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743813)]
		[param: In]
		set;
	}

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610743814)]
	new void Refresh();

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610743815)]
	new void Save();

	[DispId(1610809345)]
	bool IncludeRecommendedUpdates
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610809345)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610809345)]
		[param: In]
		set;
	}

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610809346)]
	bool CheckPermission([In][ComAliasName("WUApiLib.AutomaticUpdatesUserType")] AutomaticUpdatesUserType userType, [In][ComAliasName("WUApiLib.AutomaticUpdatesPermissionType")] AutomaticUpdatesPermissionType permissionType);
}
