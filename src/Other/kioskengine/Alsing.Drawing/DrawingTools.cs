using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Alsing.Windows;
using Alsing.Windows.Forms;

namespace Alsing.Drawing;

public static class DrawingTools
{
	public static Color MixColors(Color c1, Color c2, double mix)
	{
		return Color.FromArgb((int)((double)(int)c1.R * (1.0 - mix) + (double)(int)c2.R * mix), (int)((double)(int)c1.G * (1.0 - mix) + (double)(int)c2.G * mix), (int)((double)(int)c1.B * (1.0 - mix) + (double)(int)c2.B * mix));
	}

	private static void DrawBorder(Border3DStyle Style, Color BorderColor, Graphics g, Rectangle r)
	{
		using SolidBrush normal = GetNormalBrush(BorderColor);
		using SolidBrush light = GetLightBrush(BorderColor);
		using SolidBrush dark = GetDarkBrush(BorderColor);
		using SolidBrush darkdark = GetDarkDarkBrush(BorderColor);
		switch (Style)
		{
		case Border3DStyle.Sunken:
			DrawSunkenBorder(g, dark, r, darkdark, light, normal);
			break;
		case Border3DStyle.Raised:
			DrawRaisedBorder(g, dark, r, darkdark, light, normal);
			break;
		case Border3DStyle.RaisedInner:
			RaisedInnerBorder(g, dark, r, light);
			break;
		case Border3DStyle.SunkenOuter:
			DrawSunkenOuterBorder(g, dark, r, light);
			break;
		case (Border3DStyle)3:
		case Border3DStyle.Etched:
		case (Border3DStyle)7:
		case Border3DStyle.SunkenInner:
		case Border3DStyle.Bump:
			break;
		}
	}

	private static void DrawSunkenOuterBorder(Graphics g, Brush dark, Rectangle r, Brush light)
	{
		g.FillRectangle(dark, r.Left, r.Top, r.Width, 1);
		g.FillRectangle(dark, r.Left, r.Top, 1, r.Height);
		g.FillRectangle(light, r.Right - 1, r.Top + 1, 1, r.Height - 1);
		g.FillRectangle(light, r.Left + 1, r.Bottom - 1, r.Width - 1, 1);
	}

	private static void RaisedInnerBorder(Graphics g, Brush dark, Rectangle r, Brush light)
	{
		g.FillRectangle(light, r.Left, r.Top, r.Width - 1, 1);
		g.FillRectangle(light, r.Left, r.Top, 1, r.Height - 1);
		g.FillRectangle(dark, r.Right - 1, r.Top, 1, r.Height);
		g.FillRectangle(dark, r.Left, r.Bottom - 1, r.Width, 1);
	}

	private static void DrawRaisedBorder(Graphics g, Brush dark, Rectangle r, Brush darkdark, Brush light, Brush normal)
	{
		g.FillRectangle(normal, r.Left, r.Top, r.Width - 1, 1);
		g.FillRectangle(normal, r.Left, r.Top, 1, r.Height - 1);
		g.FillRectangle(light, r.Left + 1, r.Top + 1, r.Width - 2, 1);
		g.FillRectangle(light, r.Left + 1, r.Top + 1, 1, r.Height - 2);
		g.FillRectangle(darkdark, r.Right - 1, r.Top, 1, r.Height);
		g.FillRectangle(darkdark, r.Left, r.Bottom - 1, r.Width, 1);
		g.FillRectangle(dark, r.Right - 2, r.Top + 1, 1, r.Height - 2);
		g.FillRectangle(dark, r.Left + 1, r.Bottom - 2, r.Width - 2, 1);
	}

