using System.Runtime.InteropServices;

namespace Alsing.Windows;

[StructLayout(LayoutKind.Sequential)]
public class ENUMLOGFONTEX
{
	public LogFont elfLogFont;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
	public string elfFullName = "";

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
	public byte[] elfStyle;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
	public byte[] elfScript;
}
