using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

public class InPlaceCtrlVisibleEventArgs : PropertyEventArgs
{
	private Control _inPlaceCtrl;

	public Control InPlaceCtrl => _inPlaceCtrl;

	public InPlaceCtrlVisibleEventArgs(PropertyEnumerator propEnum, Control c)
		: base(propEnum)
	{
		_inPlaceCtrl = c;
	}
}
