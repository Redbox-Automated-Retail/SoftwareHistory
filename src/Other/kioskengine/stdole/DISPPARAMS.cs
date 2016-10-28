using System;
using System.Runtime.InteropServices;

namespace stdole;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
[ComConversionLoss]
[TypeLibType(512)]
[ComVisible(true)]
public struct DISPPARAMS
{
	[ComConversionLoss]
	public IntPtr rgvarg;

	[ComConversionLoss]
	public IntPtr rgdispidNamedArgs;

	public uint cArgs;

	public uint cNamedArgs;
}
