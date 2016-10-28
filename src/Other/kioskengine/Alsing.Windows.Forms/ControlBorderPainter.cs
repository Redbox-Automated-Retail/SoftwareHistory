using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Alsing.Windows.Forms;

[ComVisible(true)]
public class ControlBorderPainter : NativeWindow
{
	public ControlBorderPainter(IntPtr Handle)
	{
		AssignHandle(Handle);
	}

	protected override void WndProc(ref Message m)
	{
	}
}
