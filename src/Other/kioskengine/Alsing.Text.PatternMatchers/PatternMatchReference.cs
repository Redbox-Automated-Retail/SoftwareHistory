using System.Runtime.InteropServices;

namespace Alsing.Text.PatternMatchers;

[ComVisible(true)]
public class PatternMatchReference
{
	public IPatternMatcher Matcher;

	public PatternMatchReference NextSibling;

	public object[] Tags;

	public bool NeedSeparators;

	public PatternMatchReference(IPatternMatcher matcher)
	{
		Matcher = matcher;
	}
}
