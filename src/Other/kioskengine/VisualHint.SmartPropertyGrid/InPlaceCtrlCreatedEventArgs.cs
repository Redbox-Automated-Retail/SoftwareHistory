using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

public class InPlaceCtrlCreatedEventArgs : PropertyEventArgs
{
	private Control _inPlaceCtrl;

	public Control InPlaceCtrl => _inPlaceCtrl;

	public InPlaceCtrlCreatedEventArgs(PropertyEnumerator propEnum, Control c)
		: base(propEnum)
	{
		_inPlaceCtrl = c;
	}
}
