using System;

namespace VisualHint.SmartPropertyGrid;

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public class PropertyLookAttribute : PropertyAndChildrenAttribute
{
	private Type _lookType;

	private object[] _parameters;

	private int _hashCode;

	public Type LookType => _lookType;

	public object[] Parameters => _parameters;

	public override object TypeId => "PropertyLook" + base.ChildPropertyName;

	public PropertyLook GetPropertyLook(string childPropertyName)
	{
		if (base.ChildPropertyName.CompareTo(childPropertyName) == 0)
		{
			return (PropertyLook)Activator.CreateInstance(_lookType);
		}
		return null;
	}

	public PropertyLookAttribute(string childPropertyName, Type lookType)
	{
		if (childPropertyName == null)
		{
			throw new ArgumentException("The name of the child property can't be null");
		}
		mChildPropertyName = childPropertyName;
		_lookType = lookType;
	}

	public PropertyLookAttribute(string childPropertyName, Type lookType, params object[] parameters)
	{
		if (childPropertyName == null)
		{
			throw new ArgumentException("The name of the child property can't be null");
		}
		mChildPropertyName = childPropertyName;
		_lookType = lookType;
		_parameters = (object[])parameters.Clone();
	}

	public PropertyLookAttribute(Type lookType)
	{
		_lookType = lookType;
	}

	public PropertyLookAttribute(Type lookType, params object[] parameters)
	{
		_lookType = lookType;
		_parameters = (object[])parameters.Clone();
	}

	public override int GetHashCode()
	{
		if (_hashCode == 0)
		{
			_hashCode ^= base.ChildPropertyName.GetHashCode();
			_hashCode ^= _lookType.GetHashCode();
		}
		return _hashCode;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is PropertyLookAttribute propertyLookAttribute))
		{
			return false;
		}
		if (!_lookType.Equals(propertyLookAttribute._lookType))
		{
			return false;
		}
		if (base.ChildPropertyName != propertyLookAttribute.ChildPropertyName)
		{
			return false;
		}
		return true;
	}
}
