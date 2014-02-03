using System;
using System.ComponentModel;
using System.Globalization;

namespace Alsing.Design;

public class Int32DefaultConverter : Int32Converter
{
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(string))
		{
			return true;
		}
		return base.CanConvertFrom(context, sourceType);
	}

	public override bool CanConvertTo(ITypeDescriptorContext context, Type t)
	{
		if (t == typeof(string))
		{
			return true;
		}
		return base.CanConvertTo(context, t);
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value is string)
		{
			if (value.ToString().ToLower() == "default")
			{
				return 0;
			}
			return Convert.ToInt32(value);
		}
		return base.ConvertFrom(context, culture, value);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == typeof(string))
		{
			int num = Convert.ToInt32(value);
			if (num == 0)
			{
				return "default";
			}
			return num.ToString();
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}
}
