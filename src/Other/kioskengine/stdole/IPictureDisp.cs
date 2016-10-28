using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace stdole;

[ComImport]
[DefaultMember("Handle")]
[InterfaceType(2)]
[Guid("7BF80981-BF32-101A-8BBB-00AA00300CAB")]
[ComConversionLoss]
[ComVisible(true)]
public interface IPictureDisp
{
	[DispId(0)]
	[ComAliasName("stdole.OLE_HANDLE")]
	int Handle
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(0)]
		get;
	}

	[DispId(2)]
	[ComAliasName("stdole.OLE_HANDLE")]
	int hPal
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(2)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(2)]
		set;
	}

	[DispId(3)]
	short Type
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(3)]
		get;
	}

	[DispId(4)]
	[ComAliasName("stdole.OLE_XSIZE_HIMETRIC")]
	int Width
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(4)]
		get;
	}

	[DispId(5)]
	[ComAliasName("stdole.OLE_YSIZE_HIMETRIC")]
	int Height
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(5)]
		get;
	}

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(6)]
	void Render(int hdc, int x, int y, int cx, int cy, [ComAliasName("stdole.OLE_XPOS_HIMETRIC")] int xSrc, [ComAliasName("stdole.OLE_YPOS_HIMETRIC")] int ySrc, [ComAliasName("stdole.OLE_XSIZE_HIMETRIC")] int cxSrc, [ComAliasName("stdole.OLE_YSIZE_HIMETRIC")] int cySrc, IntPtr prcWBounds);
}
