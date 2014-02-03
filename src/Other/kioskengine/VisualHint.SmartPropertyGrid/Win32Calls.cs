using System;
using System.Collections;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

public class Win32Calls
{
	[StructLayout(LayoutKind.Sequential)]
	public class NMHDR
	{
		public IntPtr hwndFrom;

		public IntPtr idFrom;

		public int code;
	}

	public struct RECT(int left, int top, int right, int bottom)
	{
		public int Left = left;

		public int Top = top;

		public int Right = right;

		public int Bottom = bottom;
	}

	public struct SIZE
	{
		public int Width;

		public int Height;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public struct TEXTMETRIC
	{
		public int tmHeight;

		public int tmAscent;

		public int tmDescent;

		public int tmInternalLeading;

		public int tmExternalLeading;

		public int tmAveCharWidth;

		public int tmMaxCharWidth;

		public int tmWeight;

		public int tmOverhang;

		public int tmDigitizedAspectX;

		public int tmDigitizedAspectY;

		public char tmFirstChar;

		public char tmLastChar;

		public char tmDefaultChar;

		public char tmBreakChar;

		public byte tmItalic;

		public byte tmUnderlined;

		public byte tmStruckOut;

		public byte tmPitchAndFamily;

		public byte tmCharSet;
	}

	public struct NCCALCSIZE_PARAMS
	{
		public RECT rgrc0;

		public RECT rgrc1;

		public RECT rgrc2;

		public IntPtr lppos;
	}

	public struct PAINTSTRUCT
	{
		public IntPtr hdc;

		public bool fErase;

		public int rcPaint_left;

		public int rcPaint_top;

		public int rcPaint_right;

		public int rcPaint_bottom;

		public bool fRestore;

		public bool fIncUpdate;

		public int reserved1;

		public int reserved2;

		public int reserved3;

		public int reserved4;

		public int reserved5;

		public int reserved6;

		public int reserved7;

		public int reserved8;
	}

	public struct ColorRef(Color color)
	{
		public byte red = color.R;

		public byte green = color.G;

		public byte blue = color.B;

		private byte unused = 0;
	}

	public struct ABCFLOAT
	{
		public float abcfA;

		public float abcfB;

		public float abcfC;
	}

	public delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

	[CLSCompliant(false)]
	public struct CWPSTRUCT
	{
		public IntPtr lParam;

		public IntPtr wParam;

		public uint message;

		public IntPtr hwnd;
	}

	[CLSCompliant(false)]
	public struct CMHSTRUCT
	{
		public Point pt;

		public IntPtr hwnd;

		public uint wHitTestCode;

		public IntPtr dwExtraInfo;
	}

	public struct POINT(int x, int y)
	{
		public int x = x;

		public int y = y;
	}

	public struct SCROLLINFO
	{
		public int cbSize;

		public int fMask;

		public int nMin;

		public int nMax;

		public int nPage;

		public int nPos;

		public int nTrackPos;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public class TOOLINFO
	{
		public int cbSize;

		public int uFlags;

		public IntPtr hwnd;

		public IntPtr uId;

		public RECT rect;

		public IntPtr hinst;

		public string lpszText;

		public IntPtr lParam;

		public TOOLINFO()
		{
			cbSize = Marshal.SizeOf(typeof(TOOLINFO));
			uFlags = 0;
			hinst = IntPtr.Zero;
			lParam = IntPtr.Zero;
		}
	}

	[Serializable]
	public struct MSG
	{
		public IntPtr hwnd;

		public int message;

		public IntPtr wParam;

		public IntPtr lParam;

		public int time;

		public int pt_x;

		public int pt_y;
	}

	public const int WM_ACTIVATE = 6;

	public const int WM_SETFOCUS = 7;

	public const int WM_KILLFOCUS = 8;

	public const int WM_SETREDRAW = 11;

	public const int WM_PAINT = 15;

	public const int WM_MOUSEACTIVATE = 33;

	public const int WM_NCCALCSIZE = 131;

	public const int WM_NCPAINT = 133;

	public const int WM_ERASEBKGND = 20;

	public const int WM_NOTIFY = 78;

