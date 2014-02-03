using System;

namespace VisualHint.SmartPropertyGrid;

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public class UpDownSettingsAttribute : PropertyAndChildrenAttribute
{
	private decimal _increment;

	private int _hashCode;

	public decimal Increment => _increment;

	public override object TypeId => "UpDownSettings" + base.ChildPropertyName;

	public UpDownSettingsAttribute(object increment)
	{
		_increment = Convert.ToDecimal(increment);
	}

	public UpDownSettingsAttribute(string childPropertyName, object increment)
	{
		if (childPropertyName == null)
		{
			throw new ArgumentException("The name of the child property can't be null");
		}
		mChildPropertyName = childPropertyName;
		_increment = Convert.ToDecimal(increment);
	}

	public override int GetHashCode()
	{
		if (_hashCode == 0)
		{
			_hashCode ^= base.ChildPropertyName.GetHashCode();
			_hashCode ^= _increment.GetHashCode();
		}
		return _hashCode;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is UpDownSettingsAttribute upDownSettingsAttribute))
		{
			return false;
		}
		if (base.ChildPropertyName != upDownSettingsAttribute.ChildPropertyName)
		{
			return false;
		}
		if (!_increment.Equals(upDownSettingsAttribute._increment))
		{
			return false;
		}
		return true;
	}
}
