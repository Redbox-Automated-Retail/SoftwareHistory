using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

public interface IInPlaceControl
{
	bool RealtimeChange { get; set; }

	PropertyEnumerator OwnerPropertyEnumerator { get; set; }

	TextBox TextBox { get; }

	void OnF4();

	bool OnParentKeyDown(KeyEventArgs e);

	void RepositionChildren();

	bool GoToNextPropertyOnTab(bool forward);

	bool ScrollValueOnDoubleClick(bool forward, ScrollValueOnDoubleClickSource source);

	PropertyValue.ValueValidationResult CommitChanges(bool final);

	Control GetDropDownContainer();
}
