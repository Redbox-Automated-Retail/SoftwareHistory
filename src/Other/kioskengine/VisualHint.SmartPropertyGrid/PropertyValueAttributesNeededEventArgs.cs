using System;

namespace VisualHint.SmartPropertyGrid;

public class PropertyValueAttributesNeededEventArgs : PropertyEventArgs
{
	private Attribute[] _attributes;

	public Attribute[] Attributes
	{
		get
		{
			return _attributes;
		}
		set
		{
			_attributes = value;
		}
	}

	public PropertyValueAttributesNeededEventArgs(PropertyEnumerator propEnum)
		: base(propEnum)
	{
	}
}
