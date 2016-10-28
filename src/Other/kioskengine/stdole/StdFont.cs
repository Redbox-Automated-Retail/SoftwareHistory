using System.Runtime.InteropServices;

namespace stdole;

[ComImport]
[Guid("BEF6E003-A874-101A-8BBA-00AA00300CAB")]
[CoClass(typeof(StdFontClass))]
[ComVisible(true)]
public interface StdFont : Font, FontEvents_Event
{
}
