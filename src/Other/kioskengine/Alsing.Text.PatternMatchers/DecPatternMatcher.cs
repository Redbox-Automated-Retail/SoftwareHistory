namespace Alsing.Text.PatternMatchers;

public class DecPatternMatcher : PatternMatcherBase
{
	public static readonly DecPatternMatcher Default = new DecPatternMatcher();

	public override int Match(string textToMatch, int matchAtIndex)
	{
		int num = matchAtIndex;
		bool flag = false;
		do
		{
			char c = textToMatch[num];
			if (c < '0' || c > '9')
			{
				if (c != '.' || flag)
				{
					break;
				}
				flag = true;
			}
			num++;
		}
		while (num < textToMatch.Length);
		return num - matchAtIndex;
	}
}
