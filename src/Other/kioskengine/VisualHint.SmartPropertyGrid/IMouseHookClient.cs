using System;
using System.Drawing;

namespace VisualHint.SmartPropertyGrid;

internal interface IMouseHookClient
{
	bool OnMouseHooked(IntPtr hWnd, uint message, Point pt, uint hitTest);
}
