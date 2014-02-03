using System.Drawing;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

internal class PropertyFontFeel : PropertyFeel
{
	public PropertyFontFeel(PropertyGrid grid)
		: base(grid)
	{
	}

	public override Control ShowControl(Rectangle valueRect, PropertyEnumerator propEnum)
	{
		PropInPlaceFontButton propInPlaceFontButton;
		if (mInPlaceCtrl == null)
		{
			propInPlaceFontButton = new PropInPlaceFontButton();
			propInPlaceFontButton.Visible = false;
			propInPlaceFontButton.Parent = mParentWnd;
			mInPlaceCtrl = propInPlaceFontButton;
		}
		else
		{
			propInPlaceFontButton = (PropInPlaceFontButton)mInPlaceCtrl;
		}
		NotifyInPlaceControlCreated(propEnum);
		propInPlaceFontButton.Text = propEnum.Property.Value.DisplayString;
		propInPlaceFontButton.OwnerPropertyEnumerator = propEnum;
		propInPlaceFontButton.Font = propEnum.Property.Value.Font;
		MoveControl(valueRect, propEnum);
		return base.ShowControl(valueRect, propEnum);
	}
}
