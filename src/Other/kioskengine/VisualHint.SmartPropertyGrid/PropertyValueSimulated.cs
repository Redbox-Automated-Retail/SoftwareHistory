using System;
using System.Collections;
using System.ComponentModel;

namespace VisualHint.SmartPropertyGrid;

public class PropertyValueSimulated : PropertyValueIndirect
{
	public override bool IsDefault => base.OwnerEnumerator.Parent.Property.Value.IsDefault;

	public override object TargetInstance => base.OwnerEnumerator.Parent.Property.Value.GetValue();

	public override object[] TargetInstances
	{
		get
		{
			PropertyValue value = base.OwnerEnumerator.Parent.Property.Value;
			int num = value.TargetInstances.Length;
			if (num == 1)
			{
				return new object[1] { value.GetValue() };
			}
			object[] array = new object[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = value.GetValue(i);
			}
			return array;
		}
	}

	public PropertyValueSimulated(PropertyGrid grid, PropertyValue parentValue, object target, PropertyDescriptor propertyDescriptor)
		: base(grid, parentValue, target, propertyDescriptor, null)
	{
	}

	public override PropertyTypeDescriptorContext GetTypeDescriptorContext(PropertyEnumerator propEnum)
	{
		return new PropertyTypeDescriptorContext(base.ParentValue.PropertyDescriptor, (base.ParentValue.TargetInstances.Length > 1) ? base.ParentValue.TargetInstances : base.ParentValue.TargetInstance, propEnum, base.Grid);
	}

	protected override bool SetValuesInternal(object[] values, ValueUpdateSource updateSource)
	{
		try
		{
			PropertyValue value = mOwnerEnumerator.Parent.Property.Value;
			TypeConverter typeConverter = value.TypeConverter;
			object[] targetInstances = TargetInstances;
			object[] array = new object[targetInstances.Length];
			for (int i = 0; i < targetInstances.Length; i++)
			{
				object component = targetInstances[i];
				PropertyTypeDescriptorContext context = new PropertyTypeDescriptorContext((base.ParentValue.PropertyDescriptor as MultiplePropertyDescriptor)[i], base.ParentValue.TargetInstances[i], base.OwnerEnumerator, base.Grid);
				PropertyDescriptorCollection properties = typeConverter.GetProperties(context, value.GetValue(i));
				if (properties == null)
				{
					continue;
				}
				IDictionary dictionary = new Hashtable(properties.Count);
				foreach (PropertyDescriptor item in properties)
				{
					if (base.OwnerEnumerator.Property.Name.Equals(item.Name))
					{
						if (values[i] is string)
						{
							dictionary[item.Name] = ConvertDisplayedStringToValue(values[i] as string);
						}
						else
						{
							dictionary[item.Name] = values[i];
						}
					}
					else
					{
						dictionary[item.Name] = item.GetValue(component);
					}
				}
				try
				{
					array[i] = typeConverter.CreateInstance(context, dictionary);
				}
				catch (Exception innerException)
				{
					throw new PropertyValueSimulatedCreateInstanceException(innerException);
				}
			}
			if (base.PreventRefreshProperties)
			{
				value.PreventRefreshProperties = true;
			}
			PropertyEnumerator invalidPropEnum;
			if (array.Length == 1 || !value.HasMultipleValues)
			{
				value.SetValue(array[0], out invalidPropEnum);
			}
			else
			{
				value.SetValues(array, out invalidPropEnum);
			}
			if (base.PreventRefreshProperties)
			{
				value.PreventRefreshProperties = false;
			}
		}
		catch (Exception)
		{
			return false;
		}
		mUnderlyingType = null;
		return true;
	}

	protected override bool SetValueInternal(object value, ValueUpdateSource updateSource)
	{
		PropertyValue value2 = mOwnerEnumerator.Parent.Property.Value;
		TypeConverter typeConverter = value2.TypeConverter;
		object[] targetInstances = TargetInstances;
		object[] array = new object[targetInstances.Length];
		for (int i = 0; i < targetInstances.Length; i++)
		{
			object component = targetInstances[i];
			PropertyTypeDescriptorContext context = ((TargetInstances.Length != 1) ? new PropertyTypeDescriptorContext((base.ParentValue.PropertyDescriptor as MultiplePropertyDescriptor)[i], base.ParentValue.TargetInstances[i], base.OwnerEnumerator, base.Grid) : GetTypeDescriptorContext(base.OwnerEnumerator));
			PropertyDescriptorCollection properties = typeConverter.GetProperties(context, value2.GetValue(i));
			if (properties == null)
			{
				continue;
			}
			IDictionary dictionary = new Hashtable(properties.Count);
			foreach (PropertyDescriptor item in properties)
			{
				if (base.OwnerEnumerator.Property.Name.Equals(item.Name))
				{
					if (value is string)
					{
						dictionary[item.Name] = ConvertDisplayedStringToValue(value as string);
					}
					else
					{
						dictionary[item.Name] = value;
					}
				}
				else
				{
					dictionary[item.Name] = item.GetValue(component);
				}
			}
			try
			{
				array[i] = typeConverter.CreateInstance(context, dictionary);
			}
			catch (Exception innerException)
			{
				throw new PropertyValueSimulatedCreateInstanceException(innerException);
			}
		}
		if (base.PreventRefreshProperties)
		{
			value2.PreventRefreshProperties = true;
		}
		PropertyEnumerator invalidPropEnum;
		if (array.Length == 1 || !value2.HasMultipleValues)
		{
			value2.SetValue(array[0], out invalidPropEnum);
		}
		else
		{
			value2.SetValues(array, out invalidPropEnum);
		}
		if (base.PreventRefreshProperties)
		{
			value2.PreventRefreshProperties = false;
		}
		mUnderlyingType = null;
		return true;
	}

	public override bool IsReadOnly(PropertyEnumerator selfEnum)
	{
		if (!base.Grid.Grid.Enabled && base.Grid.DisableMode == PropertyGrid.DisableModes.Simple)
		{
			return true;
		}
		if (base.ParentValue.IsReadOnly(selfEnum.Parent))
		{
			return true;
		}
		return false;
	}
}
