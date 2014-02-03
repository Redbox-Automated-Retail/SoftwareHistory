namespace VisualHint.SmartPropertyGrid;

public class PropertyChangingEventArgs : PropertyEventArgs
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

	public PropertyChangingEventArgs(PropertyEnumerator propEnum, object additionalValueKey)
		: base(propEnum)
	{
		_additionalValueKey = additionalValueKey;
	}

	public PropertyChangingEventArgs(PropertyEnumerator propEnum)
		: base(propEnum)
	{
	}
}
