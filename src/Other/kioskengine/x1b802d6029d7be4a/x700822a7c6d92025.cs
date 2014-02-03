using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Skybound.VisualTips;
using Skybound.VisualTips.Rendering;
using Skybound.Windows.Forms;
using x43af2602ab5a7448;

namespace x1b802d6029d7be4a;

internal class x700822a7c6d92025 : Form
{
	private struct xaf08eaf97a1f0f68(int x, int y)
	{
		public int x8df91a2f90079e88 = x;

		public int xc821290d7ecc08bf = y;
	}

	private struct x06a9386d4b1957f6(int cx, int cy)
	{
		public int x681aa248b73b31f9 = cx;

		public int x81150e9fca528da1 = cy;
	}

	private struct xed11c1b8d14f577b
	{
		public byte x95889fe5dd21cfca;

		public byte x6db14cc85c0533e0;

		public byte x151ef119ba990bae;

		public byte xa7f6a856bdad9e82;
	}

	private const int x1f0fd5045d7e2d10 = 0;

	private const int x05ba99717363133e = 1;

	private const int x2dab45eb0f3c6fcc = 5;

	private const int x4c4ed64783077b76 = 4;

	private const int x5369785684a974f4 = 1;

	private const int x93b283a033d1b54a = 2;

	private const int x11a0985503a2d2df = 4;

	private const byte xdd6043f42253ee15 = 0;

	private const byte xa34cc3e091661d7f = 1;

	private VisualTip _x8c3cc20aa74dd99c;

	private bool _x7965faace5d79aa4;

	private VisualTipDisplayOptions _x73979cef1002ed01;

	private x218122dd818ebd0b x4ae5537db27c5fe4 = new x218122dd818ebd0b();

	private VisualTipProvider xb6fac368c5962c3d;

	private xab10d51bca8cf7d4 xab10d51bca8cf7d4;

	private double x1b684d050cd00a01;

	private IntPtr x6e0aee512a2fa724;

	private Size xa86b0f5a65f9e838;

	private bool x08799943903cd34a => VisualTipRenderer.x9843c083bd22e3f5;

	protected override CreateParams CreateParams
	{
		get
		{
			CreateParams createParams = base.CreateParams;
			if (x08799943903cd34a)
			{
				createParams.ExStyle |= 524288;
			}
			return createParams;
		}
	}

	public VisualTip x8c3cc20aa74dd99c
	{
		get
		{
			if (!x7965faace5d79aa4)
			{
				return null;
			}
			return _x8c3cc20aa74dd99c;
		}
	}

	public bool x7965faace5d79aa4 => _x7965faace5d79aa4;

	public VisualTipDisplayOptions x73979cef1002ed01 => _x73979cef1002ed01;

	[DllImport("user32")]
	private static extern int ShowWindow(IntPtr hwnd, int nCmdShow);

	[DllImport("user32")]
	private static extern IntPtr GetDC(IntPtr hwnd);

	[DllImport("gdi32")]
	private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

	[DllImport("user32")]
	private static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

	[DllImport("gdi32")]
	private static extern int DeleteDC(IntPtr hdc);

	[DllImport("gdi32")]
	private static extern int DeleteObject(IntPtr hObject);