	public const int WM_NCHITTEST = 132;

	public const int WM_NCLBUTTONDOWN = 161;

	public const int EM_SETMARGINS = 211;

	public const int EM_GETMARGINS = 212;

	public const int WM_KEYDOWN = 256;

	public const int WM_SYSKEYDOWN = 260;

	public const int WM_KEYUP = 257;

	public const int WM_CHAR = 258;

	public const int WM_VSCROLL = 277;

	public const int WM_MOUSEMOVE = 512;

	public const int WM_LBUTTONDOWN = 513;

	public const int WM_LBUTTONDBLCLK = 515;

	public const int WM_LBUTTONUP = 514;

	public const int WM_RBUTTONDOWN = 516;

	public const int WM_MBUTTONDOWN = 519;

	public const int WM_MOUSEWHEEL = 522;

	public const int WM_CUT = 768;

	public const int WM_COPY = 769;

	public const int WM_PASTE = 770;

	public const int WM_USER = 1024;

	public const int TTM_SETMAXTIPWIDTH = 1048;

	public const int TTM_SETDELAYTIME = 1027;

	public const int TTM_ADDTOOL = 1074;

	public const int TTM_UPDATETIPTEXT = 1081;

	public const int TTM_TRACKACTIVATE = 1041;

	public const int TTM_TRACKPOSITION = 1042;

	public const int TTM_SETMARGIN = 1050;

	public const int TTM_SETTITLE = 1056;

	public const int TTDT_AUTOMATIC = 0;

	public const int TTDT_RESHOW = 1;

	public const int TTDT_AUTOPOP = 2;

	public const int TTDT_INITIAL = 3;

	public const int TTF_IDISHWND = 1;

	public const int TTF_SUBCLASS = 16;

	public const int TTF_TRACK = 32;

	public const int TTF_ABSOLUTE = 128;

	public const int TTF_TRANSPARENT = 256;

	public const int TTN_SHOW = -521;

	public const int EC_LEFTMARGIN = 1;

	public const int EC_RIGHTMARGIN = 2;

	public const int EC_USEFONTINFO = 65535;

	public const int GWL_WNDPROC = -4;

	public const int GWL_HINSTANCE = -6;

	public const int GWL_HWNDPARENT = -8;

	public const int GWL_STYLE = -16;

	public const int GWL_EXSTYLE = -20;

	public const int GWL_USERDATA = -21;

	public const int GWL_ID = -12;

	public const int WS_POPUP = int.MinValue;

	public const int WS_BORDER = 8388608;

	public const int WS_VSCROLL = 2097152;

	public const int WS_HSCROLL = 1048576;

	public const int WS_CHILD = 1073741824;

	public const int WS_EX_TOOLWINDOW = 128;

	public const int WS_EX_TOPMOST = 8;

	public const int WS_EX_CLIENTEDGE = 512;

	public const int WS_EX_RIGHT = 4096;

	public const int WS_EX_RTLREADING = 8192;

	public const int SW_SHOWNA = 8;

	public const int CS_DROPSHADOW = 131072;

	public const int RDW_INVALIDATE = 1;

	public const int RDW_INTERNALPAINT = 2;

	public const int RDW_ERASE = 4;

	public const int RDW_VALIDATE = 8;

	public const int RDW_NOINTERNALPAINT = 16;

	public const int RDW_NOERASE = 32;

	public const int RDW_NOCHILDREN = 64;

	public const int RDW_ALLCHILDREN = 128;

	public const int RDW_UPDATENOW = 256;

	public const int RDW_ERASENOW = 512;

	public const int RDW_FRAME = 1024;

	public const int RDW_NOFRAME = 2048;

	[CLSCompliant(false)]
	public const uint MOUSEEVENTF_MOVE = 1u;

	[CLSCompliant(false)]
	public const uint MOUSEEVENTF_LEFTDOWN = 2u;

	[CLSCompliant(false)]
	public const uint MOUSEEVENTF_LEFTUP = 4u;

	[CLSCompliant(false)]
	public const uint MOUSEEVENTF_RIGHTDOWN = 8u;

