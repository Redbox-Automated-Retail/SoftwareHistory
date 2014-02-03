using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;

namespace VisualHint.SmartPropertyGrid;

public class PropertyValueManaged : PropertyValue
{
	private class ManagedPropertyDescriptor : PropertyDescriptor
	{
		private PropertyValueManaged _propertyValue;

		public override Type ComponentType => _propertyValue.GetType();

		public override bool IsReadOnly => _propertyValue.ReadOnly;

		public override Type PropertyType => _propertyValue.UnderlyingType;

		public ManagedPropertyDescriptor(PropertyValueManaged propertyValue, Attribute[] attributes)
			: base("internal", attributes)
		{
			_propertyValue = propertyValue;
		}

		public override bool CanResetValue(object component)
		{
			return false;
		}

		public override void ResetValue(object component)
		{
		}

		public override bool ShouldSerializeValue(object component)
		{
			return true;
		}

		public override void SetValue(object component, object value)
		{
			_propertyValue.SetValueInternal(value, ValueUpdateSource.FromOutside);
			OnValueChanged(component, EventArgs.Empty);
		}

		public override object GetValue(object component)
		{
			return _propertyValue.GetValue();
		}
	}

	private ManagedPropertyDescriptor _propertyDescriptor;

	public override Type UnderlyingType => mUnderlyingType;

	public override TypeConverter TypeConverter
	{
		get
		{
			if (mConverter == null)
			{
				try
				{
					TypeConverterAttribute typeConverterAttribute = (TypeConverterAttribute)GetAttribute(typeof(TypeConverterAttribute));
					if (typeConverterAttribute != null && typeConverterAttribute.ConverterTypeName.Length > 0)
					{
						Type[] types = new Type[1] { typeof(Type) };
						Type type = Type.GetType(typeConverterAttribute.ConverterTypeName);
						if (type.GetConstructor(types) != null)
						{
							mConverter = (TypeConverter)Activator.CreateInstance(type, UnderlyingType);
						}
						else
						{
							mConverter = (TypeConverter)Activator.CreateInstance(type, nonPublic: false);
						}
					}
				}
				catch (Exception)
				{
				}
				if (mConverter == null)
				{
					TypeConverter converter = TypeDescriptor.GetConverter(UnderlyingType);
					if (UnderlyingType.Equals(typeof(object)) && converter.GetType() == typeof(TypeConverter) && UnderlyingValue != null)
					{
						mConverter = TypeDescriptor.GetConverter(UnderlyingValue.GetType());
					}
					else
					{
						mConverter = converter;
					}
				}
			}
			return mConverter;
		}
	}

	public override PropertyDescriptor PropertyDescriptor => _propertyDescriptor;

	public override object TargetInstance => this;

	public override object[] TargetInstances => new object[1] { TargetInstance };

	public override bool IsDefault
	{
		get
		{
			DefaultValueAttribute defaultValueAttribute = (DefaultValueAttribute)GetAttribute(typeof(DefaultValueAttribute));
			if (defaultValueAttribute == null)
			{
				return false;
			}
			if (defaultValueAttribute.Value == null)
			{
				return GetValue() == null;
			}
			return defaultValueAttribute.Value.Equals(GetValue());
		}
	}

	public override object GetValue(int index)
	{
		return UnderlyingValue;
	}

	public PropertyValueManaged(PropertyGrid grid, int id, Type valueType, object initialValue, Attribute[] attributes)
	{
		if (initialValue != null && initialValue != DBNull.Value && !valueType.IsInstanceOfType(initialValue))
		{
			throw new Exception("The initial value supplied to create the property has the wrong type. It should be of type " + valueType);
		}
		base.Grid = grid;
		mUnderlyingValue = initialValue;
		mUnderlyingType = valueType;
		base.Attributes = attributes;
		_propertyDescriptor = new ManagedPropertyDescriptor(this, attributes);
	}

	protected override bool SetValueInternal(object value, ValueUpdateSource updateSource)
	{
		bool result = false;
		NullableConverter nullableConverter = TypeConverter as NullableConverter;
		if (value == null || (value != null && UnderlyingType.IsInstanceOfType(value)) || (nullableConverter != null && (value == null || nullableConverter.UnderlyingType.IsInstanceOfType(value))))
		{
			UnderlyingValue = value;
			result = true;
		}
		return result;
	}

