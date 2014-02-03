namespace VisualHint.SmartPropertyGrid;

public class PropertyButtonClickedEventArgs : PropertyEventArgs
{
	private bool _propertyChanged;

	public bool PropertyChanged
	{
		get
		{
			return _propertyChanged;
		}
		set
		{
			_propertyChanged = value;
		}
	}

	public PropertyButtonClickedEventArgs(PropertyEnumerator propEnum)
		: base(propEnum)
	{
	}
}
