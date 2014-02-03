using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WUApiLib;

[ComImport]
[TypeLibType(4288)]
[Guid("2EE48F22-AF3C-405F-8970-F71BE12EE9A2")]
internal interface IAutomaticUpdatesSettings
{
	[DispId(1610743809)]
	[ComAliasName("WUApiLib.AutomaticUpdatesNotificationLevel")]
	AutomaticUpdatesNotificationLevel NotificationLevel
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
	bool ReadOnly
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743810)]
		get;
	}

	[DispId(1610743811)]
	bool Required
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743811)]
		get;
	}

	[DispId(1610743812)]
	[ComAliasName("WUApiLib.AutomaticUpdatesScheduledInstallationDay")]
	AutomaticUpdatesScheduledInstallationDay ScheduledInstallationDay
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
	int ScheduledInstallationTime
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
	void Refresh();

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610743815)]
	void Save();
}
