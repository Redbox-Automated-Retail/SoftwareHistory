using System;

namespace VisualHint.SmartPropertyGrid;

public class ValueAddedToPropertyAttribute : Attribute
{
	private string _propertyName;

	private string _valueKey;

	private int _hashCode;

	public string PropertyName => _propertyName;

	public string ValueKey => _valueKey;

	public ValueAddedToPropertyAttribute(string valueKey, string propertyName)
	{
		_propertyName = propertyName;
		_valueKey = valueKey;
	}

	public override int GetHashCode()
	{
		if (_hashCode == 0)
		{
			_hashCode = _propertyName.GetHashCode();
			_hashCode ^= _valueKey.GetHashCode();
		}
		return _hashCode;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ValueAddedToPropertyAttribute valueAddedToPropertyAttribute))
		{
			return false;
		}
		if (_propertyName.Equals(valueAddedToPropertyAttribute._propertyName))
		{
			return _valueKey.Equals(valueAddedToPropertyAttribute._valueKey);
		}
		return false;
	}
}
