using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WUApiLib;

[ComImport]
[ClassInterface(0)]
[Guid("B699E5E8-67FF-4177-88B0-3684A3388BFB")]
[TypeLibType(2)]
internal class UpdateSearcherClass : IUpdateSearcher3, UpdateSearcher
{
	[DispId(1610743809)]
	public virtual extern bool CanAutomaticallyUpgradeService
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743809)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743809)]
		[param: In]
		set;
	}

	[DispId(1610743811)]
	public virtual extern string ClientApplicationID
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743811)]
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743811)]
		[param: In]
		[param: MarshalAs(UnmanagedType.BStr)]
		set;
	}

	[DispId(1610743812)]
	public virtual extern bool IncludePotentiallySupersededUpdates
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743812)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743812)]
		[param: In]
		set;
	}

	[ComAliasName("WUApiLib.ServerSelection")]
	[DispId(1610743815)]
	public virtual extern ServerSelection ServerSelection
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743815)]
		[return: ComAliasName("WUApiLib.ServerSelection")]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743815)]
		[param: In]
		[param: ComAliasName("WUApiLib.ServerSelection")]
		set;
	}

	[DispId(1610743821)]
	public virtual extern bool Online
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743821)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743821)]
		[param: In]
		set;
	}

	[DispId(1610743823)]
	public virtual extern string ServiceID
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743823)]
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743823)]
		[param: In]
		[param: MarshalAs(UnmanagedType.BStr)]
		set;
	}

	[DispId(1610809345)]
	public virtual extern bool IgnoreDownloadPriority
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610809345)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610809345)]
		[param: In]
		set;
	}

	[DispId(1610874881)]
	[ComAliasName("WUApiLib.SearchScope")]
	public virtual extern SearchScope SearchScope
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610874881)]
		[return: ComAliasName("WUApiLib.SearchScope")]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610874881)]
		[param: In]
		[param: ComAliasName("WUApiLib.SearchScope")]
		set;
	}

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610743816)]
	[return: MarshalAs(UnmanagedType.Interface)]
	public virtual extern ISearchJob BeginSearch([In][MarshalAs(UnmanagedType.BStr)] string criteria, [In][MarshalAs(UnmanagedType.IUnknown)] object onCompleted, [In][MarshalAs(UnmanagedType.Struct)] object state);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610743817)]
	[return: MarshalAs(UnmanagedType.Interface)]
	public virtual extern ISearchResult EndSearch([In][MarshalAs(UnmanagedType.Interface)] ISearchJob searchJob);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610743818)]
	[return: MarshalAs(UnmanagedType.BStr)]
	public virtual extern string EscapeString([In][MarshalAs(UnmanagedType.BStr)] string unescaped);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610743819)]
	[return: MarshalAs(UnmanagedType.Interface)]
	public virtual extern IUpdateHistoryEntryCollection QueryHistory([In] int startIndex, [In] int Count);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610743820)]
	[return: MarshalAs(UnmanagedType.Interface)]
	public virtual extern ISearchResult Search([In][MarshalAs(UnmanagedType.BStr)] string criteria);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610743822)]
	public virtual extern int GetTotalHistoryCount();
}
