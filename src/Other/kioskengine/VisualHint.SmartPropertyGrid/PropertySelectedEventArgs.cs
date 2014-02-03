namespace VisualHint.SmartPropertyGrid;

public class PropertySelectedEventArgs : PropertyEventArgs
{
	private PropertyEnumerator _previousSelectedPropertyEnum;

	public PropertyEnumerator PreviousSelectedPropertyEnum => _previousSelectedPropertyEnum;

	public PropertySelectedEventArgs(PropertyEnumerator propEnum, PropertyEnumerator previousSelectedPropEnum)
		: base(propEnum)
	{
		_previousSelectedPropertyEnum = previousSelectedPropEnum;
	}
}