	[DllImport("gdi32")]
	private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hObject);

	[DllImport("user32")]
	private static extern IntPtr UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref xaf08eaf97a1f0f68 pptDst, ref x06a9386d4b1957f6 psize, IntPtr hdcSrc, ref xaf08eaf97a1f0f68 pprSrc, int crKey, ref xed11c1b8d14f577b pblend, int dwFlags);

	[DllImport("user32")]
	private static extern int SetWindowPos(IntPtr hwnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);

	public x700822a7c6d92025()
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, value: true);
		base.StartPosition = FormStartPosition.Manual;
		base.FormBorderStyle = FormBorderStyle.None;
		base.ShowInTaskbar = false;
		xab10d51bca8cf7d4 = new xab10d51bca8cf7d4(Motion.Decelerate, 200);
		xab10d51bca8cf7d4.xf61701b848da9540 += x5901cb1455714d0c;
		xab10d51bca8cf7d4.x061479d2a6161ad7 = this;
	}

	protected override void Dispose(bool disposing)
	{
		xf7d347f09836da24();
		base.Dispose(disposing);
	}

	public void x4503f6152d2bb863(Rectangle xd19889359ad9e56f, VisualTipDisplayOptions xdfde339da46db651)
	{
		((Control)this).Location = x1a80bcf42c531264(xd19889359ad9e56f, base.Size, xdfde339da46db651);
		_x73979cef1002ed01 = xdfde339da46db651;
	}

	private Point x1a80bcf42c531264(Rectangle xd19889359ad9e56f, Size x74df63607ed76335, VisualTipDisplayOptions xdfde339da46db651)
	{
		Rectangle bounds = Screen.GetBounds(xd19889359ad9e56f);
		bool flag = RightToLeft == RightToLeft.No;
		Rectangle rectangle = new Rectangle(xd19889359ad9e56f.Location, x74df63607ed76335);
		bool flag2 = (xdfde339da46db651 & VisualTipDisplayOptions.PositionMask) == VisualTipDisplayOptions.PositionLeft;
		bool flag3 = (xdfde339da46db651 & VisualTipDisplayOptions.PositionMask) == VisualTipDisplayOptions.PositionMask;
		if (flag2 || (xdfde339da46db651 & VisualTipDisplayOptions.PositionMask) == VisualTipDisplayOptions.PositionRight)
		{
			if (flag && !flag2)
			{
				if (xd19889359ad9e56f.Right + x74df63607ed76335.Width > bounds.Right && xd19889359ad9e56f.Left - x74df63607ed76335.Width >= bounds.X)
				{
					rectangle.X -= x74df63607ed76335.Width;
				}
				else
				{
					rectangle.X += xd19889359ad9e56f.Width;
				}
			}
			else if (xd19889359ad9e56f.Left - x74df63607ed76335.Width < bounds.X && xd19889359ad9e56f.Right + x74df63607ed76335.Width <= bounds.Right)
			{
				rectangle.X += xd19889359ad9e56f.Width;
			}
			else
			{
				rectangle.X -= x74df63607ed76335.Width;
			}
		}
		else
		{
			if (!flag)
			{
				rectangle.X -= x74df63607ed76335.Width - xd19889359ad9e56f.Width;
			}
			if (flag3)
			{
				if (xd19889359ad9e56f.Y - x74df63607ed76335.Height < bounds.Y && xd19889359ad9e56f.Bottom + x74df63607ed76335.Height <= bounds.Bottom)
				{
					rectangle.Y += xd19889359ad9e56f.Height;
				}
				else
				{
					rectangle.Y -= x74df63607ed76335.Height;
				}
			}
			else if (xd19889359ad9e56f.Bottom + x74df63607ed76335.Height > bounds.Bottom && xd19889359ad9e56f.Y - x74df63607ed76335.Height >= bounds.Y)
			{
				rectangle.Y -= x74df63607ed76335.Height;
			}
			else
			{
				rectangle.Y += xd19889359ad9e56f.Height;
			}
		}
		if (rectangle.Right > bounds.Right)
		{
			rectangle.X = bounds.Right - rectangle.Width;
		}
		if (rectangle.X < bounds.X)
		{
			rectangle.X = bounds.X;
		}
		if (rectangle.Bottom > bounds.Bottom)
		{
			rectangle.Y = bounds.Bottom - rectangle.Height;
		}
		if (rectangle.Y < bounds.Y)
		{
			rectangle.Y = bounds.Y;
		}
		return rectangle.Location;
	}

	public void x2c6cfc461f9dd5e2(VisualTipProvider x83d0c26038874b92, VisualTip x2636ebb6bd343dac, Rectangle xd19889359ad9e56f, VisualTipDisplayOptions xdfde339da46db651)
	{
		xb6fac368c5962c3d = x83d0c26038874b92;
		_x8c3cc20aa74dd99c = x2636ebb6bd343dac;
		_x73979cef1002ed01 = xdfde339da46db651;
		RightToLeft = x2636ebb6bd343dac.RightToLeft;
		VisualTipLayout visualTipLayout = x83d0c26038874b92.Renderer.CreateLayout(x2636ebb6bd343dac);
		base.Size = visualTipLayout.GetSize();
		((Control)this).Location = x1a80bcf42c531264(xd19889359ad9e56f, base.Size, xdfde339da46db651);
		xd19889359ad9e56f.Location -= new Size(((Control)this).Location);
		x2636ebb6bd343dac.x2d5b2d77afb66528(xd19889359ad9e56f);
		if (x08799943903cd34a)
		{
			xab10d51bca8cf7d4.xac2b8ecd963a9a83();
			using Bitmap bitmap = new Bitmap(base.Width, base.Height);
			using Graphics graphics = Graphics.FromImage(bitmap);
			x83d0c26038874b92.Renderer.Draw(new PaintEventArgs(graphics, new Rectangle(0, 0, base.Width, base.Height)), x2636ebb6bd343dac, visualTipLayout);
			xfdc218ad4dbe03d7(bitmap, (byte)(xb6fac368c5962c3d.Opacity * 255.0));
		}
		x38b0fd944adcc7f3();
		if (!x08799943903cd34a)
		{
			base.Width = visualTipLayout.WindowBounds.Width;
			base.Region = xb6fac368c5962c3d.Renderer.CreateMaskRegion(x2636ebb6bd343dac, visualTipLayout);
		}
	}

	private void x38b0fd944adcc7f3()
	{
		SetWindowPos(base.Handle, new IntPtr(-1), 0, 0, 0, 0, 83);
		_x7965faace5d79aa4 = true;
		base.Opacity = 1.0;
	}

	public void xd7692f35479850d5()
	{
		if (x7965faace5d79aa4)
		{
			_x7965faace5d79aa4 = false;
			_x8c3cc20aa74dd99c = null;
			if ((x08799943903cd34a && xb6fac368c5962c3d.Animation == VisualTipAnimation.Enabled) || (xb6fac368c5962c3d.Animation == VisualTipAnimation.SystemDefault && xc1ab0cc8a4568e7b.x6b254b18d1dfb65f))
			{
				x1b684d050cd00a01 = xb6fac368c5962c3d.Opacity;
				xab10d51bca8cf7d4.x12cb12b5d2cad53d();
			}
			else
			{
				ShowWindow(base.Handle, 0);
			}
			xb6fac368c5962c3d.xc590aa98ef979eff(this);
		}
	}

	private void x5901cb1455714d0c(object xe0292b9ed559da7d, EventArgs xfbf34718e704c6bc)
	{
		if (xab10d51bca8cf7d4.xda1d1aa1eef530a1)
		{
			ShowWindow(base.Handle, 0);
		}
		else
		{
			xb193046fda76ec30((byte)((1.0 - xab10d51bca8cf7d4.xd2f68ee6f47e9dfb) * (x1b684d050cd00a01 * 255.0)));
		}
	}

	private void xfdc218ad4dbe03d7(Bitmap xe205f0cd81228282, byte x1965e484c4a7c6c6)
	{
		xf7d347f09836da24();
		x6e0aee512a2fa724 = xe205f0cd81228282.GetHbitmap(Color.FromArgb(0));
		xa86b0f5a65f9e838 = xe205f0cd81228282.Size;
		xb193046fda76ec30(x1965e484c4a7c6c6);
	}

	private void xb193046fda76ec30(byte x1965e484c4a7c6c6)
	{
		IntPtr dC = GetDC(IntPtr.Zero);
		IntPtr intPtr = CreateCompatibleDC(dC);
		IntPtr hObject = IntPtr.Zero;
		try
		{
			hObject = SelectObject(intPtr, x6e0aee512a2fa724);
			xaf08eaf97a1f0f68 pptDst = new xaf08eaf97a1f0f68(base.Left, base.Top);
			x06a9386d4b1957f6 psize = new x06a9386d4b1957f6(xa86b0f5a65f9e838.Width, xa86b0f5a65f9e838.Height);
			xaf08eaf97a1f0f68 pprSrc = default(xaf08eaf97a1f0f68);
			xed11c1b8d14f577b pblend = new xed11c1b8d14f577b
			{
				x95889fe5dd21cfca = 0,
				x6db14cc85c0533e0 = 0,
				x151ef119ba990bae = x1965e484c4a7c6c6,
				xa7f6a856bdad9e82 = 1
			};
			UpdateLayeredWindow(base.Handle, dC, ref pptDst, ref psize, intPtr, ref pprSrc, 0, ref pblend, 2);
		}
		finally
		{
			ReleaseDC(IntPtr.Zero, dC);
			SelectObject(intPtr, hObject);
			DeleteDC(intPtr);
		}
	}

	private void xf7d347f09836da24()
	{
		if (x6e0aee512a2fa724 != IntPtr.Zero)
		{
			DeleteObject(x6e0aee512a2fa724);
			x6e0aee512a2fa724 = IntPtr.Zero;
			xa86b0f5a65f9e838 = Size.Empty;
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		if (!x08799943903cd34a)
		{
			x4ae5537db27c5fe4.x55fae544baab0cc8(e.Graphics, base.ClientRectangle);
			xb6fac368c5962c3d.Renderer.Draw(new PaintEventArgs(x4ae5537db27c5fe4.xbc626ed723e04991, e.ClipRectangle), x8c3cc20aa74dd99c, null);
			x4ae5537db27c5fe4.xe406325e56f74b46();
		}
	}

	protected override void WndProc(ref Message m)
	{
		if (m.Msg == 33)
		{
			xd7692f35479850d5();
			m.Result = new IntPtr(4);
		}
		else
		{
			base.WndProc(ref m);
		}
	}
}
