using System;

namespace Redbox.JSONPrettyPrinter;

internal sealed class Mask
{
	public static string NullString(string actual)
	{
		return (actual == null) ? string.Empty : actual;
	}

	public static string NullString(string actual, string mask)
	{
		return (actual == null) ? mask : actual;
	}

	public static string EmptyString(string actual, string emptyValue)
	{
		return (NullString(actual).Length == 0) ? emptyValue : actual;
	}

	private Mask()
	{
		throw new NotSupportedException();
	}
}
