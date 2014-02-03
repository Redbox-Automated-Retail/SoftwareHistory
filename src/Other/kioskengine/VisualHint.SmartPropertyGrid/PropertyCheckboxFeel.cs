using System.Drawing;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

internal class PropertyCheckboxFeel : PropertyFeel
{
	public PropertyCheckboxFeel(PropertyGrid grid)
		: base(grid)
	{
	}

	public override Control ShowControl(Rectangle valueRect, PropertyEnumerator propEnum)
	{
		PropInPlaceCheckbox propInPlaceCheckbox;
		if (mInPlaceCtrl == null)
		{
			propInPlaceCheckbox = new PropInPlaceCheckbox();
			propInPlaceCheckbox.Visible = false;
			propInPlaceCheckbox.Parent = mParentWnd;
			mInPlaceCtrl = propInPlaceCheckbox;
		}
		else
		{
			propInPlaceCheckbox = (PropInPlaceCheckbox)mInPlaceCtrl;
		}
		NotifyInPlaceControlCreated(propEnum);
		propInPlaceCheckbox.OwnerPropertyEnumerator = propEnum;
		propInPlaceCheckbox.Font = propEnum.Property.Value.Font;
		MoveControl(valueRect, propEnum);
		return base.ShowControl(valueRect, propEnum);
	}
}
