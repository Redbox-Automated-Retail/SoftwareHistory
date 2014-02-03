namespace WindowsInput;

public enum MouseFlag : uint
{
	MOVE = 1u,
	LEFTDOWN = 2u,
	LEFTUP = 4u,
	RIGHTDOWN = 8u,
	RIGHTUP = 0x10u,
	MIDDLEDOWN = 0x20u,
	MIDDLEUP = 0x40u,
	XDOWN = 0x80u,
	XUP = 0x100u,
	WHEEL = 0x800u,
	VIRTUALDESK = 0x4000u,
	ABSOLUTE = 0x8000u
}
