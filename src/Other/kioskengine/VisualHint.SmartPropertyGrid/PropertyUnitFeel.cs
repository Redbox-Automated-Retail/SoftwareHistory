using System.Drawing;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

internal class PropertyUnitFeel : PropertyFeel
{
	public PropertyUnitFeel(PropertyGrid grid)
		: base(grid)
	{
	}

	public override Control ShowControl(Rectangle valueRect, PropertyEnumerator propEnum)
	{
		PropInPlaceUnit propInPlaceUnit;
		if (mInPlaceCtrl == null)
		{
			propInPlaceUnit = new PropInPlaceUnit();
			propInPlaceUnit.Visible = false;
			propInPlaceUnit.Parent = mParentWnd;
			mInPlaceCtrl = propInPlaceUnit;
		}
		else
		{
			propInPlaceUnit = (PropInPlaceUnit)mInPlaceCtrl;
		}
		NotifyInPlaceControlCreated(propEnum);
		propInPlaceUnit.OwnerPropertyEnumerator = propEnum;
		propInPlaceUnit.Font = propEnum.Property.Value.Font;
		MoveControl(valueRect, propEnum);
		return base.ShowControl(valueRect, propEnum);
	}
}
