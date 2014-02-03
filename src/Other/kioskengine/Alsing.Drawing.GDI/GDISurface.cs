using System;
using System.Drawing;
using System.Windows.Forms;
using Alsing.Windows;

namespace Alsing.Drawing.GDI;

public class GDISurface : GDIObject
{
	private WeakReference _Control;

	protected IntPtr _OldBmp = IntPtr.Zero;

	protected IntPtr _OldBrush = IntPtr.Zero;

	protected IntPtr _OldFont = IntPtr.Zero;

	protected IntPtr _OldPen = IntPtr.Zero;

	protected IntPtr mhBMP;

	protected IntPtr mhDC;

	protected int mHeight;

	protected int mTabSize = 4;

	protected int mWidth;

	private Control Control
	{
		get
		{
			if (_Control != null)
			{
				return (Control)_Control.Target;
			}
			return null;
		}
		set
		{
			_Control = new WeakReference(value);
		}
	}

	public IntPtr hDC => mhDC;

	public IntPtr hBMP => mhBMP;

	public Color TextForeColor
	{
		get
		{
			return NativeMethods.IntToColor(NativeMethods.GetTextColor(mhDC));
		}
		set
		{
			NativeMethods.SetTextColor(mhDC, NativeMethods.ColorToInt(value));
		}
	}

	public Color TextBackColor
	{
		get
		{
			return NativeMethods.IntToColor(NativeMethods.GetBkColor(mhDC));
		}
		set
		{
			NativeMethods.SetBkColor(mhDC, NativeMethods.ColorToInt(value));
		}
	}

	public bool FontTransparent
	{
		get
		{
			return NativeMethods.GetBkMode(mhDC) < 2;
		}
		set
		{
			NativeMethods.SetBkMode(mhDC, value ? 1 : 2);
		}
	}

	public GDIFont Font
	{
		get
		{
			GDITextMetric TextMetric = default(GDITextMetric);
			string text = new string(' ', 48);
			NativeMethods.GetTextMetrics(mhDC, ref TextMetric);
			NativeMethods.GetTextFace(mhDC, 79, text);
			GDIFont gDIFont = new GDIFont();
			gDIFont.FontName = text;
			gDIFont.Bold = TextMetric.tmWeight > 400;
			gDIFont.Italic = TextMetric.tmItalic != 0;
			gDIFont.Underline = TextMetric.tmUnderlined != 0;
			gDIFont.Strikethrough = TextMetric.tmStruckOut != 0;
			gDIFont.Size = (int)((double)TextMetric.tmMemoryHeight / (double)TextMetric.tmDigitizedAspectY * 72.0);
			return gDIFont;
		}
		set
		{
			IntPtr oldFont = NativeMethods.SelectObject(mhDC, value.hFont);
			if (_OldFont == IntPtr.Zero)
			{
				_OldFont = oldFont;
			}
		}
	}

	public GDISurface(IntPtr hDC)
	{
		mhDC = hDC;
	}

	public GDISurface(int width, int height, IntPtr hdc)
	{
		Init(width, height, hdc);
		Create();
	}

	public GDISurface(int width, int height, GDISurface surface)
	{
		Init(width, height, surface.hDC);
		Create();
	}

	public GDISurface(int width, int height, Control CompatibleControl, bool BindControl)
	{
		IntPtr hdc = NativeMethods.ControlDC(CompatibleControl);
		Init(width, height, hdc);
		NativeMethods.ReleaseDC(CompatibleControl.Handle, hdc);
		if (BindControl)
		{
			Control = CompatibleControl;
		}
		Create();
	}

	protected void Init(int width, int height, IntPtr hdc)
	{
		mWidth = width;
		mHeight = height;
		mhDC = NativeMethods.CreateCompatibleDC(hdc);
		mhBMP = NativeMethods.CreateCompatibleBitmap(hdc, width, height);
		IntPtr oldBmp = NativeMethods.SelectObject(mhDC, mhBMP);
		_OldBmp = oldBmp;
		if (mhDC == (IntPtr)0)
		{
			throw new OutOfMemoryException("hDC creation FAILED!!");
		}
		if (mhDC == (IntPtr)0)
		{
			throw new OutOfMemoryException("hBMP creation FAILED!!");
		}
	}

	public Size MeasureString(string Text)
	{
		return new Size(0, 0);
	}

	public Size MeasureTabbedString(string Text, int tabsize)
	{
		int tabbedTextExtent = NativeMethods.GetTabbedTextExtent(mhDC, Text, Text.Length, 1, ref tabsize);
		return new Size(tabbedTextExtent & 0xFFFF, (tabbedTextExtent >> 16) & 0xFFFF);
	}

	public void DrawString(string Text, int x, int y, int width, int height)
	{
	}

	public Size DrawTabbedString(string Text, int x, int y, int taborigin, int tabsize)
	{
		int num = NativeMethods.TabbedTextOut(mhDC, x, y, Text, Text.Length, 1, ref tabsize, taborigin);
		return new Size(num & 0xFFFF, (num >> 16) & 0xFFFF);
	}

