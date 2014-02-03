using System;
using System.Reflection;

namespace Redbox.JSONPrettyPrinter;

internal static class AttributeQuery<T> where T : Attribute
{
	public static T Get<P>(P provider) where P : ICustomAttributeProvider
	{
		return Get(provider, inherit: false);
	}

	public static T Get<P>(P provider, bool inherit) where P : ICustomAttributeProvider
	{
		T val = Find(provider, inherit);
		if (val == null)
		{
			throw new ObjectNotFoundException($"The attribute {typeof(T).FullName} was not found.");
		}
		return val;
	}

	public static T Find<P>(P provider) where P : ICustomAttributeProvider
	{
		return Find(provider, inherit: false);
	}

	public static T Find<P>(P provider, bool inherit) where P : ICustomAttributeProvider
	{
		T[] array = FindAll(provider, inherit);
		return (array.Length > 0) ? array[0] : null;
	}

	public static T FindAll<P>(P provider) where P : ICustomAttributeProvider
	{
		return Find(provider, inherit: false);
	}

	public static T[] FindAll<P>(P provider, bool inherit) where P : ICustomAttributeProvider
	{
		return (T[])provider.GetCustomAttributes(typeof(T), inherit);
	}
}
