using System.Runtime.InteropServices;

namespace Redbox.DirectShow.Interop;

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("B196B28B-BAB4-101A-B69C-00AA00341D07")]
public interface ISpecifyPropertyPages
{
	[PreserveSig]
	int GetPages(out CAUUID pPages);
}
