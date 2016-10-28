using System;
using System.Runtime.InteropServices;

namespace Alsing.Windows;

[ComVisible(true)]
public struct WINDOWPOS
{
	public IntPtr hwnd;

	public IntPtr hwndInsertAfter;

	public int x;

	public int y;

	public int cx;

	public int cy;

	public uint flags;
}