	private static void DrawSunkenBorder(Graphics g, Brush dark, Rectangle r, Brush darkdark, Brush light, Brush normal)
	{
		g.FillRectangle(dark, r.Left, r.Top, r.Width, 1);
		g.FillRectangle(dark, r.Left, r.Top, 1, r.Height);
		g.FillRectangle(darkdark, r.Left + 1, r.Top + 1, r.Width - 2, 1);
		g.FillRectangle(darkdark, r.Left + 1, r.Top + 1, 1, r.Height - 2);
		g.FillRectangle(light, r.Right - 1, r.Top + 1, 1, r.Height - 1);
		g.FillRectangle(light, r.Left + 1, r.Bottom - 1, r.Width - 1, 1);
		g.FillRectangle(normal, r.Right - 2, r.Top + 2, 1, r.Height - 3);
		g.FillRectangle(normal, r.Left + 2, r.Bottom - 2, r.Width - 3, 1);
	}

	private static SolidBrush GetNormalBrush(Color BorderColor)
	{
		return new SolidBrush(BorderColor);
	}

	private static SolidBrush GetLightBrush(Color BorderColor)
	{
		if (!((double)BorderColor.GetBrightness() > 0.6))
		{
			return new SolidBrush(MixColors(BorderColor, Color.White, 0.5));
		}
		return new SolidBrush(MixColors(BorderColor, Color.White, 1.0));
	}

	private static SolidBrush GetDarkDarkBrush(Color BorderColor)
	{
		return ((double)BorderColor.GetBrightness() < 0.5) ? new SolidBrush(MixColors(BorderColor, Color.Black, 1.0)) : new SolidBrush(MixColors(BorderColor, Color.Black, 0.6));
	}

	private static SolidBrush GetDarkBrush(Color BorderColor)
	{
		return ((double)BorderColor.GetBrightness() < 0.5) ? new SolidBrush(MixColors(BorderColor, Color.Black, 0.7)) : new SolidBrush(MixColors(BorderColor, Color.Black, 0.4));
	}

