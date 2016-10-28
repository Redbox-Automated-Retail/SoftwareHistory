using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Reflection;
using System.Text;

namespace VisualHint.SmartPropertyGrid;

public class PropertyValueIndirect : PropertyValue
{
	private enum TriStateBool
	{
		tsFalse,
		tsTrue,
		tsUndefined
	}

	private object[] _targets = new object[1];

	private int _targetsCount = 1;

	private PropertyDescriptor _propertyDescriptor;

	public override object TargetInstance
	{
		get
		{
			if (_targetsCount > 0 && _propertyDescriptor != null)
			{
				return _targets[0];
			}
			return base.ParentValue.UnderlyingValue;
		}
	}

	public override object[] TargetInstances
	{
		get
		{
			if (_targetsCount == 1)
			{
				return new object[1] { _targets[0] };
			}
			return _targets;
		}
	}

	public override PropertyDescriptor PropertyDescriptor => _propertyDescriptor;

	public override bool HasMultipleValues
	{
		get
		{
			if (_targetsCount == 1)
			{
				return false;
			}
			(_propertyDescriptor as MultiplePropertyDescriptor).GetValue(_targets, out var allEqual);
			return !allEqual;
		}
	}

	internal override object UnderlyingValue
	{
		get
		{
			if (_targetsCount == 1)
			{
				return _propertyDescriptor.GetValue(GetPropertyOwner(TargetInstance, _propertyDescriptor));
			}
			return _propertyDescriptor.GetValue(TargetInstances);
		}
	}

	public override Type UnderlyingType
	{
		get
		{
			if ((object)mUnderlyingType == null)
			{
				mUnderlyingType = _propertyDescriptor.PropertyType;
				if ((object)mUnderlyingType == typeof(object))
				{
					object value = GetValue(0);
					if (value != null)
					{
						mUnderlyingType = value.GetType();
					}
				}
			}
			return mUnderlyingType;
		}
	}

