using System.Drawing;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

internal class PropertyMaskedEditFeel : PropertyFeel
{
	public PropertyMaskedEditFeel(PropertyGrid grid)
		: base(grid)
	{
	}

	public override Control ShowControl(Rectangle valueRect, PropertyEnumerator propEnum)
	{
		PropInPlaceMaskedTextbox propInPlaceMaskedTextbox;
		if (mInPlaceCtrl == null)
		{
			propInPlaceMaskedTextbox = new PropInPlaceMaskedTextbox();
			propInPlaceMaskedTextbox.Visible = false;
			propInPlaceMaskedTextbox.Parent = mParentWnd;
			mInPlaceCtrl = propInPlaceMaskedTextbox;
		}
		else
		{
			propInPlaceMaskedTextbox = (PropInPlaceMaskedTextbox)mInPlaceCtrl;
		}
		NotifyInPlaceControlCreated(propEnum);
		if (propEnum.Property.Value.Look is PropertyMaskedEditLook)
		{
			propInPlaceMaskedTextbox.Mask = (propEnum.Property.Value.Look as PropertyMaskedEditLook).Mask;
			propInPlaceMaskedTextbox.PromptChar = (propEnum.Property.Value.Look as PropertyMaskedEditLook).PromptChar;
		}
		propInPlaceMaskedTextbox.OwnerPropertyEnumerator = propEnum;
		propInPlaceMaskedTextbox.Font = propEnum.Property.Value.Font;
		MoveControl(valueRect, propEnum);
		base.ShowControl(valueRect, propEnum);
		return propInPlaceMaskedTextbox;
	}

	public override void MoveControl(Rectangle valueRect, PropertyEnumerator propEnum)
	{
		if (mInPlaceCtrl != null)
		{
			mInPlaceCtrl.BackColor = propEnum.Property.Value.BackColor;
			Graphics graphics = mParentWnd.CreateGraphics();
			Rectangle stringValueRect = propEnum.Property.Value.GetStringValueRect(graphics, valueRect, Point.Empty);
			stringValueRect.X -= ((propEnum.Property.Value.EditboxLeftMargin == -1) ? propEnum.Property.ParentGrid.GlobalTextMargin : propEnum.Property.Value.EditboxLeftMargin);
			stringValueRect.Width = valueRect.Right - stringValueRect.Left;
			Rectangle rect = stringValueRect;
			rect.Inflate(0, -1);
			Win32Calls.TEXTMETRIC tm = default(Win32Calls.TEXTMETRIC);
			Win32Calls.GetTextMetrics(graphics, mInPlaceCtrl.Font, ref tm);
			graphics.Dispose();
			int num = rect.Height - tm.tmHeight;
			rect.Y += num / 2;
			rect.Height -= num;
			rect = mPropCtrl.TransformRectIfRTL(rect);
			if (mInPlaceCtrl.Bounds != rect)
			{
				mInPlaceCtrl.Bounds = rect;
			}
		}
	}
}
