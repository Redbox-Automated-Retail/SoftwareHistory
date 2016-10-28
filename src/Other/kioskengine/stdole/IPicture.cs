using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace stdole;

[ComImport]
[ComConversionLoss]
[TypeLibType(16)]
[Guid("7BF80980-BF32-101A-8BBB-00AA00300CAB")]
[InterfaceType(1)]
[ComVisible(true)]
public interface IPicture
{
	[DispId(1610678272)]
	[ComAliasName("stdole.OLE_HANDLE")]
	int Handle
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: ComAliasName("stdole.OLE_HANDLE")]
		get;
	}

	[DispId(1610678273)]
	[ComAliasName("stdole.OLE_HANDLE")]
	int hPal
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: ComAliasName("stdole.OLE_HANDLE")]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[param: In]
		[param: ComAliasName("stdole.OLE_HANDLE")]
		set;
	}

	[DispId(1610678274)]
	short Type
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		get;
	}

	[DispId(1610678275)]
	[ComAliasName("stdole.OLE_XSIZE_HIMETRIC")]
	int Width
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: ComAliasName("stdole.OLE_XSIZE_HIMETRIC")]
		get;
	}

	[DispId(1610678276)]
	[ComAliasName("stdole.OLE_YSIZE_HIMETRIC")]
	int Height
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: ComAliasName("stdole.OLE_YSIZE_HIMETRIC")]
		get;
	}

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void Render([In] int hdc, [In] int x, [In] int y, [In] int cx, [In] int cy, [In][ComAliasName("stdole.OLE_XPOS_HIMETRIC")] int xSrc, [In][ComAliasName("stdole.OLE_YPOS_HIMETRIC")] int ySrc, [In][ComAliasName("stdole.OLE_XSIZE_HIMETRIC")] int cxSrc, [In][ComAliasName("stdole.OLE_YSIZE_HIMETRIC")] int cySrc, [In] IntPtr prcWBounds);

	[DispId(1610678279)]
	int CurDC
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		get;
	}

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void SelectPicture([In] int hdcIn, out int phdcOut, [ComAliasName("stdole.OLE_HANDLE")] out int phbmpOut);

	[DispId(1610678281)]
	bool KeepOriginalFormat
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[param: In]
		set;
	}

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void PictureChanged();

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void SaveAsFile([In] IntPtr pstm, [In] bool fSaveMemCopy, out int pcbSize);

	[DispId(1610678285)]
	int Attributes
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		get;
	}

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void SetHdc([In][ComAliasName("stdole.OLE_HANDLE")] int hdc);
}
