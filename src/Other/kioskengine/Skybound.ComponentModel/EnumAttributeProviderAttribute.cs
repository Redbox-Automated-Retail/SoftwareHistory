using System;

namespace Skybound.ComponentModel;

[AttributeUsage(AttributeTargets.Property)]
internal sealed class EnumAttributeProviderAttribute : Attribute
{
	private string _x757f723eb7362ce0;

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

	public EnumAttributeProviderAttribute(Type providerType)
		: this(providerType.FullName)
	{
	}

	public EnumAttributeProviderAttribute(string providerTypeName)
	{
		_x757f723eb7362ce0 = providerTypeName;
	}
}
