namespace Alsing.Text.PatternMatchers;

public class PropertyPathPatterhMatcher : PatternMatcherBase
{
	public override int Match(string textToMatch, int matchAtIndex)
	{
		bool flag = true;
		int num = matchAtIndex;
		do
		{
			char c = textToMatch[num];
			if (flag && IsValidStartChar(c))
			{
				flag = false;
			}
			else
			{
				if (flag && IsWildcard(c))
				{
					num++;
					break;
				}
				if (!flag && IsSeparator(c))
				{
					flag = true;
				}
				else if (flag || !IsValidChar(c))
				{
					break;
				}
			}
			num++;
		}
		while (num < textToMatch.Length);
		if (textToMatch.Substring(matchAtIndex, num - matchAtIndex) == "*")
		{
			return 0;
		}
		return num - matchAtIndex;
	}

	private static bool IsWildcard(char c)
	{
		if (c != '*')
		{
			return c == '¤';
		}
		return true;
	}

	private static bool IsSeparator(char c)
	{
		return c == '.';
	}

	private static bool IsValidStartChar(char c)
	{
		if (CharUtils.IsLetter(c))
		{
			return true;
		}
		if ("_@".IndexOf(c) >= 0)
		{
			return true;
		}
		return false;
	}

	private static bool IsValidChar(char c)
	{
		if (CharUtils.IsLetterOrDigit(c) || c == '_')
		{
			return true;
		}
		return false;
	}
}
