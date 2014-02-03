using System;
using System.Windows.Forms;

namespace Skybound.VisualTips.External;

internal interface IVisualTipExtender
{
	object GetChildAtPoint(Control control, int x, int y);

	object GetParent(object component);

	Type[] GetChildTypes();
}
