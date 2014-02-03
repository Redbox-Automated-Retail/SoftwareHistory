using System.Drawing;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

internal class PropertyButtonFeel : PropertyFeel
{
	private bool _editable;

	public PropertyButtonFeel(PropertyGrid grid, bool editable)
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
		PropInPlaceButton propInPlaceButton;
		if (mInPlaceCtrl == null)
		{
			propInPlaceButton = new PropInPlaceButton(_editable);
			propInPlaceButton.Visible = false;
			propInPlaceButton.Parent = mParentWnd;
			mInPlaceCtrl = propInPlaceButton;
		}
		else
		{
			propInPlaceButton = (PropInPlaceButton)mInPlaceCtrl;
		}
		NotifyInPlaceControlCreated(propEnum);
		propInPlaceButton.OwnerPropertyEnumerator = propEnum;
		propInPlaceButton.Font = propEnum.Property.Value.Font;
		MoveControl(valueRect, propEnum);
		return base.ShowControl(valueRect, propEnum);
	}
}