	[CLSCompliant(false)]
	public const uint MOUSEEVENTF_RIGHTUP = 16u;

	[CLSCompliant(false)]
	public const uint MOUSEEVENTF_MIDDLEDOWN = 32u;

	[CLSCompliant(false)]
	public const uint MOUSEEVENTF_MIDDLEUP = 64u;

	[CLSCompliant(false)]
	public const uint MOUSEEVENTF_ABSOLUTE = 32768u;

	public const byte VK_RETURN = 13;

	public const byte VK_MENU = 18;

	public const byte VK_UP = 38;

	public const byte VK_DOWN = 40;

	public const byte VK_DELETE = 46;

	public const byte VK_F4 = 115;

	public const int DT_LEFT = 0;

	public const int DT_TOP = 0;

	public const int DT_CENTER = 1;

	public const int DT_RIGHT = 2;

	public const int DT_VCENTER = 4;

	public const int DT_WORDBREAK = 16;

	public const int DT_SINGLELINE = 32;

	public const int DT_CALCRECT = 1024;

	public const int DT_NOPREFIX = 2048;

	public const int DT_EDITCONTROL = 8192;

	public const int DT_END_ELLIPSIS = 32768;

	public const int DT_RTLREADING = 131072;

	public const int DT_HIDEPREFIX = 1048576;

	public const int TRANSPARENT = 1;

	public const int WH_KEYBOARD = 2;

	public const int WH_CALLWNDPROC = 4;

	public const int WH_MOUSEPROC = 7;

	public const int SB_BOTH = 3;

	public const int SB_BOTTOM = 7;

	public const int SB_CTL = 2;

	public const int SB_ENDSCROLL = 8;

	public const int SB_HORZ = 0;

	public const int SB_LEFT = 6;

	public const int SB_LINEDOWN = 1;

	public const int SB_LINELEFT = 0;

	public const int SB_LINERIGHT = 1;

	public const int SB_LINEUP = 0;

	public const int SB_PAGEDOWN = 3;

	public const int SB_PAGELEFT = 2;

	public const int SB_PAGERIGHT = 3;

	public const int SB_PAGEUP = 2;

	public const int SB_RIGHT = 7;

	public const int SB_THUMBPOSITION = 4;

	public const int SB_THUMBTRACK = 5;

	public const int SB_TOP = 6;

	public const int SB_VERT = 1;

	public const int SIF_RANGE = 1;

	public const int SIF_PAGE = 2;

	public const int SIF_POS = 4;

	public const int SIF_DISABLENOSCROLL = 8;

	public const int SIF_TRACKPOS = 16;

	public const int SIF_ALL = 23;

	public const int ESB_ENABLE_BOTH = 0;

	public const int ESB_DISABLE_BOTH = 3;

	public const int ESB_DISABLE_DOWN = 2;

	public const int ESB_DISABLE_UP = 1;

	public const int SBS_SIZEGRIP = 16;

	public const int SWP_NOSIZE = 1;

	public const int SWP_NOMOVE = 2;

	public const int SWP_NOZORDER = 4;

	public const int SWP_NOREDRAW = 8;

	public const int SWP_NOACTIVATE = 16;

	public const int SWP_FRAMECHANGED = 32;

	public const int SWP_SHOWWINDOW = 64;

	public const int SWP_HIDEWINDOW = 128;

	public const int SWP_NOCOPYBITS = 256;

	public const int SWP_NOOWNERZORDER = 512;

	public const int SWP_NOSENDCHANGING = 1024;

	public const int SWP_DRAWFRAME = 32;

	public const int SWP_NOREPOSITION = 512;

	private Hashtable _fonts = new Hashtable();

	private static Win32Calls _instance;

	private bool disposed;

	private static IntPtr _currentClipRegion;

	[DllImport("User32.dll")]
	public static extern bool GetUpdateRect(IntPtr hWnd, out RECT rect, bool bErase);

	[DllImport("User32.dll")]
	public static extern int GetWindowThreadProcessId(HandleRef hWnd, out int lpdwProcessId);

	[DllImport("User32.dll")]
	public static extern int MsgWaitForMultipleObjects(int nCount, IntPtr pHandles, bool fWaitAll, int dwMilliseconds, int dwWakeMask);

