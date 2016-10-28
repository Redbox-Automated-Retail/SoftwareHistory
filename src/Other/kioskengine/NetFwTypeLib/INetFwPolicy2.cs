using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NetFwTypeLib;

[ComImport]
[TypeLibType(4160)]
[Guid("98325047-C671-4174-8D81-DEFCD3F03186")]
[ComVisible(true)]
public interface INetFwPolicy2
{
	[DispId(1)]
	int CurrentProfileTypes
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1)]
		get;
	}

	// C# has no syntax for parameterized property 'FirewallEnabled'.
	// Its 'property:' attributes below are ignored by the compiler (CS0657).
	[property: DispId(2)]
	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(2)]
	bool get_FirewallEnabled([In] NET_FW_PROFILE_TYPE2_ profileType);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(2)]
	void set_FirewallEnabled([In] NET_FW_PROFILE_TYPE2_ profileType, [In] bool Enabled);

	// C# has no syntax for parameterized property 'ExcludedInterfaces'.
	// Its 'property:' attributes below are ignored by the compiler (CS0657).
	[property: DispId(3)]
	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(3)]
	[return: MarshalAs(UnmanagedType.Struct)]
	object get_ExcludedInterfaces([In] NET_FW_PROFILE_TYPE2_ profileType);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(3)]
	void set_ExcludedInterfaces([In] NET_FW_PROFILE_TYPE2_ profileType, [In][MarshalAs(UnmanagedType.Struct)] object Interfaces);

	// C# has no syntax for parameterized property 'BlockAllInboundTraffic'.
	// Its 'property:' attributes below are ignored by the compiler (CS0657).
	[property: DispId(4)]
	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(4)]
	bool get_BlockAllInboundTraffic([In] NET_FW_PROFILE_TYPE2_ profileType);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(4)]
	void set_BlockAllInboundTraffic([In] NET_FW_PROFILE_TYPE2_ profileType, [In] bool Block);

	// C# has no syntax for parameterized property 'NotificationsDisabled'.
	// Its 'property:' attributes below are ignored by the compiler (CS0657).
	[property: DispId(5)]
	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(5)]
	bool get_NotificationsDisabled([In] NET_FW_PROFILE_TYPE2_ profileType);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(5)]
	void set_NotificationsDisabled([In] NET_FW_PROFILE_TYPE2_ profileType, [In] bool disabled);

	// C# has no syntax for parameterized property 'UnicastResponsesToMulticastBroadcastDisabled'.
	// Its 'property:' attributes below are ignored by the compiler (CS0657).
	[property: DispId(6)]
	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(6)]
	bool get_UnicastResponsesToMulticastBroadcastDisabled([In] NET_FW_PROFILE_TYPE2_ profileType);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(6)]
	void set_UnicastResponsesToMulticastBroadcastDisabled([In] NET_FW_PROFILE_TYPE2_ profileType, [In] bool disabled);

	[DispId(7)]
	INetFwRules Rules
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(7)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}

	[DispId(8)]
	INetFwServiceRestriction ServiceRestriction
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(8)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(9)]
	void EnableRuleGroup([In] int profileTypesBitmask, [In][MarshalAs(UnmanagedType.BStr)] string group, [In] bool enable);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(10)]
	bool IsRuleGroupEnabled([In] int profileTypesBitmask, [In][MarshalAs(UnmanagedType.BStr)] string group);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(11)]
	void RestoreLocalFirewallDefaults();

	// C# has no syntax for parameterized property 'DefaultInboundAction'.
	// Its 'property:' attributes below are ignored by the compiler (CS0657).
	[property: DispId(12)]
	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(12)]
	NET_FW_ACTION_ get_DefaultInboundAction([In] NET_FW_PROFILE_TYPE2_ profileType);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(12)]
	void set_DefaultInboundAction([In] NET_FW_PROFILE_TYPE2_ profileType, [In] NET_FW_ACTION_ Action);

	// C# has no syntax for parameterized property 'DefaultOutboundAction'.
	// Its 'property:' attributes below are ignored by the compiler (CS0657).
	[property: DispId(13)]
	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(13)]
	NET_FW_ACTION_ get_DefaultOutboundAction([In] NET_FW_PROFILE_TYPE2_ profileType);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(13)]
	void set_DefaultOutboundAction([In] NET_FW_PROFILE_TYPE2_ profileType, [In] NET_FW_ACTION_ Action);

	// C# has no syntax for parameterized property 'IsRuleGroupCurrentlyEnabled'.
	// Its 'property:' attributes below are ignored by the compiler (CS0657).
	[property: DispId(14)]
	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(14)]
	bool get_IsRuleGroupCurrentlyEnabled([In][MarshalAs(UnmanagedType.BStr)] string group);

	[DispId(15)]
	NET_FW_MODIFY_STATE_ LocalPolicyModifyState
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(15)]
		get;
	}
}
