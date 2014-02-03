using System;

namespace Skybound.ComponentModel;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
internal sealed class DisplayNameAttribute : Attribute
{
	private string _xc58acdbcdb3626dc;

	public string FriendlyName => _xc58acdbcdb3626dc;

	public DisplayNameAttribute(string friendlyName)
	{
		_xc58acdbcdb3626dc = friendlyName;
	}

	public static string GetFriendlyName(Type type)
	{
		object[] customAttributes = type.GetCustomAttributes(typeof(DisplayNameAttribute), inherit: false);
		if (customAttributes == null || customAttributes.Length == 0)
		{
			return type.FullName;
		}
		return (customAttributes[0] as DisplayNameAttribute).FriendlyName;
	}
}
