namespace Alsing.Text.PatternMatchers;

public interface IPatternMatcher
{
	string[] DefaultPrefixes { get; }

	int Match(string textToMatch, int matchAtIndex);
}
