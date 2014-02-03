using System.Globalization;

namespace Alsing.Text;

public class CharUtils
{
	private static bool[] isLetterLookup;

	private static bool[] isLetterOrDigitLookup;

	private static char[] lowerCharLookup;

	private static bool[] separatorCharLookup;

	private static char[] upperCharLookup;

	public static bool[] IsLetterOrDigitLookup
	{
		get
		{
			if (isLetterOrDigitLookup == null)
			{
				isLetterOrDigitLookup = new bool[65536];
				for (int i = 0; i < 65536; i++)
				{
					isLetterOrDigitLookup[i] = char.IsLetterOrDigit((char)i);
				}
			}
			return isLetterOrDigitLookup;
		}
	}

	public static bool[] IsLetterLookup
	{
		get
		{
			if (isLetterLookup == null)
			{
				isLetterLookup = new bool[65536];
				for (int i = 0; i < 65536; i++)
				{
					isLetterLookup[i] = char.IsLetter((char)i);
				}
			}
			return isLetterLookup;
		}
	}

	public static bool[] SeparatorCharLookup
	{
		get
		{
			if (separatorCharLookup == null)
			{
				separatorCharLookup = new bool[65536];
				for (int i = 0; i < 65536; i++)
				{
					separatorCharLookup[i] = char.IsSeparator((char)i);
				}
			}
			return separatorCharLookup;
		}
	}

	public static char[] LowerCharLookup
	{
		get
		{
			if (lowerCharLookup == null)
			{
				lowerCharLookup = new char[65536];
				for (int i = 0; i < 65536; i++)
				{
					lowerCharLookup[i] = char.ToLower((char)i, CultureInfo.InvariantCulture);
				}
			}
			return lowerCharLookup;
		}
	}

	public static char[] UpperCharLookup
	{
		get
		{
			if (upperCharLookup == null)
			{
				upperCharLookup = new char[65536];
				for (int i = 0; i < 65536; i++)
				{
					upperCharLookup[i] = char.ToUpper((char)i, CultureInfo.InvariantCulture);
				}
			}
			return upperCharLookup;
		}
	}

	public static char ToUpper(char c)
	{
		return UpperCharLookup[(uint)c];
	}

	public static char ToLower(char c)
	{
		return LowerCharLookup[(uint)c];
	}

	public static bool IsSeparator(char c)
	{
		return SeparatorCharLookup[(uint)c];
	}

	public static bool IsLetter(char c)
	{
		return IsLetterLookup[(uint)c];
	}

	public static bool IsLetterOrDigit(char c)
	{
		return IsLetterOrDigitLookup[(uint)c];
	}
}
