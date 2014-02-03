using System;

namespace VisualHint.SmartPropertyGrid;

public class DropDownListboxVisualsAttribute : Attribute
{
	private bool _usePropertyValueVisuals;

	public bool UsePropertyValueVisuals => _usePropertyValueVisuals;

	public DropDownListboxVisualsAttribute(bool usePropertyValueVisuals)
	{
		_usePropertyValueVisuals = usePropertyValueVisuals;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is DropDownListboxVisualsAttribute dropDownListboxVisualsAttribute))
		{
			return false;
		}
		return _usePropertyValueVisuals == dropDownListboxVisualsAttribute._usePropertyValueVisuals;
	}

	public override int GetHashCode()
	{
		return _usePropertyValueVisuals.GetHashCode();
	}
}
