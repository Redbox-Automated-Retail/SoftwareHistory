using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

[ToolboxItem(false)]
internal class ToolTipCtrl : Control
{
	[StructLayout(LayoutKind.Sequential)]
	internal class INITCOMMONCONTROLSEX
	{
		public int dwSize;

		public int dwICC;

		public INITCOMMONCONTROLSEX()
		{
			dwSize = 8;
		}
	}

	private Control _parent;

	private string _title = "";

	private Win32Calls.TOOLINFO _toolinfo;

	private PropertyEnumerator _enumerator;

	private int _leftMargin;

	private int _topMargin;

	private int _rightMargin;

	private int _bottomMargin;

	protected override CreateParams CreateParams
	{
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		get
		{
			INITCOMMONCONTROLSEX iNITCOMMONCONTROLSEX = new INITCOMMONCONTROLSEX();
			iNITCOMMONCONTROLSEX.dwICC = 8;
			InitCommonControlsEx(iNITCOMMONCONTROLSEX);
			CreateParams createParams = new CreateParams();
			createParams.Parent = IntPtr.Zero;
			createParams.ClassName = "tooltips_class32";
			createParams.Style |= 3;
			createParams.ExStyle = 0;
			if (_parent.RightToLeft == RightToLeft.Yes)
			{
				createParams.ExStyle |= 4096;
				createParams.ExStyle |= 8192;
			}
			createParams.Caption = null;
			return createParams;
		}
	}

	[DllImport("comctl32.dll")]
	public static extern bool InitCommonControlsEx(INITCOMMONCONTROLSEX icc);

	public void SetText(string text, PropertyEnumerator enumerator)
	{
		if (_title != text || _enumerator != enumerator)
		{
			_title = "";
			Win32Calls.SendMessage(base.Handle, 1081, IntPtr.Zero, GetTOOLINFO());
			_title = text;
			Win32Calls.SendMessage(base.Handle, 1081, IntPtr.Zero, GetTOOLINFO());
		}
		_enumerator = enumerator;
	}

	public ToolTipCtrl(Control parent)
	{
		_parent = parent;
		SetStyle(ControlStyles.UserPaint, value: false);
		Font = _parent.Font;
		if (_parent.IsHandleCreated)
		{
			Win32Calls.SetWindowPos(base.Handle, (IntPtr)(-1), 0, 0, 0, 0, 19);
			Win32Calls.SendMessage(base.Handle, 1074, IntPtr.Zero, GetTOOLINFO());
			Win32Calls.SendMessage(base.Handle, 1048, IntPtr.Zero, (IntPtr)SystemInformation.MaxWindowTrackSize.Width);
		}
	}

	public Win32Calls.TOOLINFO GetTOOLINFO()
	{
		if (_toolinfo == null)
		{
			_toolinfo = new Win32Calls.TOOLINFO();
			_toolinfo.uFlags = 273;
		}
		_toolinfo.lpszText = _title;
		_toolinfo.hwnd = _parent.Handle;
		_toolinfo.uId = _parent.Handle;
		return _toolinfo;
	}

	public void SetMargins(int left, int top, int right, int bottom)
	{
		_leftMargin = left;
		_rightMargin = right;
		_topMargin = top;
		_bottomMargin = bottom;
		Win32Calls.RECT lParam = new Win32Calls.RECT(left, top, right, bottom);
		Win32Calls.SendMessage(base.Handle, 1050, IntPtr.Zero, ref lParam);
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	protected override void WndProc(ref Message msg)
	{
		if (msg.Msg == 132)
		{
			msg.Result = (IntPtr)(-1);
			return;
		}
		if (msg.Msg == 15)
		{
			Win32Calls.PAINTSTRUCT lpPaint = default(Win32Calls.PAINTSTRUCT);
			Graphics graphics = Graphics.FromHdc(Win32Calls.BeginPaint(new HandleRef(this, base.Handle), ref lpPaint));
			try
			{
				Rectangle rect = new Rectangle(lpPaint.rcPaint_left, lpPaint.rcPaint_top, lpPaint.rcPaint_right - lpPaint.rcPaint_left, lpPaint.rcPaint_bottom - lpPaint.rcPaint_top);
				graphics.FillRectangle(SystemBrushes.Info, rect);
				rect.X += _leftMargin + 2;
				rect.Width -= _leftMargin + _rightMargin + 2;
				rect.Y += _topMargin + 1;
				rect.Height -= _topMargin - 1;
				Win32Calls.DrawText(graphics, _title, ref rect, Font, SystemColors.InfoText, 8208);
				return;
			}
			finally
			{
				graphics.Dispose();
				Win32Calls.EndPaint(new HandleRef(this, base.Handle), ref lpPaint);
			}
		}
		base.WndProc(ref msg);
	}
}
