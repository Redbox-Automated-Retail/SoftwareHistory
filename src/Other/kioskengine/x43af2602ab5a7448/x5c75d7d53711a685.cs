using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Skybound.Windows.Forms;

namespace x43af2602ab5a7448;

internal class x5c75d7d53711a685
{
	private struct xec6fd8141c5aa2df
	{
		public int x2d7d0af5e89ed4c3;

		public int x38f68e1ecef00e14;

		public int xc731ad0995ff4803;

		public int xba1f845c7d069e92;

		public int xcca376b5d8d20f63;

		public byte x5dcebf562a97ada0;

		public byte x756f72350dc4a82a;

		public byte x99c78693def8f1a9;

		public byte x37d300c88d2225cd;

		public byte xd299990210c7e6d9;

		public byte x17b85bea39260524;

		public byte x6337df81402a9068;

		public byte xf39965c1dd63f2a8;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
		public string x1535bb3bbe793344;
	}

	private struct x8dd4b7a13a696a09(Rectangle r)
	{
		public int x72d92bd1aff02e37 = r.Left;

		public int xe360b1885d8d4a41 = r.Top;

		public int x419ba17a5322627b = r.Right;

		public int x9bcb07e204e30218 = r.Bottom;
	}

	private const int x714b859d487fa890 = 1024;

	private const int xad8d4aa157659af8 = 20;

	private const int xa6bf4ec63e08d4c4 = 2;

	private const int x3695fb3b80d1be03 = 1;

	private const int xeb874d3de81ee052 = 1048576;

	private const int x751463b7c44be16c = 0;

	private const int x7ca77572e7aa5b27 = 1;

	private const int xbb6ef66025d8cb8c = 2048;

	private const int xcbd664cf4ab5aa57 = 2;

	private const int x2531031747ab3d43 = 131072;

	private const int x69c01e08c0cc1126 = 32;

	private const int xc02aeaede83f57bf = 4;

	private const int x82c388bc15064819 = 0;

	private const int x924fc8c2f399da82 = 8;

	private static Font _x890e2cd4fcb22ff6;

	private static IntPtr _xecbdca26de6f9b7d;

	private static IntPtr _x47a5d035f277dbb5;

	private static Graphics _xea57d42d380196f7;

	private static Bitmap _xf571c8aa74b22a2e;

	private static IntPtr x47a5d035f277dbb5
	{
		get
		{
			if (_x47a5d035f277dbb5 == IntPtr.Zero)
			{
				_xf571c8aa74b22a2e = new Bitmap(1, 1);
				_xea57d42d380196f7 = Graphics.FromImage(_xf571c8aa74b22a2e);
				_x47a5d035f277dbb5 = _xea57d42d380196f7.GetHdc();
			}
			return _x47a5d035f277dbb5;
		}
	}

