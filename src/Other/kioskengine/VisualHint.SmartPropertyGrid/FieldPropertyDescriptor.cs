using System;
using System.ComponentModel;
using System.Reflection;

namespace VisualHint.SmartPropertyGrid;

public class FieldPropertyDescriptor : PropertyDescriptor
{
	private FieldInfo _field;

	public FieldInfo Field => _field;

	public override bool IsReadOnly => false;

	public override Type ComponentType => _field.DeclaringType;

	public override Type PropertyType => _field.FieldType;

	public FieldPropertyDescriptor(FieldInfo field)
		: base(field.Name, (Attribute[])field.GetCustomAttributes(typeof(Attribute), inherit: true))
	{
		_field = field;
	}

	public override bool Equals(object obj)
	{
		if (obj is FieldPropertyDescriptor fieldPropertyDescriptor)
		{
			return fieldPropertyDescriptor._field.Equals(_field);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return _field.GetHashCode();
	}

	public override void ResetValue(object component)
	{
	}

	public override bool CanResetValue(object component)
	{
		return false;
	}

	public override bool ShouldSerializeValue(object component)
	{
		return true;
	}

	public override object GetValue(object component)
	{
		return _field.GetValue(component);
	}

	public override void SetValue(object component, object value)
	{
		_field.SetValue(component, value);
		OnValueChanged(component, EventArgs.Empty);
	}
}
