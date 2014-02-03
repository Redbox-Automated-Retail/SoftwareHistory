using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Alsing.Drawing;

namespace Alsing.Windows.Forms;

[ToolboxItem(true)]
public class BaseControl : Control
{
	private const int WS_BORDER = 8388608;

	private const int WS_EX_CLIENTEDGE = 512;

	private Color borderColor = Color.Black;

	private BorderStyle borderStyle;

	private Container components;

	private bool RunOnce = true;

	[Browsable(false)]
	public Size WindowSize
	{
		get
		{
			APIRect Rect = default(APIRect);
			NativeMethods.GetWindowRect(base.Handle, ref Rect);
			return new Size(Rect.Width, Rect.Height);
		}
	}

	[Category("Appearance - Borders")]
	[Description("The border color")]
	[DefaultValue(typeof(Color), "Black")]
	public Color BorderColor
	{
		get
		{
			return borderColor;
		}
		set
		{
			borderColor = value;
			Refresh();
			Invalidate();
			UpdateStyles();
		}
	}

	protected override CreateParams CreateParams
	{
		get
		{
			CreateParams createParams = base.CreateParams;
			if (BorderStyle == BorderStyle.None)
			{
				return createParams;
			}
			createParams.ExStyle &= -513;
			createParams.Style &= -8388609;
			return createParams;
		}
	}

	[Category("Appearance - Borders")]
	[EditorBrowsable(EditorBrowsableState.Always)]
	[Browsable(true)]
	[Description("The border style")]
	public BorderStyle BorderStyle
	{
		get
		{
			return borderStyle;
		}
		set
		{
			if (borderStyle != value)
			{
				if (!Enum.IsDefined(typeof(BorderStyle), value))
				{
					throw new InvalidEnumArgumentException("value", (int)value, typeof(BorderStyle));
				}
				borderStyle = value;
				UpdateStyles();
				Refresh();
			}
		}
	}

	[Obsolete("Do not use!", true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Browsable(false)]
	public override Image BackgroundImage
	{
		get
		{
			return base.BackgroundImage;
		}
		set
		{
			base.BackgroundImage = value;
		}
	}

	[Browsable(false)]
	public int ClientWidth => WindowSize.Width - BorderWidth * 2;

	[Browsable(false)]
	public int ClientHeight => WindowSize.Height - BorderWidth * 2;

	[Browsable(false)]
	public int BorderWidth => borderStyle switch
	{
		BorderStyle.None => 0, 
		BorderStyle.Sunken => 2, 
		BorderStyle.SunkenThin => 1, 
		BorderStyle.Raised => 2, 
		BorderStyle.Etched => 2, 
		BorderStyle.Bump => 6, 
		BorderStyle.FixedSingle => 1, 
		BorderStyle.FixedDouble => 2, 
		BorderStyle.RaisedThin => 1, 
		BorderStyle.Dotted => 1, 
		BorderStyle.Dashed => 1, 
		_ => base.Height, 
	};

	public event EventHandler Load;

	public BaseControl()
	{
		SetStyle(ControlStyles.EnableNotifyMessage, value: true);
		BorderStyle = BorderStyle.FixedSingle;
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		base.Size = new System.Drawing.Size(272, 264);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	protected virtual void OnLoad(EventArgs e)
	{
		if (Load != null)
		{
			Load(this, e);
		}
		Refresh();
	}

	protected unsafe override void WndProc(ref Message m)
	{
		if (m.Msg == 133)
		{
			RenderBorder();
		}
		else if (m.Msg == 24)
		{
			if (RunOnce)
			{
				RunOnce = false;
				OnLoad(null);
				base.WndProc(ref m);
				UpdateStyles();
			}
			else
			{
				UpdateStyles();
				base.WndProc(ref m);
			}
		}
		else if (m.Msg == 129)
		{
			base.WndProc(ref m);
		}
		else if (m.Msg == 131)
		{
			if (m.WParam == (IntPtr)0)
			{
				_ = (void*)m.LParam;
				base.WndProc(ref m);
			}
			else if (m.WParam == (IntPtr)1)
			{
				_NCCALCSIZE_PARAMS* ptr = (_NCCALCSIZE_PARAMS*)(void*)m.LParam;
				int top = ptr->NewRect.top + BorderWidth;
				int left = ptr->NewRect.left + BorderWidth;
				int bottom = ptr->NewRect.bottom - BorderWidth;
				int right = ptr->NewRect.right - BorderWidth;
				base.WndProc(ref m);
				ptr->NewRect.top = top;
				ptr->NewRect.left = left;
				ptr->NewRect.right = right;
				ptr->NewRect.bottom = bottom;
			}
		}
		else
		{
			base.WndProc(ref m);
		}
	}

	private void RenderBorder()
	{
		IntPtr windowDC = NativeMethods.GetWindowDC(base.Handle);
		APIRect Rect = default(APIRect);
		NativeMethods.GetWindowRect(base.Handle, ref Rect);
		using (Graphics g = Graphics.FromHdc(windowDC))
		{
			DrawingTools.DrawBorder((BorderStyle2)BorderStyle, BorderColor, g, new Rectangle(0, 0, Rect.Width, Rect.Height));
		}
		NativeMethods.ReleaseDC(base.Handle, windowDC);
	}

	protected override void OnEnter(EventArgs e)
	{
		base.OnEnter(e);
	}
}