	[DllImport("User32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern void WaitMessage();

	[DllImport("User32.dll")]
	public static extern bool ShowWindow(HandleRef hWnd, int nCmdShow);

	[DllImport("User32.dll")]
	public static extern int RedrawWindow(IntPtr hwnd, IntPtr rect, IntPtr hrgnUpdate, int fuRedraw);

	[DllImport("User32.dll")]
	public static extern IntPtr GetWindowDC(IntPtr hWnd);

	[DllImport("User32.dll")]
	public static extern IntPtr SetFocus(IntPtr hWnd);

	[DllImport("User32.dll")]
	public static extern IntPtr GetForegroundWindow();

	[DllImport("User32.dll")]
	public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

	[DllImport("User32.dll")]
	public static extern int GetWindowRect(IntPtr hwnd, ref RECT rc);

	public static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
	{
		if (IntPtr.Size == 8)
		{
			return GetWindowLongPtr64(hWnd, nIndex);
		}
		return new IntPtr(GetWindowLong32(hWnd, nIndex));
	}

	[DllImport("User32.dll", EntryPoint = "GetWindowLong")]
	public static extern int GetWindowLong32(IntPtr hWnd, int nIndex);

	[DllImport("User32.dll", EntryPoint = "GetWindowLongPtr")]
	private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

	public static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
	{
		if (IntPtr.Size == 8)
		{
			return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
		}
		return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
	}

	[DllImport("User32.dll", EntryPoint = "SetWindowLong")]
	private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

	[DllImport("User32.dll", EntryPoint = "SetWindowLongPtr")]
	private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

	[DllImport("User32.dll", CharSet = CharSet.Auto)]
	public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

	[DllImport("User32.dll")]
	public static extern IntPtr GetWindow(IntPtr hWnd, int uCmd);

	[DllImport("User32.dll")]
	public static extern bool IsChild(IntPtr hWndParent, IntPtr hwnd);

	[DllImport("User32.dll")]
	public static extern IntPtr WindowFromPoint(POINT Point);

	[DllImport("kernel32.dll")]
	public static extern int GetCurrentThreadId();

	[DllImport("User32.dll")]
	[CLSCompliant(false)]
	public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, IntPtr dwExtraInfo);

	[DllImport("User32.dll", CharSet = CharSet.Auto)]
	[CLSCompliant(false)]
	public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

	[DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "DrawText")]
	public static extern int DrawTextA(IntPtr hdc, string lpStr, int nCount, ref RECT lpRect, int wFormat);

