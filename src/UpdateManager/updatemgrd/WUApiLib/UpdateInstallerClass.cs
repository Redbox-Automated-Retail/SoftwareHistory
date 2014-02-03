using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WUApiLib;

[ComImport]
[ClassInterface(0)]
[Guid("D2E0FE7F-D23E-48E1-93C0-6FA8CC346474")]
[TypeLibType(2)]
internal class UpdateInstallerClass : IUpdateInstaller2, UpdateInstaller
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

	[DispId(1610743811)]
	[ComAliasName("WUApiLib.wireHWND")]
	public virtual extern IntPtr ParentHwnd
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[TypeLibFunc(1)]
		[DispId(1610743811)]
		[return: ComAliasName("WUApiLib.wireHWND")]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743811)]
		[TypeLibFunc(1)]
		[param: In]
		[param: ComAliasName("WUApiLib.wireHWND")]
		set;
	}

	[DispId(1610743812)]
	public virtual extern object parentWindow
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743812)]
		[return: MarshalAs(UnmanagedType.IUnknown)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743812)]
		[param: In]
		[param: MarshalAs(UnmanagedType.IUnknown)]
		set;
	}

	[DispId(1610743813)]
	public virtual extern UpdateCollection Updates
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743813)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743813)]
		[param: In]
		[param: MarshalAs(UnmanagedType.Interface)]
		set;
	}

	[DispId(1610743820)]
	public virtual extern bool IsBusy
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743820)]
		get;
	}

	[DispId(1610743822)]
	public virtual extern bool AllowSourcePrompts
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743822)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743822)]
		[param: In]
		set;
	}

	[DispId(1610743823)]
	public virtual extern bool RebootRequiredBeforeInstallation
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610743823)]
		get;
	}

	[DispId(1610809345)]
	public virtual extern bool ForceQuiet
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
	[DispId(1610743814)]
	[return: MarshalAs(UnmanagedType.Interface)]
	public virtual extern IInstallationJob BeginInstall([In][MarshalAs(UnmanagedType.IUnknown)] object onProgressChanged, [In][MarshalAs(UnmanagedType.IUnknown)] object onCompleted, [In][MarshalAs(UnmanagedType.Struct)] object state);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610743815)]
	[return: MarshalAs(UnmanagedType.Interface)]
	public virtual extern IInstallationJob BeginUninstall([In][MarshalAs(UnmanagedType.IUnknown)] object onProgressChanged, [In][MarshalAs(UnmanagedType.IUnknown)] object onCompleted, [In][MarshalAs(UnmanagedType.Struct)] object state);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610743816)]
	[return: MarshalAs(UnmanagedType.Interface)]
	public virtual extern IInstallationResult EndInstall([In][MarshalAs(UnmanagedType.Interface)] IInstallationJob value);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610743817)]
	[return: MarshalAs(UnmanagedType.Interface)]
	public virtual extern IInstallationResult EndUninstall([In][MarshalAs(UnmanagedType.Interface)] IInstallationJob value);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610743818)]
	[return: MarshalAs(UnmanagedType.Interface)]
	public virtual extern IInstallationResult Install();

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610743819)]
	[return: MarshalAs(UnmanagedType.Interface)]
	public virtual extern IInstallationResult RunWizard([In][MarshalAs(UnmanagedType.BStr)] string dialogTitle = "");

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(1610743821)]
	[return: MarshalAs(UnmanagedType.Interface)]
	public virtual extern IInstallationResult Uninstall();
}
