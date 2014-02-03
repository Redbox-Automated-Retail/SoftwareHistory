using System.Drawing;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

internal class PropertyUpDownFeel : PropertyFeel
{
	private bool _editable;

	public PropertyUpDownFeel(PropertyGrid grid, bool editable)
		: base(grid)
	{
		_editable = editable;
	}

	public override bool DontShowInPlaceCtrl(PropertyEnumerator propEnum)
	{
		if (propEnum.Property.Value.IsReadOnly(propEnum))
		{
			return !_editable;
		}
		return false;
	}

	public override Control ShowControl(Rectangle valueRect, PropertyEnumerator propEnum)
	{
		if (propEnum.Property.Value.IsReadOnly(propEnum) && !_editable)
		{
			return null;
		}
		PropInPlaceUpDown propInPlaceUpDown;
		if (mInPlaceCtrl == null)
		{
			propInPlaceUpDown = new PropInPlaceUpDown(_editable);
			propInPlaceUpDown.Visible = false;
			propInPlaceUpDown.Parent = mParentWnd;
			mInPlaceCtrl = propInPlaceUpDown;
		}
		else
		{
			propInPlaceUpDown = (PropInPlaceUpDown)mInPlaceCtrl;
		}
		NotifyInPlaceControlCreated(propEnum);
		propInPlaceUpDown.OwnerPropertyEnumerator = propEnum;
		propInPlaceUpDown.Font = propEnum.Property.Value.Font;
		MoveControl(valueRect, propEnum);
		return base.ShowControl(valueRect, propEnum);
	}
}
