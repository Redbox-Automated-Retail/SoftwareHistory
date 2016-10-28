using System.Runtime.InteropServices;

namespace Alsing.Windows;

[ComVisible(true)]
public struct _NCCALCSIZE_PARAMS
{
	public APIRect NewRect;

	public APIRect OldRect;

	public APIRect OldClientRect;

	public WINDOWPOS lppos;
}
