namespace VisualHint.SmartPropertyGrid;

public class PropertyChangedEventArgs : PropertyEventArgs
{
	private object _additionalValueKey;

	public object AdditionalValueKey
	{
		get
		{
			return _additionalValueKey;
		}
		set
		{
			_additionalValueKey = value;
		}
	}

	public PropertyChangedEventArgs(PropertyEnumerator propEnum, object additionalValueKey)
		: base(propEnum)
	{
		_additionalValueKey = additionalValueKey;
	}

	public PropertyChangedEventArgs(PropertyEnumerator propEnum)
		: base(propEnum)
	{
	}
}
