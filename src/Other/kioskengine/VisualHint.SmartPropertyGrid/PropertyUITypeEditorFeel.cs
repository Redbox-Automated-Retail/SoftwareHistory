using System.Drawing;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

internal class PropertyUITypeEditorFeel : PropertyFeel
{
	public PropertyUITypeEditorFeel(PropertyGrid grid)
		: base(grid)
	{
	}

	public override Control ShowControl(Rectangle valueRect, PropertyEnumerator propEnum)
	{
		PropInPlaceUITypeEditor propInPlaceUITypeEditor;
		if (mInPlaceCtrl == null)
		{
			propInPlaceUITypeEditor = new PropInPlaceUITypeEditor();
			propInPlaceUITypeEditor.Visible = false;
			propInPlaceUITypeEditor.Parent = mParentWnd;
			mInPlaceCtrl = propInPlaceUITypeEditor;
		}
		else
		{
			propInPlaceUITypeEditor = (PropInPlaceUITypeEditor)mInPlaceCtrl;
		}
		propInPlaceUITypeEditor.TextBoxVisibility = ((UITypeEditorTextBoxVisibilityAttribute)propEnum.Property.Value.GetAttribute(typeof(UITypeEditorTextBoxVisibilityAttribute)))?.TextBoxVisible ?? true;
		NotifyInPlaceControlCreated(propEnum);
		propInPlaceUITypeEditor.OwnerPropertyEnumerator = propEnum;
		propInPlaceUITypeEditor.Font = propEnum.Property.Value.Font;
		MoveControl(valueRect, propEnum);
		return base.ShowControl(valueRect, propEnum);
	}
}
