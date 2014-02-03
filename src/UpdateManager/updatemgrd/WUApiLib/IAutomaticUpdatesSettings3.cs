using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WUApiLib;

[ComImport]
[TypeLibType(4288)]
[Guid("B587F5C3-F57E-485F-BBF5-0D181C5CD0DC")]
internal interface IAutomaticUpdatesSettings3 : IAutomaticUpdatesSettings2
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
	new bool IncludeRecommendedUpdates
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
	new bool CheckPermission([In][ComAliasName("WUApiLib.AutomaticUpdatesUserType")] AutomaticUpdatesUserType userType, [In][ComAliasName("WUApiLib.AutomaticUpdatesPermissionType")] AutomaticUpdatesPermissionType permissionType);

	[DispId(1610874881)]
	bool NonAdministratorsElevated
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610874881)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610874881)]
		[param: In]
		set;
	}

	[DispId(1610874882)]
	bool FeaturedUpdatesEnabled
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610874882)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610874882)]
		[param: In]
		set;
	}
}
