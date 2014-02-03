namespace VisualHint.SmartPropertyGrid;

public class CommitChangesEventArgs : PropertyEventArgs
{
	private object _value;

	private bool _valueIsText;

	public object Value
	{
		get
		{
			return _value;
		}
		set
		{
			_value = value;
		}
	}

	public bool ValueIsText
	{
		get
		{
			return _valueIsText;
		}
		set
		{
			_valueIsText = value;
		}
	}

	public CommitChangesEventArgs(PropertyEnumerator propEnum, object value, bool valueIsText)
		: base(propEnum)
	{
		Value = value;
		ValueIsText = ValueIsText;
	}
}
