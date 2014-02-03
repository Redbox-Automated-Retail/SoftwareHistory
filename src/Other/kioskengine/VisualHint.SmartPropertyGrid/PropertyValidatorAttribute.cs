using System;

namespace VisualHint.SmartPropertyGrid;

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public class PropertyValidatorAttribute : PropertyAndChildrenAttribute
{
	private PropertyValidatorBase _validator;

	private int _hashCode;

	public PropertyValidatorBase Validator => _validator;

	public override object TypeId => "PropertyValidator" + base.ChildPropertyName;

	public PropertyValidatorAttribute()
	{
	}

	public PropertyValidatorAttribute(Type validatorType)
	{
		_validator = Activator.CreateInstance(validatorType) as PropertyValidatorBase;
	}

	public PropertyValidatorAttribute(Type validatorType, params object[] parameters)
	{
		_validator = Activator.CreateInstance(validatorType, parameters) as PropertyValidatorBase;
	}

	public PropertyValidatorAttribute(string childPropertyName, Type validatorType, params object[] parameters)
	{
		if (childPropertyName == null)
		{
			throw new ArgumentException("The name of the child property can't be null");
		}
		mChildPropertyName = childPropertyName;
		_validator = Activator.CreateInstance(validatorType, parameters) as PropertyValidatorBase;
	}

	public override int GetHashCode()
	{
		if (_hashCode == 0)
		{
			_hashCode = base.ChildPropertyName.GetHashCode();
			_hashCode ^= _validator.GetHashCode();
		}
		return _hashCode;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is PropertyValidatorAttribute propertyValidatorAttribute))
		{
			return false;
		}
		if (!_validator.Equals(propertyValidatorAttribute._validator))
		{
			return false;
		}
		if (base.ChildPropertyName != propertyValidatorAttribute.ChildPropertyName)
		{
			return false;
		}
		return true;
	}
}
