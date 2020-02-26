using Redbox.JSONPrettyPrinter;

namespace Redbox.KioskEngine.ComponentModel;

public static class StringExtensionMethods
{
	public static string ReplaceFirst(this string text, string search, string replace)
	{
		int num = text.IndexOf(search);
		if (num < 0)
		{
			return text;
		}
		return text.Substring(0, num) + replace + text.Substring(num + search.Length);
	}

	public static string ToPrettyJson(this string source)
	{
		if (source == null)
		{
			return string.Empty;
		}
		return PrettyPrinter.GetPrettyString(source);
	}
}
