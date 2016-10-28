using System.Runtime.InteropServices;

namespace Alsing.Text.PatternMatchers;

[ComVisible(true)]
public class IntPatternMatcher : PatternMatcherBase
{
	public static readonly IntPatternMatcher Default = new IntPatternMatcher();

	public override int Match(string textToMatch, int matchAtIndex)
	{
		int num = matchAtIndex;
		do
		{
			char c = textToMatch[num];
			if (c < '0' || c > '9')
			{
				break;
			}
			num++;
		}
		while (num < textToMatch.Length);
		return num - matchAtIndex;
	}
}
