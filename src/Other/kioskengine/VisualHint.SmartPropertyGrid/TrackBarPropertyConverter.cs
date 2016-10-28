using System;
using System.ComponentModel;
using System.Globalization;

namespace VisualHint.SmartPropertyGrid;

public class TrackBarPropertyConverter : TypeConverter
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
		if (value is string)
		{
			Type baseType = ((GenericTrackBar)context.Instance)._baseType;
			return TypeDescriptor.GetConverter(baseType).ConvertFrom(context, culture, value);
		}
		return base.ConvertFrom(context, culture, value);
	}
}
