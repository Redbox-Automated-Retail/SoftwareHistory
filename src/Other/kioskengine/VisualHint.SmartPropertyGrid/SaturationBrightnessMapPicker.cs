using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

[ToolboxItem(false)]
public class SaturationBrightnessMapPicker : Control
{
	private double _hue;

	private double _saturation;

	private double _brightness;

	private Color _color = Color.Empty;

	private bool _dragInProgress;

	private bool _needToRecrateInternalImage = true;

	private Image _saturationBrightnessImage;

	public double Hue
	{
		set
		{
			_hue = value;
			_color = Color.FromArgb(_color.A, ColorUtils.HsbToRgb(_hue, _saturation, _brightness));
			_needToRecrateInternalImage = true;
			Invalidate();
		}
	}

	public Color Color
	{
		get
		{
			return _color;
		}
		set
		{
			if (_color == Color.Empty)
			{
				_color = value;
				ColorUtils.RgbToHsb(_color, out _hue, out _saturation, out _brightness);
			}
			else
			{
				_color = value;
			}
			_needToRecrateInternalImage = true;
			Invalidate();
		}
	}

	public event EventHandler ValueChanged;

	public SaturationBrightnessMapPicker()
	{
		SetStyle(Utilities.DoubleBufferStyle | ControlStyles.Opaque, value: true);
		UpdateStyles();
	}

	public void ResetColor()
	{
		_color = Color.Empty;
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Rectangle clientRectangle = base.ClientRectangle;
		if (_needToRecrateInternalImage)
		{
			_saturationBrightnessImage = new Bitmap(clientRectangle.Width, clientRectangle.Height, e.Graphics);
			_needToRecrateInternalImage = false;
			Graphics graphics = Graphics.FromImage(_saturationBrightnessImage);
			Color color = ColorUtils.HsbToRgb(_hue, 100.0, 100.0);
			double num = (double)(255 - color.R) / (double)clientRectangle.Width;
			double num2 = (double)(255 - color.G) / (double)clientRectangle.Width;
			double num3 = (double)(255 - color.B) / (double)clientRectangle.Width;
			double num4 = 255.0;
			double num5 = 255.0;
			double num6 = 255.0;
			for (int i = clientRectangle.Left; i <= clientRectangle.Right; i++)
			{
				using (LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(i, clientRectangle.Top, 1, clientRectangle.Height + 1), Color.FromArgb((int)Math.Round(num4), (int)Math.Round(num5), (int)Math.Round(num6)), Color.FromArgb(0, 0, 0), 90f, isAngleScaleable: false))
				{
					graphics.FillRectangle(brush, new Rectangle(i, clientRectangle.Top, 1, clientRectangle.Height + 1));
				}
				num4 -= num;
				num5 -= num2;
				num6 -= num3;
			}
			graphics.Dispose();
		}
		e.Graphics.DrawImage(_saturationBrightnessImage, 0, 0);
		int num7 = (int)Math.Round(_saturation / 100.0 * (double)clientRectangle.Width);
		int num8 = clientRectangle.Height - (int)Math.Round(_brightness / 100.0 * (double)clientRectangle.Height);
		Rectangle rect = new Rectangle(num7 - 5, num8 - 5, 10, 10);
		using (Pen pen = new Pen(Color.White))
		{
			e.Graphics.DrawEllipse(pen, rect);
		}
		rect.Inflate(1, 1);
		using Pen pen2 = new Pen(Color.Black);
		e.Graphics.DrawEllipse(pen2, rect);
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	protected override void WndProc(ref Message m)
	{
		if (m.Msg == 131)
		{
			Win32Calls.NCCALCSIZE_PARAMS nCCALCSIZE_PARAMS = (Win32Calls.NCCALCSIZE_PARAMS)Marshal.PtrToStructure(m.LParam, typeof(Win32Calls.NCCALCSIZE_PARAMS));
			nCCALCSIZE_PARAMS.rgrc0.Left += 2;
			nCCALCSIZE_PARAMS.rgrc0.Right -= 2;
			nCCALCSIZE_PARAMS.rgrc0.Top += 2;
			nCCALCSIZE_PARAMS.rgrc0.Bottom -= 2;
			Marshal.StructureToPtr(nCCALCSIZE_PARAMS, m.LParam, fDeleteOld: false);
		}
		else if (m.Msg == 133)
		{
			IntPtr windowDC = Win32Calls.GetWindowDC(m.HWnd);
			Graphics graphics = Graphics.FromHdc(windowDC);
			Win32Calls.RECT rc = default(Win32Calls.RECT);
			Win32Calls.GetWindowRect(m.HWnd, ref rc);
			Rectangle rectangle = new Rectangle(0, 0, rc.Right - rc.Left, rc.Bottom - rc.Top);
			if (Focused)
			{
				ControlPaint.DrawFocusRectangle(graphics, rectangle);
			}
			else
			{
				Rectangle rect = rectangle;
				rect.Width--;
				rect.Height--;
				using Pen pen = new Pen(base.Parent.BackColor);
				graphics.DrawRectangle(pen, rect);
			}
			rectangle.Inflate(-1, -1);
			rectangle.Width--;
			rectangle.Height--;
			graphics.DrawLine(SystemPens.ControlDarkDark, new Point(rectangle.Left, rectangle.Bottom), rectangle.Location);
			graphics.DrawLine(SystemPens.ControlDarkDark, rectangle.Location, new Point(rectangle.Right, rectangle.Top));
			graphics.DrawLine(SystemPens.ControlLightLight, new Point(rectangle.Left, rectangle.Bottom), new Point(rectangle.Right, rectangle.Bottom));
			graphics.DrawLine(SystemPens.ControlLightLight, new Point(rectangle.Right, rectangle.Bottom), new Point(rectangle.Right, rectangle.Top));
			Win32Calls.ReleaseDC(m.HWnd, windowDC);
			graphics.Dispose();
			return;
		}
		base.WndProc(ref m);
	}

