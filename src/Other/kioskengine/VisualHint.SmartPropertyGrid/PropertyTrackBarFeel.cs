using System.Drawing;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

internal class PropertyTrackBarFeel : PropertyFeel
{
	private bool _editable;

	public PropertyTrackBarFeel(PropertyGrid grid, bool editable)
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
		if (propEnum.Property.Value.ReadOnly)
		{
			PropInPlaceTextbox propInPlaceTextbox;
			if (mInPlaceCtrl == null || !(mInPlaceCtrl is PropInPlaceTextbox))
			{
				propInPlaceTextbox = new PropInPlaceTextbox(isPassword: false);
				propInPlaceTextbox.Visible = false;
				propInPlaceTextbox.Parent = mParentWnd;
				mInPlaceCtrl = propInPlaceTextbox;
			}
			else
			{
				propInPlaceTextbox = (PropInPlaceTextbox)mInPlaceCtrl;
			}
			NotifyInPlaceControlCreated(propEnum);
			propInPlaceTextbox.OwnerPropertyEnumerator = propEnum;
			propInPlaceTextbox.Font = propEnum.Property.Value.Font;
			MoveControl(valueRect, propEnum);
			return base.ShowControl(valueRect, propEnum);
		}
		PropInPlaceTrackbar propInPlaceTrackbar;
		if (!(mInPlaceCtrl is PropInPlaceTrackbar))
		{
			propInPlaceTrackbar = new PropInPlaceTrackbar(_editable);
			propInPlaceTrackbar.Visible = false;
			propInPlaceTrackbar.Parent = mParentWnd;
			mInPlaceCtrl = propInPlaceTrackbar;
		}
		else
		{
			propInPlaceTrackbar = (PropInPlaceTrackbar)mInPlaceCtrl;
		}
		NotifyInPlaceControlCreated(propEnum);
		propInPlaceTrackbar.OwnerPropertyEnumerator = propEnum;
		propInPlaceTrackbar.Font = propEnum.Property.Value.Font;
		base.MoveControl(valueRect, propEnum);
		Control result = base.ShowControl(valueRect, propEnum);
		propInPlaceTrackbar.ResetTrackBarValue();
		return result;
	}

	public override void MoveControl(Rectangle valueRect, PropertyEnumerator propEnum)
	{
		if (mInPlaceCtrl == null)
		{
			return;
		}
		if (mInPlaceCtrl is PropInPlaceTextbox)
		{
			Graphics graphics = mParentWnd.CreateGraphics();
			Rectangle stringValueRect = propEnum.Property.Value.GetStringValueRect(graphics, valueRect, Point.Empty);
			stringValueRect.X -= ((propEnum.Property.Value.EditboxLeftMargin == -1) ? propEnum.Property.ParentGrid.GlobalTextMargin : propEnum.Property.Value.EditboxLeftMargin);
			stringValueRect.Width = valueRect.Right - stringValueRect.Left;
			Rectangle rectangle = stringValueRect;
			rectangle.Inflate(0, -1);
			Win32Calls.TEXTMETRIC tm = default(Win32Calls.TEXTMETRIC);
			Win32Calls.GetTextMetrics(graphics, mInPlaceCtrl.Font, ref tm);
			graphics.Dispose();
			int num = rectangle.Height - tm.tmHeight;
			rectangle.Y += num / 2;
			rectangle.Height -= num;
			if (mInPlaceCtrl.Bounds != rectangle)
			{
				mInPlaceCtrl.Bounds = rectangle;
			}
		}
		else
		{
			base.MoveControl(valueRect, propEnum);
		}
	}
}
