using System;

namespace VisualHint.SmartPropertyGrid;

public class PropertyEventArgs : EventArgs
{
	private PropertyEnumerator _propertyEnum;

	public PropertyEnumerator PropertyEnum => _propertyEnum;

	internal PropertyEventArgs(PropertyEnumerator propEnum)
	{
		_propertyEnum = (PropertyEnumerator)propEnum.Clone();
	}
}
