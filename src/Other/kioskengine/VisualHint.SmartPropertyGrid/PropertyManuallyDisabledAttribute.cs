using System;

namespace VisualHint.SmartPropertyGrid;

public class PropertyManuallyDisabledAttribute : Attribute
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
