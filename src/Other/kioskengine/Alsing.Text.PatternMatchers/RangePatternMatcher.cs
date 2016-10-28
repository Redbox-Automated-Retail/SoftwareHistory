using System.Runtime.InteropServices;

namespace Alsing.Text.PatternMatchers;

[ComVisible(true)]
public class RangePatternMatcher : PatternMatcherBase
{
	public override string[] DefaultPrefixes => new string[1] { StartChar.ToString() };

	public char StartChar { get; set; }

	public char EndChar { get; set; }

	public char EscapeChar { get; set; }

	public RangePatternMatcher(char quote)
	{
		StartChar = quote;
		EndChar = quote;
	}

	public RangePatternMatcher(char start, char end)
	{
		StartChar = start;
		EndChar = end;
	}

	public RangePatternMatcher(char start, char end, char escape)
	{
		StartChar = start;
		EndChar = end;
		EscapeChar = escape;
	}

	public override int Match(string textToMatch, int matchAtIndex)
	{
		int i = 0;
		for (int length = textToMatch.Length; matchAtIndex + i != length; i++)
		{
			if (textToMatch[matchAtIndex + i] == EndChar && matchAtIndex + i < length - 1 && textToMatch[matchAtIndex + i + 1] == EndChar)
			{
				i++;
			}
			else if (textToMatch[matchAtIndex + i] == EndChar && (matchAtIndex + i == length - 1 || textToMatch[matchAtIndex + i + 1] != EndChar))
			{
				return i + 1;
			}
		}
		return 0;
	}
}
