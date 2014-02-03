using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;

namespace Skybound.ComponentModel;

internal abstract class RendererConverter : ExpandableObjectConverter, IComparer
{
	private Type x7b7d3ca0edcb2c59;

	private StandardValuesCollection _xc06186ffdd711674;

	private Hashtable x7061fe7e7c9334af;

	protected virtual object DefaultRenderer => x7b7d3ca0edcb2c59.GetProperty("DefaultRenderer", BindingFlags.Static | BindingFlags.Public).GetValue(null, null);

	protected RendererConverter(Type rendererType)
	{
		x7b7d3ca0edcb2c59 = rendererType;
	}

	protected virtual InstanceDescriptor CreateInstanceDescriptor(object renderer)
	{
		if (renderer == DefaultRenderer)
		{
			return new InstanceDescriptor(x7b7d3ca0edcb2c59.GetProperty("DefaultRenderer", BindingFlags.Static | BindingFlags.Public), null, isComplete: true);
		}
		return new InstanceDescriptor(renderer.GetType().GetConstructor(Type.EmptyTypes), null, renderer.GetType() == x7b7d3ca0edcb2c59);
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
		return xdc3b9070b0e5c53c(context);
	}

	private StandardValuesCollection xdc3b9070b0e5c53c(ITypeDescriptorContext x0f7b23d1c393aed9)
	{
		if (_xc06186ffdd711674 == null)
		{
			x7061fe7e7c9334af = new Hashtable();
			ArrayList arrayList = new ArrayList();
			arrayList.Add(DefaultRenderer);
			x7061fe7e7c9334af["(Default)"] = CreateInstanceDescriptor(DefaultRenderer);
			Type[] exportedTypes = x7b7d3ca0edcb2c59.Assembly.GetExportedTypes();
			foreach (Type type in exportedTypes)
			{
				if ((type == x7b7d3ca0edcb2c59 || type.IsSubclassOf(x7b7d3ca0edcb2c59)) && !type.IsAbstract)
				{
					string friendlyName = DisplayNameAttribute.GetFriendlyName(type);
					x7061fe7e7c9334af[friendlyName] = new InstanceDescriptor(type.GetConstructor(Type.EmptyTypes), null, isComplete: true);
					arrayList.Add(Activator.CreateInstance(type, null));
				}
			}
			arrayList.Sort(this);
			_xc06186ffdd711674 = new StandardValuesCollection(arrayList);
		}
		return _xc06186ffdd711674;
	}

	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(string) || destinationType == typeof(InstanceDescriptor))
		{
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == typeof(string))
		{
			return DisplayNameAttribute.GetFriendlyName(value.GetType());
		}
		if (destinationType == typeof(InstanceDescriptor))
		{
			return CreateInstanceDescriptor(value);
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(string))
		{
			return true;
		}
		return base.CanConvertFrom(context, sourceType);
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value is string)
		{
			if (x7061fe7e7c9334af.ContainsKey(value))
			{
				return (x7061fe7e7c9334af[value] as InstanceDescriptor).Invoke();
			}
			throw new FormatException();
		}
		return base.ConvertFrom(context, culture, value);
	}

	int IComparer.Compare(object x, object y)
	{
		string friendlyName = DisplayNameAttribute.GetFriendlyName(x.GetType());
		string friendlyName2 = DisplayNameAttribute.GetFriendlyName(y.GetType());
		return friendlyName.CompareTo(friendlyName2);
	}
}
