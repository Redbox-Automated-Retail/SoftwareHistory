namespace VisualHint.SmartPropertyGrid;

public class PropertyUpDownEventArgs : PropertyEventArgs
{
	public enum UpDownButtons
	{
		Up,
		Down
	}

	private UpDownButtons _buttonPressed;

	private string _value;

	public UpDownButtons ButtonPressed => _buttonPressed;

	public string Value
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

	public PropertyUpDownEventArgs(PropertyEnumerator propEnum, UpDownButtons buttonPressed)
		: base(propEnum)
	{
		_buttonPressed = buttonPressed;
	}
}
