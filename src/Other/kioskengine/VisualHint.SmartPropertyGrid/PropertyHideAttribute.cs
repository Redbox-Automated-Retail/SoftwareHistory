using System;

namespace VisualHint.SmartPropertyGrid;

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public class PropertyHideAttribute : PropertyAndChildrenAttribute
{
	private int _hashCode;

	public override object TypeId => "PropertyHide" + base.ChildPropertyName;

	public PropertyHideAttribute()
	{
	}

	public PropertyHideAttribute(string childPropertyName)
	{
		if (childPropertyName == null || childPropertyName.Length == 0)
		{
			throw new ArgumentException("The name of the child property can't be null or empty");
		}
		mChildPropertyName = childPropertyName;
	}

	public override int GetHashCode()
	{
		if (_hashCode == 0)
		{
			_hashCode = base.ChildPropertyName.GetHashCode();
		}
		return _hashCode;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is PropertyHideAttribute propertyHideAttribute))
		{
			return false;
		}
		if (base.ChildPropertyName != null && propertyHideAttribute.ChildPropertyName != null && base.ChildPropertyName.CompareTo(propertyHideAttribute.ChildPropertyName) != 0)
		{
			return false;
		}
		return true;
	}
}