	public static void DrawBorder(BorderStyle2 Style, Color BorderColor, Graphics g, Rectangle r)
	{
		switch (Style)
		{
		case BorderStyle2.Dotted:
		{
			r.Width--;
			r.Height--;
			g.DrawRectangle(new Pen(SystemColors.Control), r);
			Pen pen3 = new Pen(BorderColor);
			pen3.DashStyle = DashStyle.Dot;
			Pen pen4 = pen3;
			g.DrawRectangle(pen4, r);
			break;
		}
		case BorderStyle2.Dashed:
		{
			r.Width--;
			r.Height--;
			g.DrawRectangle(new Pen(SystemColors.Control), r);
			Pen pen = new Pen(BorderColor);
			pen.DashStyle = DashStyle.Dash;
			Pen pen2 = pen;
			g.DrawRectangle(pen2, r);
			break;
		}
		case BorderStyle2.Sunken:
			if (BorderColor == Color.Black)
			{
				BorderColor = SystemColors.Control;
			}
			DrawBorder(Border3DStyle.Sunken, BorderColor, g, r);
			break;
		case BorderStyle2.FixedSingle:
			r.Width--;
			r.Height--;
			g.DrawRectangle(new Pen(BorderColor), r);
			break;
		case BorderStyle2.FixedDouble:
			g.DrawRectangle(new Pen(BorderColor), r.Left, r.Top, r.Width - 1, r.Height - 1);
			g.DrawRectangle(new Pen(BorderColor), r.Left + 1, r.Top + 1, r.Width - 3, r.Height - 3);
			break;
		case BorderStyle2.Raised:
			if (BorderColor == Color.Black)
			{
				BorderColor = SystemColors.Control;
			}
			DrawBorder(Border3DStyle.Raised, BorderColor, g, r);
			break;
		case BorderStyle2.RaisedThin:
			if (BorderColor == Color.Black)
			{
				BorderColor = Color.FromArgb(SystemColors.Control.R, SystemColors.Control.G, SystemColors.Control.B);
			}
			DrawBorder(Border3DStyle.RaisedInner, BorderColor, g, r);
			break;
		case BorderStyle2.SunkenThin:
			if (BorderColor == Color.Black)
			{
				BorderColor = Color.FromArgb(SystemColors.Control.R, SystemColors.Control.G, SystemColors.Control.B);
			}
			DrawBorder(Border3DStyle.SunkenOuter, BorderColor, g, r);
			break;
		case BorderStyle2.Etched:
			ControlPaint.DrawBorder3D(g, r, Border3DStyle.Etched);
			break;
		case BorderStyle2.Bump:
		{
			if (BorderColor == Color.Black)
			{
				BorderColor = SystemColors.Control;
			}
			SolidBrush solidBrush = new SolidBrush(BorderColor);
			g.FillRectangle(solidBrush, r.Left, r.Top, r.Width, 4);
			g.FillRectangle(solidBrush, r.Left, r.Bottom - 4, r.Width, 4);
			g.FillRectangle(solidBrush, r.Left, r.Top, 4, r.Height);
			g.FillRectangle(solidBrush, r.Right - 4, r.Top, 4, r.Height);
			solidBrush.Dispose();
			DrawBorder(Border3DStyle.Raised, BorderColor, g, r);
			DrawBorder(Border3DStyle.Sunken, BorderColor, g, new Rectangle(r.Left + 4, r.Top + 4, r.Width - 8, r.Height - 8));
			break;
		}
		case BorderStyle2.Column:
		{
			SolidBrush brush3 = new SolidBrush(MixColors(BorderColor, Color.White, 1.0));
			SolidBrush brush4 = new SolidBrush(MixColors(BorderColor, Color.Black, 0.4));
			g.FillRectangle(brush3, r.Left, r.Top, r.Width, 1);
			g.FillRectangle(brush3, r.Left, r.Top + 3, 1, r.Height - 1 - 6);
			g.FillRectangle(brush4, r.Right - 1, r.Top + 3, 1, r.Height - 6);
			g.FillRectangle(brush4, r.Left, r.Bottom - 1, r.Width, 1);
			break;
		}
		case BorderStyle2.Row:
		{
			SolidBrush brush = new SolidBrush(MixColors(BorderColor, Color.White, 1.0));
			SolidBrush brush2 = new SolidBrush(MixColors(BorderColor, Color.Black, 0.4));
			g.FillRectangle(brush, r.Left + 3, r.Top, r.Width - 6, 1);
			g.FillRectangle(brush, r.Left, r.Top, 1, r.Height - 1);
			g.FillRectangle(brush2, r.Right - 1, r.Top, 1, r.Height);
			g.FillRectangle(brush2, r.Left + 3, r.Bottom - 1, r.Width - 6, 1);
			break;
		}
		}
	}

	public static void DrawDesignTimeLine(Graphics g, int x1, int y1, int x2, int y2)
	{
		Pen pen = new Pen(SystemColors.ControlDarkDark);
		pen.DashOffset = 10f;
		pen.DashStyle = DashStyle.Dash;
		Pen pen2 = pen;
		g.DrawLine(pen2, x1, y1, x2, y2);
		pen2.Dispose();
	}

	public static void DrawGrayImage(Graphics g, Image Image, int X, int Y, float TransparencyFactor)
	{
		ColorMatrix colorMatrix = new ColorMatrix();
		ImageAttributes imageAttributes = new ImageAttributes();
		colorMatrix.Matrix33 = TransparencyFactor;
		colorMatrix.Matrix00 = 1f / 3f;
		colorMatrix.Matrix01 = 1f / 3f;
		colorMatrix.Matrix02 = 1f / 3f;
		colorMatrix.Matrix10 = 1f / 3f;
		colorMatrix.Matrix11 = 1f / 3f;
		colorMatrix.Matrix12 = 1f / 3f;
		colorMatrix.Matrix20 = 1f / 3f;
		colorMatrix.Matrix21 = 1f / 3f;
		colorMatrix.Matrix22 = 1f / 3f;
		imageAttributes.SetColorMatrix(colorMatrix);
		g.DrawImage(Image, new Rectangle(X, Y, Image.Width, Image.Height), 0, 0, Image.Width, Image.Height, GraphicsUnit.Pixel, imageAttributes);
	}

