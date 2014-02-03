using System;
using System.Windows.Forms;

namespace Alsing.Windows.Forms;

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
