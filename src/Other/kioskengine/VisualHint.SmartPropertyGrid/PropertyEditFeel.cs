using System.Drawing;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

internal class PropertyEditFeel : PropertyFeel
{
	private bool _password;

	public PropertyEditFeel(PropertyGrid grid, bool password)
		: base(grid)
	{
		_password = password;
	}

	public override Control ShowControl(Rectangle valueRect, PropertyEnumerator propEnum)
	{
		PropInPlaceTextbox propInPlaceTextbox;
		if (mInPlaceCtrl == null)
		{
			propInPlaceTextbox = new PropInPlaceTextbox(_password);
			propInPlaceTextbox.Visible = false;
			if (_password)
			{
				propInPlaceTextbox.PasswordChar = propEnum.Property.ParentGrid.DrawManager.PasswordChar;
			}
			propInPlaceTextbox.Parent = mParentWnd;
			mInPlaceCtrl = propInPlaceTextbox;
		}
		else
		{
			propInPlaceTextbox = (PropInPlaceTextbox)mInPlaceCtrl;
		}
		NotifyInPlaceControlCreated(propEnum);
		propInPlaceTextbox.OwnerPropertyEnumerator = propEnum;
		if (_password)
		{
			propInPlaceTextbox.Font = new Font("Microsoft Sans Serif", propEnum.Property.ParentGrid.Font.Size);
		}
		else
		{
			propInPlaceTextbox.Font = propEnum.Property.Value.Font;
		}
		MoveControl(valueRect, propEnum);
		return base.ShowControl(valueRect, propEnum);
	}

	public override void MoveControl(Rectangle valueRect, PropertyEnumerator propEnum)
	{
		if (mInPlaceCtrl != null)
		{
			Graphics graphics = mParentWnd.CreateGraphics();
			Rectangle stringValueRect = propEnum.Property.Value.GetStringValueRect(graphics, valueRect, Point.Empty);
			int globalTextMargin = propEnum.Property.ParentGrid.GlobalTextMargin;
			stringValueRect.X -= ((propEnum.Property.Value.EditboxLeftMargin == -1) ? globalTextMargin : propEnum.Property.Value.EditboxLeftMargin);
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