	protected override void OnGotFocus(EventArgs e)
	{
		Win32Calls.RedrawWindow(base.Handle, IntPtr.Zero, IntPtr.Zero, 1025);
		base.OnGotFocus(e);
	}

	protected override void OnLostFocus(EventArgs e)
	{
		Win32Calls.RedrawWindow(base.Handle, IntPtr.Zero, IntPtr.Zero, 1025);
		base.OnLostFocus(e);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			Focus();
			UpdateColor(new Point(e.X, e.Y));
			_dragInProgress = true;
		}
		base.OnMouseDown(e);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		if (_dragInProgress)
		{
			UpdateColor(new Point(e.X, e.Y));
		}
		base.OnMouseMove(e);
	}

	protected void UpdateColor(Point mousePos)
	{
		Rectangle clientRectangle = base.ClientRectangle;
		if (mousePos.X < 0)
		{
			mousePos.X = 0;
		}
		if (mousePos.Y < 0)
		{
			mousePos.Y = 0;
		}
		if (mousePos.X > clientRectangle.Right - 1)
		{
			mousePos.X = clientRectangle.Right - 1;
		}
		if (mousePos.Y > clientRectangle.Bottom - 1)
		{
			mousePos.Y = clientRectangle.Bottom - 1;
		}
		Rectangle clientRectangle2 = base.ClientRectangle;
		_saturation = (double)mousePos.X * 100.0 / (double)clientRectangle2.Width;
		_brightness = (double)(clientRectangle2.Height - mousePos.Y) * 100.0 / (double)clientRectangle2.Height;
		Color color = Color.FromArgb(_color.A, ColorUtils.HsbToRgb(_hue, _saturation, _brightness));
		Invalidate();
		if (color != _color)
		{
			_color = color;
			if (ValueChanged != null)
			{
				ValueChanged(this, EventArgs.Empty);
			}
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		if (_dragInProgress)
		{
			_dragInProgress = false;
		}
		base.OnMouseUp(e);
	}

	protected override bool ProcessDialogKey(Keys keyData)
	{
		switch (keyData & Keys.KeyCode)
		{
		case Keys.Down:
		{
			Rectangle clientRectangle4 = base.ClientRectangle;
			int num7 = (int)Math.Round(_saturation / 100.0 * (double)clientRectangle4.Width);
			int num8 = clientRectangle4.Height - (int)Math.Round(_brightness / 100.0 * (double)clientRectangle4.Height) + 1;
			UpdateColor(new Point(num7, num8));
			return true;
		}
		case Keys.Up:
		{
			Rectangle clientRectangle3 = base.ClientRectangle;
			int num5 = (int)Math.Round(_saturation / 100.0 * (double)clientRectangle3.Width);
			int num6 = clientRectangle3.Height - (int)Math.Round(_brightness / 100.0 * (double)clientRectangle3.Height) - 1;
			UpdateColor(new Point(num5, num6));
			return true;
		}
		case Keys.Right:
		{
			Rectangle clientRectangle2 = base.ClientRectangle;
			int num3 = (int)Math.Round(_saturation / 100.0 * (double)clientRectangle2.Width) + 1;
			int num4 = clientRectangle2.Height - (int)Math.Round(_brightness / 100.0 * (double)clientRectangle2.Height);
			UpdateColor(new Point(num3, num4));
			return true;
		}
		case Keys.Left:
		{
			Rectangle clientRectangle = base.ClientRectangle;
			int num = (int)Math.Round(_saturation / 100.0 * (double)clientRectangle.Width) - 1;
			int num2 = clientRectangle.Height - (int)Math.Round(_brightness / 100.0 * (double)clientRectangle.Height);
			UpdateColor(new Point(num, num2));
			return true;
		}
		default:
			return base.ProcessDialogKey(keyData);
		}
	}
}