	public void RenderTo(IntPtr hdc, int x, int y)
	{
		NativeMethods.BitBlt(hdc, x, y, mWidth, mHeight, mhDC, 0, 0, 13369376);
	}

	public void RenderTo(GDISurface target, int x, int y)
	{
		RenderTo(target.hDC, x, y);
	}

	public void RenderTo(GDISurface target, int SourceX, int SourceY, int Width, int Height, int DestX, int DestY)
	{
		NativeMethods.BitBlt(target.hDC, DestX, DestY, Width, Height, hDC, SourceX, SourceY, 13369376);
	}

	public void RenderToControl(int x, int y)
	{
		IntPtr hdc = NativeMethods.ControlDC(Control);
		RenderTo(hdc, x, y);
		NativeMethods.ReleaseDC(Control.Handle, hdc);
	}

	public Graphics CreateGraphics()
	{
		return Graphics.FromHdc(mhDC);
	}

	public void FillRect(GDIBrush brush, int x, int y, int width, int height)
	{
		APIRect rect = default(APIRect);
		rect.top = y;
		rect.left = x;
		rect.right = width + x;
		rect.bottom = height + y;
		NativeMethods.FillRect(mhDC, ref rect, brush.hBrush);
	}

	public void DrawFocusRect(int x, int y, int width, int height)
	{
		APIRect rect = default(APIRect);
		rect.top = y;
		rect.left = x;
		rect.right = width + x;
		rect.bottom = height + y;
		NativeMethods.DrawFocusRect(mhDC, ref rect);
	}

	public void FillRect(Color color, int x, int y, int width, int height)
	{
		GDIBrush gDIBrush = new GDIBrush(color);
		FillRect(gDIBrush, x, y, width, height);
		gDIBrush.Dispose();
	}

	public void InvertRect(int x, int y, int width, int height)
	{
		APIRect rect = default(APIRect);
		rect.top = y;
		rect.left = x;
		rect.right = width + x;
		rect.bottom = height + y;
		NativeMethods.InvertRect(mhDC, ref rect);
	}

	public void DrawLine(GDIPen pen, Point p1, Point p2)
	{
		IntPtr hObject = NativeMethods.SelectObject(mhDC, pen.hPen);
		APIPoint lpPoint = default(APIPoint);
		lpPoint.x = 0;
		lpPoint.y = 0;
		NativeMethods.MoveToEx(mhDC, p1.X, p1.Y, ref lpPoint);
		NativeMethods.LineTo(mhDC, p2.X, p2.Y);
		NativeMethods.SelectObject(mhDC, hObject);
	}

	public void DrawLine(Color color, Point p1, Point p2)
	{
		GDIPen gDIPen = new GDIPen(color, 1);
		DrawLine(gDIPen, p1, p2);
		gDIPen.Dispose();
	}

	public void DrawRect(Color color, int left, int top, int width, int height)
	{
		GDIPen gDIPen = new GDIPen(color, 1);
		DrawRect(gDIPen, left, top, width, height);
		gDIPen.Dispose();
	}

	public void DrawRect(GDIPen pen, int left, int top, int width, int height)
	{
		DrawLine(pen, new Point(left, top), new Point(left + width, top));
		DrawLine(pen, new Point(left, top + height), new Point(left + width, top + height));
		DrawLine(pen, new Point(left, top), new Point(left, top + height));
		DrawLine(pen, new Point(left + width, top), new Point(left + width, top + height + 1));
	}

	public void Clear(Color color)
	{
		GDIBrush gDIBrush = new GDIBrush(color);
		Clear(gDIBrush);
		gDIBrush.Dispose();
	}

	public void Clear(GDIBrush brush)
	{
		FillRect(brush, 0, 0, mWidth, mHeight);
	}

	public void Flush()
	{
		NativeMethods.GdiFlush();
	}

	protected override void Destroy()
	{
		if (_OldBmp != IntPtr.Zero)
		{
			NativeMethods.SelectObject(hDC, _OldBmp);
		}
		if (_OldFont != IntPtr.Zero)
		{
			NativeMethods.SelectObject(hDC, _OldFont);
		}
		if (_OldPen != IntPtr.Zero)
		{
			NativeMethods.SelectObject(hDC, _OldPen);
		}
		if (_OldBrush != IntPtr.Zero)
		{
			NativeMethods.SelectObject(hDC, _OldBrush);
		}
		if (mhBMP != (IntPtr)0)
		{
			NativeMethods.DeleteObject(mhBMP);
		}
		if (mhDC != (IntPtr)0)
		{
			NativeMethods.DeleteDC(mhDC);
		}
		mhBMP = (IntPtr)0;
		mhDC = (IntPtr)0;
		base.Destroy();
	}

	public void SetBrushOrg(int x, int y)
	{
		APIPoint p = default(APIPoint);
		p.x = 0;
		p.y = 0;
		NativeMethods.SetBrushOrgEx(mhDC, x, y, ref p);
	}
}