	public static void DrawTransparentImage(Graphics g, Image Image, int X, int Y, float TransparencyFactor)
	{
		ImageAttributes imageAttributes = new ImageAttributes();
		ColorMatrix colorMatrix = new ColorMatrix();
		colorMatrix.Matrix33 = TransparencyFactor;
		colorMatrix.Matrix00 = 1f;
		colorMatrix.Matrix11 = 1f;
		colorMatrix.Matrix22 = 1f;
		ColorMatrix colorMatrix2 = colorMatrix;
		imageAttributes.SetColorMatrix(colorMatrix2);
		g.DrawImage(Image, new Rectangle(X, Y, Image.Width, Image.Height), 0, 0, Image.Width, Image.Height, GraphicsUnit.Pixel, imageAttributes);
	}

	public static void DrawDesignTimeBorder(Graphics g, Rectangle rect)
	{
		rect.Width--;
		rect.Height--;
		Pen pen = new Pen(SystemColors.ControlDarkDark);
		pen.DashStyle = DashStyle.Dash;
		Pen pen2 = pen;
		g.DrawRectangle(pen2, rect);
		pen2.Dispose();
	}

	public static void DrawInsertIndicatorH(int x, int y, int width, Graphics g, Color c)
	{
		y -= 3;
		x -= 2;
		ControlPaint.FillReversibleRectangle(new Rectangle(x, y, 2, 7), c);
		ControlPaint.FillReversibleRectangle(new Rectangle(x + 2, y + 1, width, 5), c);
		ControlPaint.FillReversibleRectangle(new Rectangle(width + 2 + x, y, 2, 7), c);
	}

	public static void DrawInsertIndicatorV(int x, int y, int height, Graphics g, Color c)
	{
		x -= 3;
		y -= 2;
		ControlPaint.FillReversibleRectangle(new Rectangle(x, y, 7, 2), c);
		ControlPaint.FillReversibleRectangle(new Rectangle(x + 1, y + 2, 5, height), c);
		ControlPaint.FillReversibleRectangle(new Rectangle(x, height + 2 + y, 7, 2), c);
	}

	public static Bitmap DrawControl(Control control)
	{
		Bitmap bitmap = new Bitmap(control.Width, control.Height);
		Graphics graphics = Graphics.FromImage(bitmap);
		IntPtr hdc = graphics.GetHdc();
		NativeMethods.SendMessage(control.Handle, 791, (int)hdc, 12);
		graphics.ReleaseHdc(hdc);
		graphics.Dispose();
		return bitmap;
	}

	public static bool DrawControl(Control control, Bitmap b)
	{
		Graphics graphics = Graphics.FromImage(b);
		IntPtr hdc = graphics.GetHdc();
		int num = NativeMethods.SendMessage(control.Handle, 791, (int)hdc, 12);
		graphics.ReleaseHdc(hdc);
		graphics.Dispose();
		return num != 0;
	}

	public static void DrawSortArrow(int x, int y, Graphics g, bool Ascending)
	{
		Color color = Color.FromArgb(220, 255, 255, 255);
		Color color2 = Color.FromArgb(140, 0, 0, 0);
		Pen pen = new Pen(color);
		Pen pen2 = new Pen(color2);
		if (Ascending)
		{
			g.DrawLine(pen, x, y + 6, x + 7, y + 6);
			g.DrawLine(pen2, x + 3, y, x, y + 6);
			g.DrawLine(pen, x + 4, y, x + 7, y + 6);
		}
		else
		{
			g.DrawLine(pen2, x, y, x + 7, y);
			g.DrawLine(pen2, x, y, x + 3, y + 6);
			g.DrawLine(pen, x + 7, y, x + 4, y + 6);
		}
	}
}
