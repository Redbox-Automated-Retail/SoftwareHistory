using System.Runtime.InteropServices;

namespace Alsing.Text.PatternMatchers;

[ComVisible(true)]
public interface IPatternMatcher
{
	string[] DefaultPrefixes { get; }

	int Match(string textToMatch, int matchAtIndex);
}
