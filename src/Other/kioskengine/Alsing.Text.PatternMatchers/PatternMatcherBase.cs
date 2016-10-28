using System.Runtime.InteropServices;

namespace Alsing.Text.PatternMatchers;

[ComVisible(true)]
public abstract class PatternMatcherBase : IPatternMatcher
{
	public virtual string[] DefaultPrefixes => null;

	public virtual int Match(string textToMatch, int matchAtIndex)
	{
		return 0;
	}
}
