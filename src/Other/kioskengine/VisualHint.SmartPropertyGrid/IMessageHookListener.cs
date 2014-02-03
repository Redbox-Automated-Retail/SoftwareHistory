using System;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

[CLSCompliant(false)]
public interface IMessageHookListener
{
	bool OnListenMessageHook(Control control, uint message, IntPtr wParam, IntPtr lParam);
}
