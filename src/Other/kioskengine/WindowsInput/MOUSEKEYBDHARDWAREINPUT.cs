using System.Runtime.InteropServices;

namespace WindowsInput;

[StructLayout(LayoutKind.Explicit)]
internal struct MOUSEKEYBDHARDWAREINPUT
{
	[FieldOffset(0)]
	public MOUSEINPUT Mouse;

	[FieldOffset(0)]
	public KEYBDINPUT Keyboard;

	[FieldOffset(0)]
	public HARDWAREINPUT Hardware;
}