	[DllImport("gdi32")]
	private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hObject);

	[DllImport("user32", CharSet = CharSet.Auto)]
	private static extern int DrawText(IntPtr hdc, string lpStr, int nCount, ref x8dd4b7a13a696a09 lpRect, int wFormat);

	[DllImport("gdi32")]
	private static extern int SetTextColor(IntPtr hdc, int crColor);

	[DllImport("gdi32")]
	private static extern int GetTextColor(IntPtr hdc);

	[DllImport("gdi32")]
	private static extern int SetBkColor(IntPtr hdc, int crColor);

	[DllImport("gdi32")]
	private static extern int SetBkMode(IntPtr hdc, int nBkMode);

	[DllImport("gdi32", EntryPoint = "CreateFontIndirectA")]
	private static extern IntPtr CreateFontIndirect(ref xec6fd8141c5aa2df lpLogFont);

	[DllImport("gdi32")]
	private static extern int DeleteObject(IntPtr hObject);

	private x5c75d7d53711a685()
	{
	}

	public static void xc9f808b2238be32a(Graphics x41347a961b838962, string xb41faee6912a2313, Font x26094932cf7a9139, Color x93532ca0ace0c1ae, Color x154083d58301ef75, Rectangle xda73fcb97c77d998, TextFormatFlags x74b55ac974344a87)
	{
		if (xb41faee6912a2313 == null || xb41faee6912a2313.Length == 0 || x41347a961b838962 == null || x26094932cf7a9139 == null || xda73fcb97c77d998.Size == Size.Empty || x93532ca0ace0c1ae.Equals(Color.Empty) || x93532ca0ace0c1ae.Equals(Color.Transparent))
		{
			return;
		}
		IntPtr hdc = x41347a961b838962.GetHdc();
		try
		{
			IntPtr hObject = x9c13decea8d7ae73(x26094932cf7a9139);
			IntPtr hObject2 = SelectObject(hdc, hObject);
			if (x154083d58301ef75.Equals(Color.Empty) || x154083d58301ef75.Equals(Color.Transparent))
			{
				SetBkMode(hdc, 1);
			}
			else
			{
				SetBkMode(hdc, 2);
				SetBkColor(hdc, ColorTranslator.ToWin32(x154083d58301ef75));
			}
			int textColor = GetTextColor(hdc);
			SetTextColor(hdc, ColorTranslator.ToWin32(x93532ca0ace0c1ae));
			x8dd4b7a13a696a09 lpRect = new x8dd4b7a13a696a09(xda73fcb97c77d998);
			DrawText(hdc, xb41faee6912a2313, xb41faee6912a2313.Length, ref lpRect, (int)x74b55ac974344a87);
			SetTextColor(hdc, textColor);
			SelectObject(hdc, hObject2);
		}
		finally
		{
			x41347a961b838962.ReleaseHdc(hdc);
		}
	}

	public static Size xbf83406974150862(string xb41faee6912a2313, Font x26094932cf7a9139)
	{
		return xbf83406974150862(xb41faee6912a2313, x26094932cf7a9139, Size.Empty, TextFormatFlags.Default);
	}

	public static Size xbf83406974150862(string xb41faee6912a2313, Font x26094932cf7a9139, Size x8a5438a210b3746e, TextFormatFlags x74b55ac974344a87)
	{
		if (xb41faee6912a2313 == null || xb41faee6912a2313.Length == 0 || x26094932cf7a9139 == null)
		{
			return Size.Empty;
		}
		if (x8a5438a210b3746e == Size.Empty)
		{
			x8a5438a210b3746e = new Size(32767, 0);
		}
		IntPtr hObject = x9c13decea8d7ae73(x26094932cf7a9139);
		IntPtr hObject2 = SelectObject(x47a5d035f277dbb5, hObject);
		x8dd4b7a13a696a09 lpRect = new x8dd4b7a13a696a09(new Rectangle(0, 0, x8a5438a210b3746e.Width, x8a5438a210b3746e.Height));
		DrawText(x47a5d035f277dbb5, xb41faee6912a2313, xb41faee6912a2313.Length, ref lpRect, (int)(x74b55ac974344a87 | (TextFormatFlags)0x400));
		SelectObject(x47a5d035f277dbb5, hObject2);
		return new Size(lpRect.x419ba17a5322627b - lpRect.x72d92bd1aff02e37, lpRect.x9bcb07e204e30218 - lpRect.xe360b1885d8d4a41);
	}

	private static IntPtr x9c13decea8d7ae73(Font x26094932cf7a9139)
	{
		if (x26094932cf7a9139 == _x890e2cd4fcb22ff6)
		{
			return _xecbdca26de6f9b7d;
		}
		if (_xecbdca26de6f9b7d != IntPtr.Zero)
		{
			DeleteObject(_xecbdca26de6f9b7d);
		}
		_x890e2cd4fcb22ff6 = x26094932cf7a9139;
		return _xecbdca26de6f9b7d = x591851da5cac8490(x26094932cf7a9139);
	}

	private static IntPtr x591851da5cac8490(Font x26094932cf7a9139)
	{
		object obj = default(xec6fd8141c5aa2df);
		x26094932cf7a9139.ToLogFont(obj);
		xec6fd8141c5aa2df lpLogFont = (xec6fd8141c5aa2df)obj;
		lpLogFont.x1535bb3bbe793344 = x26094932cf7a9139.Name;
		lpLogFont.x6337df81402a9068 = 0;
		return CreateFontIndirect(ref lpLogFont);
	}
}