	public override TypeConverter TypeConverter
	{
		get
		{
			if (mConverter == null)
			{
				try
				{
					TypeConverterAttribute typeConverterAttribute = null;
					Attribute[] customAttributes = GetCustomAttributes(typeof(TypeConverterAttribute));
					if (customAttributes != null && customAttributes.Length > 0)
					{
						typeConverterAttribute = (TypeConverterAttribute)customAttributes[0];
					}
					if (typeConverterAttribute != null && typeConverterAttribute.ConverterTypeName.Length > 0)
					{
						Type[] types = new Type[1] { typeof(Type) };
						Type type = Type.GetType(typeConverterAttribute.ConverterTypeName);
						if ((object)type.GetConstructor(types) != null)
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
					TypeConverter converter = _propertyDescriptor.Converter;
					if ((object)_propertyDescriptor.PropertyType == typeof(object) && (object)converter.GetType() == typeof(TypeConverter))
					{
						mConverter = TypeDescriptor.GetConverter(UnderlyingType);
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

	public override string DisplayName => _propertyDescriptor.DisplayName;

	public override string Name => _propertyDescriptor.Name;

	public override bool IsDefault
	{
		get
		{
			DefaultValueAttribute defaultValueAttribute = null;
			Attribute[] customAttributes = GetCustomAttributes(typeof(DefaultValueAttribute));
			if (customAttributes != null && customAttributes.Length > 0)
			{
				defaultValueAttribute = (DefaultValueAttribute)customAttributes[0];
			}
			if (defaultValueAttribute != null)
			{
				if (defaultValueAttribute.Value == null)
				{
					return GetValue() == null;
				}
				return defaultValueAttribute.Value.Equals(GetValue());
			}
			if (TargetInstances.Length == 1)
			{
				return !_propertyDescriptor.ShouldSerializeValue(TargetInstance);
			}
			return !_propertyDescriptor.ShouldSerializeValue(TargetInstances);
		}
	}

	protected object GetPropertyOwner(object supposedTarget, PropertyDescriptor pd)
	{
		object obj = supposedTarget;
		if (obj is ICustomTypeDescriptor)
		{
			obj = ((ICustomTypeDescriptor)obj).GetPropertyOwner(pd);
		}
		return obj;
	}

	public PropertyValueIndirect(PropertyGrid grid, PropertyValue parentValue, object targetInstance, PropertyDescriptor propertyDescriptor, Attribute[] attributes)
		: base(parentValue)
	{
		base.Grid = grid;
		_propertyDescriptor = propertyDescriptor;
		if (targetInstance is object[])
		{
			_targets = targetInstance as object[];
			_targetsCount = _targets.Length;
		}
		else
		{
			_targets[0] = targetInstance;
		}
		base.Attributes = attributes;
	}

	public override bool HasMultipleTexts(out string text)
	{
		if (_targetsCount == 1)
		{
			try
			{
				text = ConvertValueToDisplayedString(UnderlyingValue);
			}
			catch (Exception ex)
			{
				if (ex.InnerException != null)
				{
					text = ex.InnerException.Message;
				}
				else
				{
					text = ex.Message;
				}
			}
			return false;
		}
		if (base.HasMultipleTextsMustBeCached && base.CachedMultipleText != null)
		{
			text = base.CachedMultipleText;
			return base.CachedHasMultipleText;
		}
		text = null;
		bool flag = false;
		for (int i = 0; i < _targetsCount; i++)
		{
			string text2 = null;
			object value = null;
			try
			{
				value = GetValue(i);
			}
			catch (Exception ex2)
			{
				text2 = ((ex2.InnerException == null) ? ex2.Message : ex2.InnerException.Message);
			}
			if (text == null)
			{
				try
				{
					if (text2 == null)
					{
						text2 = ConvertValueToDisplayedString(value, (_propertyDescriptor as MultiplePropertyDescriptor).PropertyDescriptors[i], _targets[i]);
					}
					text = ((text2 == null) ? "" : text2);
				}
				catch (Exception)
				{
				}
				continue;
			}
			try
			{
				if (text2 == null)
				{
					text2 = ConvertValueToDisplayedString(value, (_propertyDescriptor as MultiplePropertyDescriptor).PropertyDescriptors[i], _targets[i]);
				}
				if (text2 == null)
				{
					text2 = "";
				}
			}
			catch (Exception)
			{
			}
			if (!text.Equals(text2))
			{
				flag = true;
				text = "";
				break;
			}
		}
		base.CachedMultipleText = text;
		base.CachedHasMultipleText = flag;
		return flag;
	}

	public override object GetValue(int index)
	{
		if (index >= _targetsCount)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (_targetsCount == 1)
		{
			return _propertyDescriptor.GetValue(GetPropertyOwner(TargetInstance, _propertyDescriptor));
		}
		return (_propertyDescriptor as MultiplePropertyDescriptor).GetValue(TargetInstances, index);
	}

	private bool SetValueToMultipleTargets(object value)
	{
		if (_targetsCount == 1)
		{
			object propertyOwner = GetPropertyOwner(_targets[0], _propertyDescriptor);
			_propertyDescriptor.SetValue(propertyOwner, value);
		}
		else
		{
			_propertyDescriptor.SetValue(_targets, value);
		}
		return true;
	}

	public override bool ValidateValue(object value, out PropertyEnumerator invalidPropertyEnum)
	{
		invalidPropertyEnum = base.Grid.RightBound;
		if (base.Validator != null && !base.Validator.Check(value, modify: false))
		{
			invalidPropertyEnum = mOwnerEnumerator;
			return false;
		}
		return true;
	}

	protected override bool SetValuesInternal(object[] values, ValueUpdateSource updateSource)
	{
		for (int i = 0; i < values.Length; i++)
		{
			PropertyDescriptor propertyDescriptor = (_propertyDescriptor as MultiplePropertyDescriptor)[i];
			propertyDescriptor.SetValue(GetPropertyOwner(_targets[i], propertyDescriptor), values[i]);
		}
		return true;
	}

	protected override bool SetValueInternal(object value, ValueUpdateSource updateSource)
	{
		bool flag = false;
		if (value is string)
		{
			if (UnderlyingType.IsEnum)
			{
				object obj = TypeConverter.ConvertFromString(GetTypeDescriptorContext(base.OwnerEnumerator), base.CultureInfo, GetActualString(value as string));
				if (UnderlyingType.IsDefined(typeof(FlagsAttribute), inherit: false) || Enum.IsDefined(UnderlyingType, obj))
				{
					flag = SetValueToMultipleTargets(obj);
				}
			}
			else
			{
				string displayedStr = value as string;
				if (!PropertyValue.DontConvertStringValues)
				{
					GetTypeDescriptorContext(base.OwnerEnumerator);
					value = ConvertDisplayedStringToValue(displayedStr);
				}
				flag = SetValueToMultipleTargets(value);
			}
		}
		else
		{
			flag = SetValueToMultipleTargets(value);
		}
		if (flag)
		{
			mUnderlyingType = null;
		}
		return flag;
	}

	public override bool IsReadOnly(PropertyEnumerator selfEnum)
	{
		if (!base.Grid.Grid.Enabled && base.Grid.DisableMode == PropertyGrid.DisableModes.Simple)
		{
			return true;
		}
		ReadOnlyAttribute readOnlyAttribute = null;
		if (selfEnum.Parent.Property != null && selfEnum.Parent.Property.Value != null)
		{
			PropertyValue value = selfEnum.Parent.Property.Value;
			ReadOnlyChildPropertiesAttribute readOnlyChildPropertiesAttribute = (ReadOnlyChildPropertiesAttribute)value.GetAttribute(typeof(ReadOnlyChildPropertiesAttribute));
			if (readOnlyChildPropertiesAttribute != null)
			{
				return true;
			}
		}
		PropertyEnumerator propertyEnumerator = (PropertyEnumerator)selfEnum.Clone();
		PropertyValue value2 = propertyEnumerator.Property.Value;
		while (propertyEnumerator.HasParent)
		{
			propertyEnumerator = propertyEnumerator.Parent;
			if (propertyEnumerator.Property.Value != null)
			{
				value2 = propertyEnumerator.Property.Value;
				readOnlyAttribute = (ReadOnlyAttribute)TypeDescriptor.GetAttributes(value2.UnderlyingType)[typeof(ReadOnlyAttribute)];
				if (readOnlyAttribute != null && readOnlyAttribute.IsReadOnly)
				{
					return true;
				}
			}
		}
		if (value2 != null)
		{
			readOnlyAttribute = (ReadOnlyAttribute)TypeDescriptor.GetAttributes(value2.TargetInstance)[typeof(ReadOnlyAttribute)];
			if (readOnlyAttribute != null && readOnlyAttribute.IsReadOnly)
			{
				return true;
			}
		}
		bool isReadOnly = _propertyDescriptor.IsReadOnly;
		bool flag = ((ReadOnlyAttribute)selfEnum.Property.Value.GetAttribute(typeof(ReadOnlyAttribute)))?.IsReadOnly ?? false;
		bool flag2 = isReadOnly || flag;
		PropertyTypeDescriptorContext typeDescriptorContext = GetTypeDescriptorContext(selfEnum);
		if (flag2)
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
		if (!flag2 && !base.Grid.HasOneVisibleChild(selfEnum) && (mFeel == null || mFeel == PropertyFeel.Empty || (mFeel != null && mFeel.GetType().Equals(typeof(PropertyEditFeel)) && !TypeConverter.CanConvertFrom(typeDescriptorContext, typeof(string)))))
		{
			return true;
		}
		return flag2;
	}

	public override Attribute GetAttribute(Type attributeType)
	{
		Attribute attribute = base.GetAttribute(attributeType);
		if (attribute != null)
		{
			return attribute;
		}
		return _propertyDescriptor.Attributes[attributeType];
	}

	public override ArrayList GetAttributes(Type attributeType)
	{
		ArrayList attributes = base.GetAttributes(attributeType);
		if (attributes.Count == 0)
		{
			AttributeUsageAttribute attributeUsageAttribute = (AttributeUsageAttribute)TypeDescriptor.GetAttributes(attributeType)[typeof(AttributeUsageAttribute)];
			if (attributeUsageAttribute.AllowMultiple)
			{
				foreach (Attribute attribute in _propertyDescriptor.Attributes)
				{
					if (attributeType.IsAssignableFrom(attribute.GetType()))
					{
						attributes.Add(attribute);
					}
				}
			}
		}
		return attributes;
	}

	public override PropertyTypeDescriptorContext GetTypeDescriptorContext(PropertyEnumerator propEnum)
	{
		object instance = null;
		if (propEnum != null)
		{
			PropertyValue value = propEnum.Property.Value;
			instance = ((value.TargetInstances.Length != 1) ? value.TargetInstances : value.TargetInstance);
		}
		return new PropertyTypeDescriptorContext(_propertyDescriptor, instance, propEnum, base.Grid);
	}

	internal void AddTargetInstance(object targetInstance, PropertyDescriptor propertyDescriptor)
	{
		if (_targetsCount == 1)
		{
			_propertyDescriptor = new MultiplePropertyDescriptor(new PropertyDescriptor[2] { _propertyDescriptor, propertyDescriptor });
		}
		else
		{
			PropertyDescriptor[] propertyDescriptors = ((MultiplePropertyDescriptor)_propertyDescriptor).PropertyDescriptors;
			PropertyDescriptor[] array = new PropertyDescriptor[propertyDescriptors.Length + 1];
			Array.Copy(propertyDescriptors, array, propertyDescriptors.Length);
			array[propertyDescriptors.Length] = propertyDescriptor;
			_propertyDescriptor = new MultiplePropertyDescriptor(array);
		}
		if (_targets.Length == _targetsCount)
		{
			object[] array2 = new object[_targetsCount * 2];
			Array.Copy(_targets, 0, array2, 0, _targetsCount);
			_targets = array2;
		}
		_targets[_targetsCount++] = targetInstance;
		PropertyEnumerator children = base.OwnerEnumerator.Children;
		object value = GetValue(_targetsCount - 1);
		while (children != children.RightBound)
		{
			((PropertyValueIndirect)children.Property.Value).AddTargetInstance(value, children.Property.Value.PropertyDescriptor);
			children.MoveNext();
		}
	}

	public void AddTargetInstance(object targetInstance, string memberName)
	{
		PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(targetInstance, new Attribute[1] { BrowsableAttribute.Yes })[memberName];
		AddTargetInstance(targetInstance, propertyDescriptor);
	}

	public override void ResetToDefaultValue()
	{
		DefaultValueAttribute defaultValueAttribute = (DefaultValueAttribute)GetAttribute(typeof(DefaultValueAttribute));
		if (defaultValueAttribute != null)
		{
			SetValue(defaultValueAttribute.Value);
			return;
		}
		StringBuilder stringBuilder = new StringBuilder("Reset", 50);
		stringBuilder.Append(base.OwnerEnumerator.Property.Name);
		MethodInfo method = TargetInstance.GetType().GetMethod(stringBuilder.ToString(), BindingFlags.Instance | BindingFlags.NonPublic);
		if ((object)method != null)
		{
			method.Invoke(TargetInstance, new object[0]);
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
