using System;

namespace VisualHint.SmartPropertyGrid;

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public class PropertyDisableAttribute : PropertyAndChildrenAttribute
{
	private int _hashCode;

	public override object TypeId => "PropertyDisable" + base.ChildPropertyName;

	public PropertyDisableAttribute()
	{
	}

	public PropertyDisableAttribute(string childPropertyName)
	{
		if (childPropertyName == null)
		{
			throw new ArgumentException("The name of the child property can't be null");
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
		if (!(obj is PropertyDisableAttribute propertyDisableAttribute))
		{
			return false;
		}
		if (base.ChildPropertyName != null && propertyDisableAttribute.ChildPropertyName != null && base.ChildPropertyName.CompareTo(propertyDisableAttribute.ChildPropertyName) != 0)
		{
			return false;
		}
		return true;
	}
}
