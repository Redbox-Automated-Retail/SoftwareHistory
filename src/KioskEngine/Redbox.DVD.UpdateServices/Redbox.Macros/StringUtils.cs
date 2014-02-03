using System;
using System.Collections.Specialized;

namespace Redbox.Macros;

internal static class StringUtils
{
	public static bool EndsWith(string value, char c)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		int length = value.Length;
		return length != 0 && value[length - 1] == c;
	}

	public static string ConvertEmptyToNull(string value)
	{
		return (!string.IsNullOrEmpty(value)) ? value : null;
	}

	public static string ConvertNullToEmpty(string value)
	{
		return value ?? string.Empty;
	}

	public static string Join(string separator, StringCollection value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (separator == null)
		{
			separator = string.Empty;
		}
		string[] array = new string[value.Count];
		value.CopyTo(array, 0);
		return string.Join(separator, array);
	}

	public static StringCollection Clone(StringCollection stringCollection)
	{
		string[] array = new string[stringCollection.Count];
		stringCollection.CopyTo(array, 0);
		StringCollection stringCollection2 = new StringCollection();
		stringCollection2.AddRange(array);
		return stringCollection2;
	}
}
