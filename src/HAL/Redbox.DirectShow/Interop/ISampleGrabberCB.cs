using System;
using System.Runtime.InteropServices;

namespace Redbox.DirectShow.Interop;

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("0579154A-2B53-4994-B0D0-E773148EFF85")]
public interface ISampleGrabberCB
{
	[PreserveSig]
	int SampleCB(double sampleTime, IntPtr sample);

	[PreserveSig]
	int BufferCB(double sampleTime, IntPtr buffer, int bufferLen);
}
