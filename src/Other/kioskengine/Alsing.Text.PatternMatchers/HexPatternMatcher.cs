namespace Alsing.Text.PatternMatchers;

public class HexPatternMatcher : PatternMatcherBase
{
	public static readonly HexPatternMatcher Default = new HexPatternMatcher();

	public override int Match(string textToMatch, int matchAtIndex)
	{
		int num = matchAtIndex;
		do
		{
			char c = textToMatch[num];
			if ((c < '0' || c > '9') && (c < 'a' || c > 'f') && (c < 'A' || c > 'F'))
			{
				break;
			}
			num++;
		}
		while (num < textToMatch.Length);
		return num - matchAtIndex;
	}
}
