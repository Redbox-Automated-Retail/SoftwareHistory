using System;
using System.Drawing;
using System.Reflection;

namespace Skybound.ComponentModel;

[AttributeUsage(AttributeTargets.Field)]
internal sealed class EnumValueImageProviderAttribute : Attribute
{
	private string _x757f723eb7362ce0;

	private string _x34bbb6ae17f92a08;

	public Type ProviderType
	{
		get
		{
			if (_x757f723eb7362ce0 != null && _x757f723eb7362ce0.Length != 0)
			{
				return Type.GetType(_x757f723eb7362ce0);
			}
			return null;
		}
	}

	public string PropertyName => _x34bbb6ae17f92a08;

	public EnumValueImageProviderAttribute(Type providerType, string propertyName)
		: this(providerType.FullName, propertyName)
	{
	}

	public EnumValueImageProviderAttribute(string providerTypeName, string propertyName)
	{
		_x757f723eb7362ce0 = providerTypeName;
		_x34bbb6ae17f92a08 = propertyName;
	}

	public Image GetImage()
	{
		PropertyInfo property = ProviderType.GetProperty(PropertyName);
		if ((object)property != null)
		{
			return property.GetValue(null, null) as Image;
		}
		return null;
	}
}
