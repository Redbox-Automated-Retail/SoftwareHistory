using System;

namespace VisualHint.SmartPropertyGrid;

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public class PropertyFeelAttribute : PropertyAndChildrenAttribute
{
	private string _propertyFeelName;

	private int _hashCode;

	public string PropertyFeelName => _propertyFeelName;

	public override object TypeId => "PropertyFeel" + base.ChildPropertyName;

	public PropertyFeelAttribute(string childPropertyName, string propertyFeelName)
	{
		if (childPropertyName == null)
		{
			throw new ArgumentException("The name of the child property can't be null");
		}
		mChildPropertyName = childPropertyName;
		_propertyFeelName = propertyFeelName;
	}

	public PropertyFeelAttribute(string propertyFeelName)
	{
		_propertyFeelName = propertyFeelName;
	}

	public override int GetHashCode()
	{
		if (_hashCode == 0)
		{
			_hashCode ^= base.ChildPropertyName.GetHashCode();
			_hashCode ^= _propertyFeelName.GetHashCode();
		}
		return _hashCode;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is PropertyFeelAttribute propertyFeelAttribute))
		{
			return false;
		}
		if (_propertyFeelName != propertyFeelAttribute._propertyFeelName)
		{
			return false;
		}
		if (base.ChildPropertyName != propertyFeelAttribute.ChildPropertyName)
		{
			return false;
		}
		return true;
	}
}