	public override bool ValidateValue(object value, out PropertyEnumerator invalidPropertyEnum)
	{
		invalidPropertyEnum = base.Grid.RightBound;
		if (base.Validator != null && !base.Validator.Check(value, modify: false))
		{
			invalidPropertyEnum = base.OwnerEnumerator;
			return false;
		}
		return true;
	}

	public override Attribute GetAttribute(Type attributeType)
	{
		Attribute attribute = base.GetAttribute(attributeType);
		if (attribute != null)
		{
			return attribute;
		}
		return TypeDescriptor.GetAttributes(UnderlyingType)[attributeType];
	}

	public override ArrayList GetAttributes(Type attributeType)
	{
		ArrayList attributes = base.GetAttributes(attributeType);
		if (attributes.Count == 0)
		{
			AttributeUsageAttribute attributeUsageAttribute = (AttributeUsageAttribute)TypeDescriptor.GetAttributes(attributeType)[typeof(AttributeUsageAttribute)];
			if (attributeUsageAttribute.AllowMultiple)
			{
				foreach (Attribute attribute3 in PropertyDescriptor.Attributes)
				{
					if (attribute3.GetType() == attributeType)
					{
						attributes.Add(attribute3);
					}
				}
			}
			if (attributes.Count == 0)
			{
				Attribute attribute2 = TypeDescriptor.GetAttributes(UnderlyingType)[attributeType];
				if (attribute2 != null)
				{
					attributes.Add(attribute2);
				}
			}
		}
		return attributes;
	}

	public override bool IsReadOnly(PropertyEnumerator selfEnum)
	{
		if (!base.Grid.Grid.Enabled && base.Grid.DisableMode == PropertyGrid.DisableModes.Simple)
		{
			return true;
		}
		bool flag = ((ReadOnlyAttribute)GetAttribute(typeof(ReadOnlyAttribute)))?.IsReadOnly ?? false;
		PropertyTypeDescriptorContext typeDescriptorContext = GetTypeDescriptorContext(selfEnum);
		if (flag)
		{
			object obj = null;
			try
			{
				obj = GetValue();
			}
			catch
			{
			}
			if (TypeConverter.CanConvertFrom(typeDescriptorContext, typeof(string)))
			{
				return true;
			}
			if (obj is Array || UnderlyingType.IsArray)
			{
				return true;
			}
			if (base.UITypeEditor != null && base.UITypeEditor.GetEditStyle(typeDescriptorContext) == UITypeEditorEditStyle.Modal && (obj is ICollection || typeof(ICollection).IsAssignableFrom(UnderlyingType) || (UnderlyingType.IsGenericType && typeof(ICollection<>).IsAssignableFrom(UnderlyingType.GetGenericTypeDefinition()))))
			{
				return false;
			}
			if ((!(GetValue() is ValueType) || !UnderlyingType.IsValueType) && base.Grid.HasOneVisibleChild(base.OwnerEnumerator))
			{
				return false;
			}
		}
		if (!flag && !base.Grid.HasOneVisibleChild(selfEnum) && (mFeel == null || mFeel == PropertyFeel.Empty || (mFeel != null && mFeel.GetType().Equals(typeof(PropertyEditFeel)) && !TypeConverter.CanConvertFrom(typeDescriptorContext, typeof(string)))))
		{
			return true;
		}
		return flag;
	}

	public override PropertyTypeDescriptorContext GetTypeDescriptorContext(PropertyEnumerator propEnum)
	{
		return new PropertyTypeDescriptorContext(PropertyDescriptor, this, propEnum, base.Grid);
	}

	public override void ResetToDefaultValue()
	{
		DefaultValueAttribute defaultValueAttribute = (DefaultValueAttribute)GetAttribute(typeof(DefaultValueAttribute));
		if (defaultValueAttribute != null)
		{
			SetValue(defaultValueAttribute.Value);
			if (base.Grid.InPlaceControl is IInPlaceControl inPlaceControl && inPlaceControl.OwnerPropertyEnumerator == mOwnerEnumerator)
			{
				inPlaceControl.OwnerPropertyEnumerator = mOwnerEnumerator;
			}
			else
			{
				base.Grid.Grid.Invalidate();
			}
			base.Grid.HandleDefaultValue(mOwnerEnumerator);
			CheckRecreateAfterValueChanged();
		}
	}
}