	protected virtual void Dispose(bool disposing)
	{
		if (disposed)
		{
			return;
		}
		foreach (IntPtr value in _fonts.Values)
		{
			DeleteObject(value);
		}
		disposed = true;
		if (disposing)
		{
			GC.SuppressFinalize(this);
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	~Win32Calls()
	{
		Dispose(disposing: false);
	}

	static Win32Calls()
	{
		_instance = null;
		_instance = new Win32Calls();
	}

	[DllImport("User32.dll")]
	public static extern IntPtr BeginPaint(HandleRef hWnd, [In][Out] ref PAINTSTRUCT lpPaint);

	[DllImport("User32.dll")]
	public static extern bool EndPaint(HandleRef hWnd, ref PAINTSTRUCT lpPaint);

	public static IntPtr GetHFont(Font font)
	{
		object obj = _instance._fonts[font];
		if (obj == null)
		{
			obj = font.ToHfont();
			_instance._fonts.Add(font, obj);
		}
		return (IntPtr)obj;
	}

	public static int DrawText(Graphics graphics, string text, ref Rectangle rect, Font font, Color color, int format)
	{
		if (text == null)
		{
			text = "";
		}
		RECT lpRect = new RECT
		{
			Left = rect.Left,
			Right = rect.Right,
			Top = rect.Top,
			Bottom = rect.Bottom
		};
		IntPtr hdc = graphics.GetHdc();
		IntPtr hFont = GetHFont(font);
		IntPtr hgdiObj = SelectObject(hdc, hFont);
		ColorRef color2 = SetTextColor(hdc, new ColorRef(color));
		SetBkMode(hdc, 1);
		int result = DrawTextA(hdc, text, text.Length, ref lpRect, format);
		if ((format & 0x400) != 0)
		{
			rect.X = lpRect.Left;
			rect.Y = lpRect.Top;
			rect.Width = lpRect.Right - lpRect.Left;
			rect.Height = lpRect.Bottom - lpRect.Top;
		}
		SetTextColor(hdc, color2);
		SelectObject(hdc, hgdiObj);
		graphics.ReleaseHdc(hdc);
		return result;
	}

	[DllImport("gdi32.dll")]
	public static extern ColorRef SetTextColor(IntPtr hDC, ColorRef color);

	[DllImport("gdi32.dll")]
	public static extern int SetBkMode(IntPtr hDC, int mode);

	[DllImport("gdi32.dll")]
	public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiObj);

	[DllImport("gdi32.dll", CharSet = CharSet.Auto, EntryPoint = "GetTextExtentPoint32")]
	public static extern int GetTextExtentPoint32A(IntPtr hdc, string lpStr, int nCount, ref SIZE lpSize);

	public static Size GetTextExtent(Graphics graphics, string text, Font font)
	{
		if (text == null)
		{
			text = "";
		}
		IntPtr hdc = graphics.GetHdc();
		IntPtr hFont = GetHFont(font);
		IntPtr hgdiObj = SelectObject(hdc, hFont);
		SIZE lpSize = default(SIZE);
		GetTextExtentPoint32A(hdc, text, text.Length, ref lpSize);
		int num = lpSize.Width;
		if ((font.Bold || font.Italic) && text.Length > 0)
		{
			ABCFLOAT[] array = new ABCFLOAT[1];
			uint num2 = text[text.Length - 1];
			GetCharABCWidthsFloat(hdc, num2, num2, array);
			double num3 = array[0].abcfC;
			if (num3 < 0.0)
			{
				num -= (int)num3;
			}
		}
		SelectObject(hdc, hgdiObj);
		graphics.ReleaseHdc(hdc);
		return new Size(num, lpSize.Height);
	}

	[DllImport("gdi32.dll")]
	[CLSCompliant(false)]
	public static extern bool GetCharABCWidthsFloat(IntPtr hdc, uint iFirstChar, uint iLastChar, [Out] ABCFLOAT[] lpABCF);

	[DllImport("gdi32.dll")]
	public static extern int DeleteObject(IntPtr hObject);

	[DllImport("gdi32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetTextMetrics")]
	public static extern int GetTextMetricsA(IntPtr hDC, ref TEXTMETRIC tm);

	public static void GetTextMetrics(Graphics graphics, Font font, ref TEXTMETRIC tm)
	{
		IntPtr hdc = graphics.GetHdc();
		IntPtr hFont = GetHFont(font);
		IntPtr hgdiObj = SelectObject(hdc, hFont);
		GetTextMetricsA(hdc, ref tm);
		SelectObject(hdc, hgdiObj);
		graphics.ReleaseHdc(hdc);
	}

	[DllImport("gdi32.dll")]
	public static extern IntPtr CreateRectRgnIndirect(ref RECT lprc);

	[DllImport("gdi32.dll")]
	public static extern int SelectClipRgn(IntPtr hdc, IntPtr hrgn);

	public static void SetClippingRect(Graphics graphics, Rectangle rect)
	{
		RECT lprc = new RECT(rect.Left, rect.Top, rect.Right, rect.Bottom);
		_currentClipRegion = CreateRectRgnIndirect(ref lprc);
		IntPtr hdc = graphics.GetHdc();
		SelectClipRgn(hdc, _currentClipRegion);
		graphics.ReleaseHdc(hdc);
		graphics.SetClip(rect);
	}

	public static void ResetClippingRect(Graphics graphics)
	{
		IntPtr hdc = graphics.GetHdc();
		SelectClipRgn(hdc, IntPtr.Zero);
		DeleteObject(_currentClipRegion);
		graphics.ReleaseHdc(hdc);
		graphics.ResetClip();
	}

	[DllImport("User32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern int MapWindowPoints(IntPtr hwndFrom, IntPtr hwndTo, ref POINT lpPoints, int cPoints);

	[DllImport("User32.dll", CharSet = CharSet.Auto)]
	public static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

	[DllImport("User32.dll", CharSet = CharSet.Auto)]
	public static extern bool UnhookWindowsHookEx(HandleRef hhook);

	[DllImport("User32.dll", CharSet = CharSet.Auto)]
	public static extern IntPtr CallNextHookEx(HandleRef hhook, int code, IntPtr wparam, IntPtr lparam);

	[DllImport("User32.dll")]
	public static extern int ShowScrollBar(IntPtr hWnd, int wBar, bool bShow);

	[DllImport("User32.dll")]
	public static extern int EnableScrollBar(IntPtr hWnd, int wBar, int wArrows);

	[DllImport("User32.dll")]
	public static extern int SetScrollInfo(IntPtr hWnd, int wBar, ref SCROLLINFO lpsi, bool fRedraw);

	[DllImport("User32.dll")]
	public static extern int GetScrollInfo(IntPtr hwnd, int n, ref SCROLLINFO lpScrollInfo);

	[DllImport("User32.dll")]
	public static extern int GetScrollPos(IntPtr hWnd, int nBar);

	[DllImport("User32.dll")]
	public static extern int GetScrollRange(IntPtr hWnd, int nBar, ref int lpMinPos, ref int lpMaxPos);

	[DllImport("User32.dll")]
	public static extern int SetScrollPos(IntPtr hWnd, int nBar, int nPos, bool bRedraw);

	[DllImport("User32.dll")]
	public static extern int SetScrollRange(IntPtr hWnd, int nBar, int nMinPos, int nMaxPos, int bRedraw);

	[DllImport("User32.dll", CharSet = CharSet.Auto)]
	public static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

	[DllImport("User32.dll", CharSet = CharSet.Auto)]
	public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, TOOLINFO lParam);

	[DllImport("User32.dll", CharSet = CharSet.Auto)]
	public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, ref RECT lParam);

	[DllImport("User32.dll", CharSet = CharSet.Auto)]
	public static extern int PostMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

	[DllImport("User32.dll", CharSet = CharSet.Auto)]
	public static extern bool PeekMessage(out MSG msg, HandleRef hwnd, int msgMin, int msgMax, int remove);

	public static void RemovePendingMessages(HandleRef handle, int msgMin, int msgMax)
	{
		MSG msg = default(MSG);
		while (PeekMessage(out msg, handle, msgMin, msgMax, 1))
		{
		}
	}

	public static bool ModifyStyleEx(IntPtr hWnd, int remove, int add, int flags)
	{
		int windowLong = GetWindowLong32(hWnd, -20);
		int num = (windowLong & ~remove) | add;
		if (windowLong == num)
		{
			return false;
		}
		SetWindowLongPtr(hWnd, -20, (IntPtr)num);
		if (flags != 0)
		{
			SetWindowPos(hWnd, IntPtr.Zero, 0, 0, 0, 0, 0x17 | flags);
		}
		return true;
	}

	public static int MakeLong(int LoWord, int HiWord)
	{
		return (HiWord << 16) | (LoWord & 0xFFFF);
	}

	public static IntPtr MakeLParam(int LoWord, int HiWord)
	{
		return (IntPtr)((HiWord << 16) | (LoWord & 0xFFFF));
	}

	public static int HiWord(int doubleword)
	{
		return (short)((doubleword >> 16) & 0xFFFF);
	}

	public static int LoWord(IntPtr n)
	{
		return LoWord((int)(long)n);
	}

	public static int LoWord(int doubleword)
	{
		return (short)(doubleword & 0xFFFF);
	}

	[DllImport("User32.dll")]
	public static extern short GetKeyState(int virtualKeyCode);

	[DllImport("User32.dll")]
	public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int flags);

	public static void ImageListDraw(ImageList il, int index, Graphics graphics, int x, int y, int dx, int dy)
	{
		il.Draw(graphics, x, y, dx, dy, index);
	}
}
