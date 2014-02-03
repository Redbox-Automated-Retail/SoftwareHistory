namespace VisualHint.SmartPropertyGrid;

public class PropertyCreatedEventArgs : PropertyEventArgs
{
	public PropertyCreatedEventArgs(PropertyEnumerator propEnum)
		: base(propEnum)
	{
	}
}
