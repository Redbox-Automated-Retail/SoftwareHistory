using System;

namespace VisualHint.SmartPropertyGrid;

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public class PropertyDropDownContentAttribute : PropertyAndChildrenAttribute
{
	private Type _controlType;

	private object[] _parameters;

	private int _hashCode;

	public Type ControlType => _controlType;

	public object[] Parameters => _parameters;

	public override object TypeId => "PropertyDropDownContent" + base.ChildPropertyName;

	public Type GetDropDownContent(string childPropertyName)
	{
		if (childPropertyName.CompareTo(base.ChildPropertyName) == 0)
		{
			return _controlType;
		}
		return null;
	}

	public PropertyDropDownContentAttribute(Type controlType)
	{
		_controlType = controlType;
	}

	public PropertyDropDownContentAttribute(Type controlType, params object[] parameters)
	{
		_controlType = controlType;
		_parameters = (object[])parameters.Clone();
	}

	public PropertyDropDownContentAttribute(string childPropertyName, Type controlType, params object[] parameters)
		: this(controlType, parameters)
	{
		if (childPropertyName == null)
		{
			throw new ArgumentException("The name of the child property can't be null");
		}
		mChildPropertyName = childPropertyName;
	}

	public PropertyDropDownContentAttribute(string childPropertyName, Type controlType)
		: this(controlType)
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
			_hashCode ^= _controlType.GetHashCode();
		}
		return _hashCode;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is PropertyDropDownContentAttribute propertyDropDownContentAttribute))
		{
			return false;
		}
		if (_controlType != propertyDropDownContentAttribute._controlType)
		{
			return false;
		}
		if (base.ChildPropertyName != null && propertyDropDownContentAttribute.ChildPropertyName != null && base.ChildPropertyName.CompareTo(propertyDropDownContentAttribute.ChildPropertyName) != 0)
		{
			return false;
		}
		return true;
	}
}
