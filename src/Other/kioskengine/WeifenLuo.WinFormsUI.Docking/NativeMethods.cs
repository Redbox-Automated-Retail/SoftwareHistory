using System;
using System.Drawing;
using System.Runtime.InteropServices;
using WeifenLuo.WinFormsUI.Docking.Win32;

namespace WeifenLuo.WinFormsUI.Docking;

internal static class NativeMethods
{
	public delegate IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam);

	[DllImport("User32.dll", CharSet = CharSet.Auto)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool DragDetect(IntPtr hWnd, Point pt);

	[DllImport("User32.dll", CharSet = CharSet.Auto)]
	public static extern IntPtr GetFocus();

	[DllImport("User32.dll", CharSet = CharSet.Auto)]
	public static extern IntPtr SetFocus(IntPtr hWnd);

	[DllImport("User32.dll", CharSet = CharSet.Auto)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool PostMessage(IntPtr hWnd, int Msg, uint wParam, uint lParam);

	[DllImport("User32.dll", CharSet = CharSet.Auto)]
	public static extern uint SendMessage(IntPtr hWnd, int Msg, uint wParam, uint lParam);

	[DllImport("User32.dll", CharSet = CharSet.Auto)]
	public static extern int ShowWindow(IntPtr hWnd, short cmdShow);

	[DllImport("User32.dll", CharSet = CharSet.Auto)]
	public static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndAfter, int X, int Y, int Width, int Height, FlagsSetWindowPos flags);

	[DllImport("User32.dll", CharSet = CharSet.Auto)]
	public static extern int GetWindowLong(IntPtr hWnd, int Index);

	[DllImport("User32.dll", CharSet = CharSet.Auto)]
	public static extern int SetWindowLong(IntPtr hWnd, int Index, int Value);

	[DllImport("User32.dll", CharSet = CharSet.Auto)]
	public static extern int ShowScrollBar(IntPtr hWnd, int wBar, int bShow);

	[DllImport("User32.dll", CharSet = CharSet.Auto)]
	public static extern IntPtr WindowFromPoint(Point point);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
	public static extern int GetCurrentThreadId();

	[DllImport("User32.dll")]
	public static extern IntPtr SetWindowsHookEx(HookType code, HookProc func, IntPtr hInstance, int threadID);

	[DllImport("User32.dll")]
	public static extern int UnhookWindowsHookEx(IntPtr hhook);

	[DllImport("User32.dll")]
	public static extern IntPtr CallNextHookEx(IntPtr hhook, int code, IntPtr wParam, IntPtr lParam);
}
