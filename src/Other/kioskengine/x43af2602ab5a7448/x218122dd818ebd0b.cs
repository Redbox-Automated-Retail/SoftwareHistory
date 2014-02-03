using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace x43af2602ab5a7448;

internal class x218122dd818ebd0b : IDisposable
{
	private Graphics xa297bd7ce6c903e0;

	private Rectangle xca3ffbc3c18d1cc6;

	private Size x4891be42b61d8833;

	private IntPtr _x6ea442512bb15b8e;

	private IntPtr _x48c80e8090a157b0;

	private IntPtr _x0f9f31bbf6fdd118;

	private Graphics _xbc626ed723e04991;

	public Graphics xbc626ed723e04991
	{
		get
		{
			if (_xbc626ed723e04991 == null)
			{
				throw new InvalidOperationException("No target has been defined.");
			}
			return _xbc626ed723e04991;
		}
	}

	[DllImport("gdi32")]
	private static extern IntPtr CreateDC(string lpDriverName, IntPtr lpDeviceName, IntPtr lpOutput, IntPtr lpInitData);

	[DllImport("gdi32")]
	private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int width, int height);

	[DllImport("gdi32")]
	private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

	[DllImport("gdi32")]
	private static extern int DeleteObject(IntPtr hObject);

	[DllImport("gdi32")]
	private static extern int DeleteDC(IntPtr hdc);

	[DllImport("gdi32")]
	private static extern int BitBlt(IntPtr hDestDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);

	[DllImport("gdi32")]
	private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hObject);

	~x218122dd818ebd0b()
	{
		x8868cb45fcfa99b0();
	}

	public virtual void Dispose()
	{
		x8868cb45fcfa99b0();
		GC.SuppressFinalize(this);
	}

	public void x55fae544baab0cc8(Graphics x41347a961b838962, Rectangle xda73fcb97c77d998)
	{
		if (x41347a961b838962 == null)
		{
			throw new ArgumentException("graphics");
		}
		if (x4891be42b61d8833.Width < xda73fcb97c77d998.Width || x4891be42b61d8833.Height < xda73fcb97c77d998.Height)
		{
			x8868cb45fcfa99b0();
			x0efc34769f078ca5(xda73fcb97c77d998.Size.Width + xda73fcb97c77d998.Size.Width % 16, xda73fcb97c77d998.Size.Height + xda73fcb97c77d998.Size.Height % 16);
		}
		xa297bd7ce6c903e0 = x41347a961b838962;
		xca3ffbc3c18d1cc6 = xda73fcb97c77d998;
	}

	private void x0efc34769f078ca5(int x9b0739496f8b5475, int x4d5aabc7a55b12ba)
	{
		IntPtr hdc = CreateDC("DISPLAY", IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
		try
		{
			_x6ea442512bb15b8e = CreateCompatibleBitmap(hdc, x9b0739496f8b5475, x4d5aabc7a55b12ba);
			_x48c80e8090a157b0 = CreateCompatibleDC(hdc);
			_x0f9f31bbf6fdd118 = SelectObject(_x48c80e8090a157b0, _x6ea442512bb15b8e);
			_xbc626ed723e04991 = Graphics.FromHdc(_x48c80e8090a157b0);
			ref Size reference = ref x4891be42b61d8833;
			reference = new Size(x9b0739496f8b5475, x4d5aabc7a55b12ba);
		}
		finally
		{
			DeleteDC(hdc);
		}
		GC.KeepAlive(this);
	}

	private void x8868cb45fcfa99b0()
	{
		if (_x6ea442512bb15b8e != IntPtr.Zero)
		{
			if (_xbc626ed723e04991 != null)
			{
				_xbc626ed723e04991.Dispose();
			}
			SelectObject(_x48c80e8090a157b0, _x0f9f31bbf6fdd118);
			DeleteObject(_x6ea442512bb15b8e);
			DeleteDC(_x48c80e8090a157b0);
			_x6ea442512bb15b8e = IntPtr.Zero;
			_x48c80e8090a157b0 = IntPtr.Zero;
			x4891be42b61d8833 = Size.Empty;
			GC.KeepAlive(this);
		}
	}

	public void xe406325e56f74b46()
	{
		if (xa297bd7ce6c903e0 == null)
		{
			throw new InvalidOperationException("No target has been defined.");
		}
		IntPtr hdc = xa297bd7ce6c903e0.GetHdc();
		IntPtr hdc2 = xbc626ed723e04991.GetHdc();
		try
		{
			BitBlt(hdc, xca3ffbc3c18d1cc6.X, xca3ffbc3c18d1cc6.Y, xca3ffbc3c18d1cc6.Width, xca3ffbc3c18d1cc6.Height, hdc2, 0, 0, 13369376);
		}
		finally
		{
			xbc626ed723e04991.ReleaseHdc(hdc2);
			xa297bd7ce6c903e0.ReleaseHdc(hdc);
		}
	}
}
