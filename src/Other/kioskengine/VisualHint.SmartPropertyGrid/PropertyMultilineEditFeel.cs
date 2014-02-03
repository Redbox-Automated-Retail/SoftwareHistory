using System;
using System.Drawing;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

internal class PropertyMultilineEditFeel : PropertyFeel
{
	public PropertyMultilineEditFeel(PropertyGrid grid)
		: base(grid)
	{
	}

	public override Control ShowControl(Rectangle valueRect, PropertyEnumerator propEnum)
	{
		PropInPlaceTextbox propInPlaceTextbox = null;
		if (mInPlaceCtrl == null)
		{
			propInPlaceTextbox = new PropInPlaceTextbox(isPassword: false);
			propInPlaceTextbox.Visible = false;
			propInPlaceTextbox.Parent = mParentWnd;
			propInPlaceTextbox.Multiline = true;
			mInPlaceCtrl = propInPlaceTextbox;
			propEnum.Property.ParentGrid.RightToLeftChanged += ParentGrid_RightToLeftChanged;
		}
		else
		{
			propInPlaceTextbox = (PropInPlaceTextbox)mInPlaceCtrl;
			propInPlaceTextbox.RightToLeft = propEnum.Property.ParentGrid.RightToLeft;
		}
		NotifyInPlaceControlCreated(propEnum);
		propInPlaceTextbox.OwnerPropertyEnumerator = propEnum;
		propInPlaceTextbox.Font = propEnum.Property.Value.Font;
		MoveControl(valueRect, propEnum);
		if (!propInPlaceTextbox.Focused && propInPlaceTextbox.Text != propEnum.Property.Value.DisplayString)
		{
			propInPlaceTextbox.Text = propEnum.Property.Value.DisplayString;
		}
		return base.ShowControl(valueRect, propEnum);
	}

	private void ParentGrid_RightToLeftChanged(object sender, EventArgs e)
	{
		((PropInPlaceTextbox)mInPlaceCtrl).OwnerPropertyEnumerator.Property.ParentGrid.RightToLeftChanged -= ParentGrid_RightToLeftChanged;
		mInPlaceCtrl = null;
	}

	public override void MoveControl(Rectangle valueRect, PropertyEnumerator propEnum)
	{
		if (mInPlaceCtrl != null)
		{
			Graphics graphics = mParentWnd.CreateGraphics();
			Rectangle stringValueRect = propEnum.Property.Value.GetStringValueRect(graphics, valueRect, Point.Empty);
			graphics.Dispose();
			int globalTextMargin = propEnum.Property.ParentGrid.GlobalTextMargin;
			stringValueRect.X -= ((propEnum.Property.Value.EditboxLeftMargin == -1) ? globalTextMargin : propEnum.Property.Value.EditboxLeftMargin);
			stringValueRect.Width = valueRect.Right - stringValueRect.Left - globalTextMargin;
			stringValueRect.Y += 2;
			stringValueRect.Height = valueRect.Bottom - stringValueRect.Y - 1;
			stringValueRect = mPropCtrl.TransformRectIfRTL(stringValueRect);
			if (mInPlaceCtrl.Bounds != stringValueRect)
			{
				mInPlaceCtrl.Bounds = stringValueRect;
			}
		}
	}
}
