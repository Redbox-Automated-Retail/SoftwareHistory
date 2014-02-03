using System;

namespace VisualHint.SmartPropertyGrid;

public class ShowChildPropertiesAttribute : Attribute
{
	private bool _show;

	public bool ShowChildProperties => _show;

	public ShowChildPropertiesAttribute(bool show)
	{
		_show = show;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ShowChildPropertiesAttribute showChildPropertiesAttribute))
		{
			return false;
		}
		return _show == showChildPropertiesAttribute._show;
	}

	public override int GetHashCode()
	{
		return _show.GetHashCode();
	}
}
