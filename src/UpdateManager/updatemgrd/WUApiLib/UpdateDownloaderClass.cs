using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WUApiLib;

[ComImport]
[ClassInterface(0)]
[Guid("5BAF654A-5A07-4264-A255-9FF54C7151E7")]
[TypeLibType(2)]
internal class UpdateDownloaderClass : IUpdateDownloader, UpdateDownloader
{
	[DispId(1610743809)]
	public virtual extern string ClientApplicationID
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743809)]
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743809)]
		[param: In]
		[param: MarshalAs(UnmanagedType.BStr)]
		set;
	}

	[DispId(1610743810)]
	public virtual extern bool IsForced
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743810)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743810)]
		[param: In]
		set;
	}

	[ComAliasName("WUApiLib.DownloadPriority")]
	[DispId(1610743811)]
	public virtual extern DownloadPriority Priority
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743811)]
		[return: ComAliasName("WUApiLib.DownloadPriority")]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743811)]
		[param: In]
		[param: ComAliasName("WUApiLib.DownloadPriority")]
		set;
	}

	[DispId(1610743812)]
	public virtual extern UpdateCollection Updates
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743812)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743812)]
		[param: In]
		[param: MarshalAs(UnmanagedType.Interface)]
		set;
	}

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610743813)]
	[return: MarshalAs(UnmanagedType.Interface)]
	public virtual extern IDownloadJob BeginDownload([In][MarshalAs(UnmanagedType.IUnknown)] object onProgressChanged, [In][MarshalAs(UnmanagedType.IUnknown)] object onCompleted, [In][MarshalAs(UnmanagedType.Struct)] object state);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610743814)]
	[return: MarshalAs(UnmanagedType.Interface)]
	public virtual extern IDownloadResult Download();

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610743815)]
	[return: MarshalAs(UnmanagedType.Interface)]
	public virtual extern IDownloadResult EndDownload([In][MarshalAs(UnmanagedType.Interface)] IDownloadJob value);
}
