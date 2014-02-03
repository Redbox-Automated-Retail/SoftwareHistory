using System;
using System.Drawing;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

internal class PropertyDateTimeFeel : PropertyFeel
{
	public PropertyDateTimeFeel(PropertyGrid grid)
		: base(grid)
	{
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
		}
		else
		{
			PropInPlaceDateTime propInPlaceDateTime;
			if (mInPlaceCtrl == null || !(mInPlaceCtrl is PropInPlaceDateTime))
			{
				propInPlaceDateTime = new PropInPlaceDateTime();
				propInPlaceDateTime.Visible = false;
				propInPlaceDateTime.Parent = mParentWnd;
				propInPlaceDateTime.AutoSize = false;
				if (propInPlaceDateTime.RightToLeft == RightToLeft.Yes)
				{
					propInPlaceDateTime.RightToLeftLayout = true;
				}
				mInPlaceCtrl = propInPlaceDateTime;
			}
			else
			{
				propInPlaceDateTime = (PropInPlaceDateTime)mInPlaceCtrl;
			}
			propInPlaceDateTime.OwnerPropertyEnumerator = propEnum;
			propInPlaceDateTime.Font = propEnum.Property.Value.Font;
		}
		MoveControl(valueRect, propEnum);
		return base.ShowControl(valueRect, propEnum);
	}

	public override void MoveControl(Rectangle valueRect, PropertyEnumerator propEnum)
	{
		if (mInPlaceCtrl == null)
		{
			return;
		}
		if (mInPlaceCtrl is PropInPlaceDateTime)
		{
			Graphics graphics = mParentWnd.CreateGraphics();
			Rectangle stringValueRect = propEnum.Property.Value.GetStringValueRect(graphics, valueRect, Point.Empty);
			graphics.Dispose();
			int globalTextMargin = propEnum.Property.ParentGrid.GlobalTextMargin;
			stringValueRect.X -= ((propEnum.Property.Value.EditboxLeftMargin == -1) ? globalTextMargin : propEnum.Property.Value.EditboxLeftMargin);
			stringValueRect.Width = valueRect.Right - stringValueRect.Left;
			Win32Calls.RECT rc = default(Win32Calls.RECT);
			Win32Calls.GetWindowRect(mInPlaceCtrl.Handle, ref rc);
			Rectangle r = Rectangle.FromLTRB(rc.Left, rc.Top, rc.Right, rc.Bottom);
			r = mInPlaceCtrl.Parent.RectangleToClient(r);
			stringValueRect.Height--;
			stringValueRect = mPropCtrl.TransformRectIfRTL(stringValueRect);
			if (r != stringValueRect)
			{
				mInPlaceCtrl.Invalidate();
				Win32Calls.SetWindowPos(mInPlaceCtrl.Handle, IntPtr.Zero, stringValueRect.Left, stringValueRect.Top, stringValueRect.Width, stringValueRect.Height, 28);
				Win32Calls.RedrawWindow(mInPlaceCtrl.Handle, IntPtr.Zero, IntPtr.Zero, 1377);
			}
		}
		else
		{
			Graphics graphics2 = mParentWnd.CreateGraphics();
			Rectangle stringValueRect2 = propEnum.Property.Value.GetStringValueRect(graphics2, valueRect, Point.Empty);
			stringValueRect2.X -= ((propEnum.Property.Value.EditboxLeftMargin == -1) ? propEnum.Property.ParentGrid.GlobalTextMargin : propEnum.Property.Value.EditboxLeftMargin);
			stringValueRect2.Width = valueRect.Right - stringValueRect2.Left;
			Rectangle rect = stringValueRect2;
			rect.Inflate(0, -1);
			Win32Calls.TEXTMETRIC tm = default(Win32Calls.TEXTMETRIC);
			Win32Calls.GetTextMetrics(graphics2, mInPlaceCtrl.Font, ref tm);
			graphics2.Dispose();
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
