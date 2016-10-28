using System.Runtime.InteropServices;

namespace Alsing.Text.PatternMatchers;

[ComVisible(true)]
public class BinPatternMatcher : PatternMatcherBase
{
	public static readonly BinPatternMatcher Default = new BinPatternMatcher();

	public override string[] DefaultPrefixes => new string[2] { "0", "1" };

	public override int Match(string textToMatch, int matchAtIndex)
	{
		int num = matchAtIndex;
		do
		{
			char c = textToMatch[num];
			if (c != '0' && c != '1')
			{
				break;
			}
			num++;
		}
		while (num < textToMatch.Length);
		return num - matchAtIndex;
	}
}
