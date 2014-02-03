using System;
using System.Drawing;
using System.Windows.Forms;

namespace Skybound.VisualTips;

public interface IVisualTipExtender
{
	object GetChildAtPoint(Control control, int x, int y);

	object GetParent(object component);

	Type[] GetChildTypes();

	Rectangle GetBounds(object component);
}
