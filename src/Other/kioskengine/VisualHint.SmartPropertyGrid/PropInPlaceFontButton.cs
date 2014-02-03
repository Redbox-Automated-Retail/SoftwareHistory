using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

[ToolboxItem(false)]
public class PropInPlaceFontButton : PropInPlaceButton
{
	public PropInPlaceFontButton()
		: base(editable: true)
	{
	}

	protected override void RunButton()
	{
		base.Parent.Focus();
		FontDialog fontDialog = new FontDialog();
		fontDialog.Font = (Font)mOwnerPropertyEnum.Property.Value.GetValue();
		if (fontDialog.ShowDialog() != DialogResult.Cancel)
		{
			mOwnerPropertyEnum.Property.Value.PreviousValue = mOwnerPropertyEnum.Property.Value.GetValue();
			mOwnerPropertyEnum.Property.Value.SetValue(fontDialog.Font);
			if (mEdit != null)
			{
				mEdit.Text = mOwnerPropertyEnum.Property.Value.DisplayString;
			}
			mOwnerPropertyEnum.Property.ParentGrid.NotifyPropertyChanged(new PropertyChangedEventArgs(mOwnerPropertyEnum));
		}
		mOwnerPropertyEnum.Property.ParentGrid.OnInPlaceCtrlFinishedEdition();
	}
}
