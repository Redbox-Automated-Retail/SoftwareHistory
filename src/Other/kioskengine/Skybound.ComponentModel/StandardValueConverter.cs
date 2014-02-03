using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;

namespace Skybound.ComponentModel;

internal abstract class StandardValueConverter : TypeConverter
{
	private Type xdcfa1dd29a7e2921;

	protected StandardValueConverter(Type standardType)
	{
		xdcfa1dd29a7e2921 = standardType;
	}

	public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
	{
		return true;
	}

	public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
	{
		return true;
	}

	public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
	{
		ArrayList arrayList = new ArrayList();
		ArrayList arrayList2 = new ArrayList();
		PropertyInfo[] properties = xdcfa1dd29a7e2921.GetProperties(BindingFlags.Static | BindingFlags.Public);
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (propertyInfo.IsDefined(typeof(StandardValueAttribute), inherit: true))
			{
				arrayList.Add(propertyInfo.GetValue(null, null));
				arrayList2.Add(propertyInfo.Name);
			}
		}
		Array array = arrayList.ToArray();
		Array.Sort(arrayList2.ToArray(), array);
		return new StandardValuesCollection(array);
	}

	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType != typeof(string) && destinationType != typeof(InstanceDescriptor))
		{
			return base.CanConvertTo(context, destinationType);
		}
		return true;
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == typeof(string))
		{
			PropertyInfo propertyInfo = x9c639476a525c3f4(value);
			if (propertyInfo != null)
			{
				return x6d5427cb1a81c070(propertyInfo);
			}
			throw new FormatException();
		}
		if (destinationType == typeof(InstanceDescriptor))
		{
			PropertyInfo propertyInfo2 = x9c639476a525c3f4(value);
			if (propertyInfo2 != null)
			{
				return new InstanceDescriptor(propertyInfo2, null, isComplete: true);
			}
			throw new FormatException();
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	private PropertyInfo x9c639476a525c3f4(object xbcea506a33cf9111)
	{
		PropertyInfo[] properties = xdcfa1dd29a7e2921.GetProperties(BindingFlags.Static | BindingFlags.Public);
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (propertyInfo.IsDefined(typeof(StandardValueAttribute), inherit: true) && object.Equals(propertyInfo.GetValue(null, null), xbcea506a33cf9111))
			{
				return propertyInfo;
			}
		}
		return null;
	}

	private string x6d5427cb1a81c070(PropertyInfo x46710263f0fedd95)
	{
		string name = (Attribute.GetCustomAttribute(x46710263f0fedd95, typeof(StandardValueAttribute)) as StandardValueAttribute).Name;
		if (name == null)
		{
			return x46710263f0fedd95.Name;
		}
		return name;
	}

	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType != typeof(string))
		{
			return base.CanConvertFrom(context, sourceType);
		}
		return true;
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value is string)
		{
			PropertyInfo[] properties = xdcfa1dd29a7e2921.GetProperties(BindingFlags.Static | BindingFlags.Public);
			foreach (PropertyInfo propertyInfo in properties)
			{
				if (propertyInfo.IsDefined(typeof(StandardValueAttribute), inherit: true) && x6d5427cb1a81c070(propertyInfo) == (string)value)
				{
					return propertyInfo.GetValue(null, null);
				}
			}
		}
		return base.ConvertFrom(context, culture, value);
	}
}
