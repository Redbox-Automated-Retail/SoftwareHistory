namespace VisualHint.SmartPropertyGrid;

public abstract class PropertyValidatorBase
{
	private PropertyEnumerator _enumerator;

	private static string _message = "";

	public PropertyEnumerator Enumerator
	{
		get
		{
			return _enumerator;
		}
		set
		{
			_enumerator = value;
		}
	}

	public static string Message
	{
		get
		{
			return _message;
		}
		set
		{
			_message = value;
		}
	}

	public PropertyValidatorBase()
	{
	}

	public PropertyValidatorBase(PropertyEnumerator propEnum)
	{
		_enumerator = propEnum;
	}

	public virtual bool Check(object value, bool modify)
	{
		return true;
	}
}
