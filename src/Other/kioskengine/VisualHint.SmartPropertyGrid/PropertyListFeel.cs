using System.Drawing;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

internal class PropertyListFeel : PropertyFeel
{
	private bool _editable;

	public PropertyListFeel(PropertyGrid grid, bool editable)
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
		if (DontShowInPlaceCtrl(propEnum))
		{
			return null;
		}
		PropInPlaceList propInPlaceList;
		if (mInPlaceCtrl == null)
		{
			propInPlaceList = new PropInPlaceList(_editable);
			propInPlaceList.Visible = false;
			propInPlaceList.Parent = mParentWnd;
			mInPlaceCtrl = propInPlaceList;
		}
		else
		{
			propInPlaceList = (PropInPlaceList)mInPlaceCtrl;
		}
		NotifyInPlaceControlCreated(propEnum);
		propInPlaceList.OwnerPropertyEnumerator = propEnum;
		propInPlaceList.Font = propEnum.Property.Value.Font;
		MoveControl(valueRect, propEnum);
		return base.ShowControl(valueRect, propEnum);
	}
}
