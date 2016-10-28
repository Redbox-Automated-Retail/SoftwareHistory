using System.Runtime.InteropServices;

namespace Alsing.Windows;

[StructLayout(LayoutKind.Sequential)]
[ComVisible(true)]
public class COMPOSITIONFORM
{
	public int dwStyle;

	public APIPoint ptCurrentPos;

	public APIRect rcArea;
}
