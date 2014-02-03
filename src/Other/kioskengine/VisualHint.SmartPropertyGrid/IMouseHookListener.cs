using System;
using System.Drawing;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

[CLSCompliant(false)]
public interface IMouseHookListener
{
	bool OnListenMouseHook(Control control, uint message, Point pt, uint hitTest);
}
