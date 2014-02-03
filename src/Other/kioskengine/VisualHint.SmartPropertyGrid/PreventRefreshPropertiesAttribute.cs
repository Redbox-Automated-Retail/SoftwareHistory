using System;

namespace VisualHint.SmartPropertyGrid;

public class PreventRefreshPropertiesAttribute : Attribute
{
	public override bool Equals(object obj)
	{
		return true;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
