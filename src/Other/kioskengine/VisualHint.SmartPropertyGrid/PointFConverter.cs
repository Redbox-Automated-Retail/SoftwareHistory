using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Globalization;
using System.Reflection;

namespace VisualHint.SmartPropertyGrid;

public class PointFConverter : ExpandableObjectConverter
{
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if ((object)sourceType == typeof(string))
		{
			return true;
		}
		return base.CanConvertFrom(context, sourceType);
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (!(value is string text))
		{
			return base.ConvertFrom(context, culture, value);
		}
		string text2 = text.Trim();
		if (text2.Length == 0)
		{
			return null;
		}
		if (culture == null)
		{
			culture = CultureInfo.CurrentCulture;
		}
		char c = culture.TextInfo.ListSeparator[0];
		string[] array = text2.Split(c);
		float[] array2 = new float[array.Length];
		TypeConverter converter = TypeDescriptor.GetConverter(typeof(float));
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i] = (float)converter.ConvertFromString(context, culture, array[i]);
		}
		if (array2.Length != 2)
		{
			throw new ArgumentException("Parse failed.");
		}
		return new PointF(array2[0], array2[1]);
	}

	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if ((object)destinationType == typeof(string))
		{
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if ((object)destinationType == null)
		{
			throw new ArgumentNullException("destinationType");
		}
		if (value is PointF)
		{
			if ((object)destinationType == typeof(string))
			{
				PointF pointF = (PointF)value;
				if (culture == null)
				{
					culture = CultureInfo.CurrentCulture;
				}
				string separator = culture.TextInfo.ListSeparator + " ";
				TypeConverter converter = TypeDescriptor.GetConverter(typeof(float));
				string[] array = new string[2];
				int num = 0;
				array[num++] = converter.ConvertToString(context, culture, pointF.X);
				array[num++] = converter.ConvertToString(context, culture, pointF.Y);
				return string.Join(separator, array);
			}
			if ((object)destinationType == typeof(InstanceDescriptor))
			{
				PointF pointF2 = (PointF)value;
				ConstructorInfo constructor = typeof(PointF).GetConstructor(new Type[2]
				{
					typeof(float),
					typeof(float)
				});
				if ((object)constructor != null)
				{
					return new InstanceDescriptor(constructor, new object[2] { pointF2.X, pointF2.Y });
				}
			}
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
	{
		return true;
	}

	public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
	{
		if (propertyValues != null)
		{
			return new PointF((float)propertyValues["X"], (float)propertyValues["Y"]);
		}
		return null;
	}
}
