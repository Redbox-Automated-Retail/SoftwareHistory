using System;

namespace VisualHint.SmartPropertyGrid;

public class UITypeEditorTextBoxVisibilityAttribute : Attribute
{
	private bool _visibility;

	public bool TextBoxVisible => _visibility;

	public UITypeEditorTextBoxVisibilityAttribute(bool visible)
	{
		_visibility = visible;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is UITypeEditorTextBoxVisibilityAttribute uITypeEditorTextBoxVisibilityAttribute))
		{
			return false;
		}
		return _visibility == uITypeEditorTextBoxVisibilityAttribute._visibility;
	}

	public override int GetHashCode()
	{
		return _visibility.GetHashCode();
	}
}
