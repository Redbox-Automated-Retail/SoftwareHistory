using System.Drawing;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

internal class PropertyRadioButtonFeel : PropertyFeel
{
	public PropertyRadioButtonFeel(PropertyGrid grid)
		: base(grid)
	{
	}

	public override Control ShowControl(Rectangle valueRect, PropertyEnumerator propEnum)
	{
		PropInPlaceRadioButton propInPlaceRadioButton;
		if (mInPlaceCtrl == null)
		{
			propInPlaceRadioButton = new PropInPlaceRadioButton();
			propInPlaceRadioButton.Visible = false;
			propInPlaceRadioButton.Parent = mParentWnd;
			mInPlaceCtrl = propInPlaceRadioButton;
		}
		else
		{
			propInPlaceRadioButton = (PropInPlaceRadioButton)mInPlaceCtrl;
		}
		NotifyInPlaceControlCreated(propEnum);
		propInPlaceRadioButton.OwnerPropertyEnumerator = propEnum;
		propInPlaceRadioButton.Font = propEnum.Property.Value.Font;
		MoveControl(valueRect, propEnum);
		return base.ShowControl(valueRect, propEnum);
	}
}
