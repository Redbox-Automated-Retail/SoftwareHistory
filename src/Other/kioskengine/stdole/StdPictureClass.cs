using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace stdole;

[ComImport]
[TypeLibType(2)]
[ComConversionLoss]
[Guid("0BE35204-8F91-11CE-9DE3-00AA004BB851")]
[DefaultMember("Handle")]
[ClassInterface(0)]
[ComVisible(true)]
public class StdPictureClass : Picture, StdPicture, IPicture
{
	[ComAliasName("stdole.OLE_HANDLE")]
	[DispId(0)]
	public virtual extern int Handle
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(0)]
		get;
	}

	[DispId(2)]
	[ComAliasName("stdole.OLE_HANDLE")]
	public virtual extern int hPal
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(2)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(2)]
		set;
	}

	[DispId(3)]
	public virtual extern short Type
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(3)]
		get;
	}

	[DispId(4)]
	[ComAliasName("stdole.OLE_XSIZE_HIMETRIC")]
	public virtual extern int Width
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(4)]
		get;
	}

	[DispId(5)]
	[ComAliasName("stdole.OLE_YSIZE_HIMETRIC")]
	public virtual extern int Height
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(5)]
		get;
	}

	[ComAliasName("stdole.OLE_HANDLE")]
	public virtual extern int IPicture_Handle
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: ComAliasName("stdole.OLE_HANDLE")]
		get;
	}

	[ComAliasName("stdole.OLE_HANDLE")]
	public virtual extern int IPicture_hPal
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: ComAliasName("stdole.OLE_HANDLE")]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[param: In]
		[param: ComAliasName("stdole.OLE_HANDLE")]
		set;
	}

	public virtual extern short IPicture_Type
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		get;
	}

	[ComAliasName("stdole.OLE_XSIZE_HIMETRIC")]
	public virtual extern int IPicture_Width
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: ComAliasName("stdole.OLE_XSIZE_HIMETRIC")]
		get;
	}

	[ComAliasName("stdole.OLE_YSIZE_HIMETRIC")]
	public virtual extern int IPicture_Height
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: ComAliasName("stdole.OLE_YSIZE_HIMETRIC")]
		get;
	}

	public virtual extern int CurDC
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		get;
	}

	public virtual extern bool KeepOriginalFormat
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[param: In]
		set;
	}

	public virtual extern int Attributes
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		get;
	}

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(6)]
	public virtual extern void Render(int hdc, int x, int y, int cx, int cy, [ComAliasName("stdole.OLE_XPOS_HIMETRIC")] int xSrc, [ComAliasName("stdole.OLE_YPOS_HIMETRIC")] int ySrc, [ComAliasName("stdole.OLE_XSIZE_HIMETRIC")] int cxSrc, [ComAliasName("stdole.OLE_YSIZE_HIMETRIC")] int cySrc, IntPtr prcWBounds);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	public virtual extern void IPicture_Render([In] int hdc, [In] int x, [In] int y, [In] int cx, [In] int cy, [In][ComAliasName("stdole.OLE_XPOS_HIMETRIC")] int xSrc, [In][ComAliasName("stdole.OLE_YPOS_HIMETRIC")] int ySrc, [In][ComAliasName("stdole.OLE_XSIZE_HIMETRIC")] int cxSrc, [In][ComAliasName("stdole.OLE_YSIZE_HIMETRIC")] int cySrc, [In] IntPtr prcWBounds);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	public virtual extern void SelectPicture([In] int hdcIn, out int phdcOut, [ComAliasName("stdole.OLE_HANDLE")] out int phbmpOut);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	public virtual extern void PictureChanged();

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	public virtual extern void SaveAsFile([In] IntPtr pstm, [In] bool fSaveMemCopy, out int pcbSize);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	public virtual extern void SetHdc([In][ComAliasName("stdole.OLE_HANDLE")] int hdc);
}
