using System.Drawing;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

public class PropertyFeel
{
	public static PropertyFeel Empty = new PropertyFeel(null);

	protected Control mInPlaceCtrl;

	protected Control mParentWnd;

	protected PropertyGrid mPropCtrl;

	private string _name;

	internal Control InPlaceCtrl => mInPlaceCtrl;

	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			_name = value;
		}
	}

	public PropertyFeel(PropertyGrid grid)
	{
		if (grid != null)
		{
			mPropCtrl = grid;
			mParentWnd = mPropCtrl.Grid;
		}
	}

	public virtual bool DontShowInPlaceCtrl(PropertyEnumerator propEnum)
	{
		return false;
	}

	public virtual Control ShowControl(Rectangle valueRect, PropertyEnumerator propEnum)
	{
		if (mInPlaceCtrl != null)
		{
			mInPlaceCtrl.Visible = true;
			mInPlaceCtrl.BackColor = ((InternalGrid)mParentWnd).DrawManager.GetValueBackgroundColor(propEnum);
			if (propEnum.Property.Value.ReadOnly)
			{
				if (!mPropCtrl.Enabled && mPropCtrl.DisableMode != PropertyGrid.DisableModes.None && mPropCtrl.DisableModeGrayedOut)
				{
					mInPlaceCtrl.ForeColor = mPropCtrl.DisabledForeColor;
				}
				else
				{
					mInPlaceCtrl.ForeColor = propEnum.Property.Value.ReadOnlyForeColor;
				}
			}
			else
			{
				mInPlaceCtrl.ForeColor = propEnum.Property.Value.ForeColor;
			}
		}
		return mInPlaceCtrl;
	}

	public void UpdateInPlaceControlFromValue(PropertyEnumerator propEnum)
	{
		if (mInPlaceCtrl != null)
		{
			mPropCtrl.HandleDefaultValue(propEnum);
			(mInPlaceCtrl as IInPlaceControl).OwnerPropertyEnumerator = propEnum;
			mInPlaceCtrl.Refresh();
		}
	}

	public void UpdateFont()
	{
		if (mInPlaceCtrl != null)
		{
			mInPlaceCtrl.Font = mPropCtrl.Font;
		}
	}

	public virtual void MoveControl(Rectangle valueRect, PropertyEnumerator propEnum)
	{
		if (mInPlaceCtrl != null)
		{
			valueRect.Height--;
			valueRect = mPropCtrl.TransformRectIfRTL(valueRect);
			if (mInPlaceCtrl.Bounds != valueRect)
			{
				mInPlaceCtrl.Invalidate();
				mInPlaceCtrl.Bounds = valueRect;
			}
			(mInPlaceCtrl as IInPlaceControl).RepositionChildren();
		}
	}

	protected void NotifyInPlaceControlCreated(PropertyEnumerator propEnum)
	{
		((PropertyGrid)mParentWnd.Parent).NotifyInPlaceControlCreated(new InPlaceCtrlCreatedEventArgs(propEnum, mInPlaceCtrl));
	}
}
