using System;
using System.Runtime.InteropServices;

namespace Alsing.Windows.Forms.IntelliMouse;

[ComVisible(true)]
public class ScrollEventArgs : EventArgs
{
	public int DeltaX;

	public int DeltaY;
}
