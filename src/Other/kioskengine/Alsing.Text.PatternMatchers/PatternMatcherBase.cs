namespace Alsing.Text.PatternMatchers;

public abstract class PatternMatcherBase : IPatternMatcher
{
	public virtual string[] DefaultPrefixes => null;

	public virtual int Match(string textToMatch, int matchAtIndex)
	{
		return 0;
	}
}
