using System;

namespace WindowsInput;

internal struct MOUSEINPUT
{
	public int X;

	public int Y;

	public uint MouseData;

	public uint Flags;

	public uint Time;

	public IntPtr ExtraInfo;
}
