using System;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

internal interface IMessageHookClient
{
	bool OnMessageHooked(Control control, uint message, IntPtr wParam, IntPtr lParam);
}
