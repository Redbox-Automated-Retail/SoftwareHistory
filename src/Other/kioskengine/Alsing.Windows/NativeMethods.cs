using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Alsing.Windows;

[ComVisible(true)]
public static class NativeMethods
{
	public const int GWL_STYLE = -16;

	public const int WS_CHILD = 1073741824;

	[DllImport("uxtheme.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, SetLastError = true)]
	public static extern IntPtr OpenThemeData(IntPtr hwnd, [MarshalAs(UnmanagedType.LPWStr)] string pszClassList);

	[DllImport("uxtheme.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern IntPtr CloseThemeData(IntPtr hTheme);

	[DllImport("uxtheme.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern bool IsThemeActive();

	[DllImport("uxtheme.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern int DrawThemeBackground(IntPtr hTheme, IntPtr hdc, int iPartId, int iStateId, ref APIRect rect, ref APIRect clipRect);

	[DllImport("uxtheme.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern int DrawThemeText(IntPtr hTheme, IntPtr hdc, int iPartId, int iStateId, string pszText, int iCharCount, uint dwTextFlags, uint dwTextFlags2, [MarshalAs(UnmanagedType.Struct)] ref APIRect rect);

	[DllImport("uxtheme.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern int GetThemeColor(IntPtr hTheme, int iPartId, int iStateId, int iPropId, out ulong color);

	[DllImport("imm32.dll")]
	public static extern IntPtr ImmGetDefaultIMEWnd(IntPtr hWnd);

	[DllImport("User32.dll")]
	public static extern int SendMessage(IntPtr hWnd, int msg, int wParam, COMPOSITIONFORM lParam);

	[DllImport("User32.dll")]
	public static extern int SendMessage(IntPtr hWnd, int msg, int wParam, LogFont lParam);

	[DllImport("User32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern int DrawText(IntPtr hDC, string lpString, int nCount, ref APIRect Rect, int wFormat);

	[DllImport("gdi32", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, SetLastError = true)]
	public static extern int EnumFontFamiliesEx(IntPtr hDC, [MarshalAs(UnmanagedType.LPStruct)] LogFont lf, FONTENUMPROC proc, long LParam, long DW);

	[DllImport("shlwapi.dll", SetLastError = true)]
	public static extern int SHAutoComplete(IntPtr hWnd, uint flags);

	[DllImport("User32.dll")]
	public static extern IntPtr GetDesktopWindow();

	[DllImport("User32.dll")]
	public static extern IntPtr GetWindowRect(IntPtr hWND, ref APIRect Rect);

	[DllImport("User32.dll")]
	public static extern int SendMessage(IntPtr hWND, int message, int WParam, int LParam);

	[DllImport("User32.dll")]
	public static extern bool ReleaseCapture();

	[DllImport("User32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

	[DllImport("gdi32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern int SetBkColor(IntPtr hDC, int crColor);

	[DllImport("gdi32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern int SetBkMode(IntPtr hDC, int Mode);

	[DllImport("User32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern IntPtr ReleaseDC(IntPtr hWND, IntPtr hDC);

	[DllImport("gdi32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern IntPtr DeleteDC(IntPtr hDC);

	[DllImport("gdi32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern IntPtr GdiFlush();

	[DllImport("User32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern IntPtr GetWindowDC(IntPtr hWND);

	[DllImport("User32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern IntPtr GetDC(IntPtr hWND);

	[DllImport("gdi32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight);

	[DllImport("gdi32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

	[DllImport("gdi32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

	[DllImport("gdi32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern IntPtr DeleteObject(IntPtr hObject);

	[DllImport("gdi32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern int GetTextColor(IntPtr hDC);

	[DllImport("gdi32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern int SetTextColor(IntPtr hDC, int crColor);

	[DllImport("gdi32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern int GetBkColor(IntPtr hDC);

	[DllImport("gdi32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern int GetBkMode(IntPtr hDC);

	[DllImport("user32", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern int DrawFocusRect(IntPtr hDC, ref APIRect rect);

	[DllImport("gdi32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern IntPtr CreateSolidBrush(int crColor);

	[DllImport("gdi32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern int Rectangle(IntPtr hDC, int left, int top, int right, int bottom);

	[DllImport("gdi32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern IntPtr CreateHatchBrush(int Style, int crColor);

	[DllImport("User32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern int TabbedTextOut(IntPtr hDC, int x, int y, string lpString, int nCount, int nTabPositions, ref int lpnTabStopPositions, int nTabOrigin);

	[DllImport("gdi32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern IntPtr BitBlt(IntPtr hDestDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);

	[DllImport("User32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern int FillRect(IntPtr hDC, ref APIRect rect, IntPtr hBrush);

	[DllImport("gdi32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern int GetTextFace(IntPtr hDC, int nCount, string lpFacename);

	[DllImport("gdi32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern int GetTextMetrics(IntPtr hDC, ref GDITextMetric TextMetric);

	[DllImport("gdi32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	public static extern IntPtr CreateFontIndirect([MarshalAs(UnmanagedType.LPStruct)] LogFont LogFont);

	[DllImport("User32.dll", CharSet = CharSet.Auto)]
	public static extern int GetTabbedTextExtent(IntPtr hDC, string lpString, int nCount, int nTabPositions, ref int lpnTabStopPositions);

	[DllImport("User32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern int InvertRect(IntPtr hDC, ref APIRect rect);

	[DllImport("gdi32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern IntPtr CreatePen(int nPenStyle, int nWidth, int crColor);

	[DllImport("gdi32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern int SetBrushOrgEx(IntPtr hDC, int x, int y, ref APIPoint p);

	[DllImport("gdi32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern IntPtr CreatePatternBrush(IntPtr hBMP);

	[DllImport("User32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern int ShowWindow(IntPtr hWnd, short cmdShow);

	[DllImport("gdi32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern IntPtr MoveToEx(IntPtr hDC, int x, int y, ref APIPoint lpPoint);

	[DllImport("gdi32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern IntPtr LineTo(IntPtr hDC, int x, int y);

	[DllImport("user32", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern ushort GetAsyncKeyState(int vKey);

	public static bool IsKeyPressed(Keys k)
	{
		int asyncKeyState = GetAsyncKeyState((int)k);
		asyncKeyState = (asyncKeyState & 0x8000) >> 15;
		return asyncKeyState == 1;
	}

	public static IntPtr ControlDC(Control control)
	{
		return GetDC(control.Handle);
	}

	public static int ColorToInt(Color color)
	{
		return (color.B << 16) | (color.G << 8) | color.R;
	}

	public static Color IntToColor(int color)
	{
		int blue = (color >> 16) & 0xFF;
		int green = (color >> 8) & 0xFF;
		int red = color & 0xFF;
		return Color.FromArgb(red, green, blue);
	}
}
