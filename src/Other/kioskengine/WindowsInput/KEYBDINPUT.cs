using System;

namespace WindowsInput;

internal struct KEYBDINPUT
{
	public ushort Vk;

	public ushort Scan;

	public uint Flags;

	public uint Time;

	public IntPtr ExtraInfo;
}
