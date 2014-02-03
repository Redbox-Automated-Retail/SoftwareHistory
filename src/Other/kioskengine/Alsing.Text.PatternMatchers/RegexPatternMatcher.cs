using System;
using System.Text.RegularExpressions;

namespace Alsing.Text.PatternMatchers;

public class RegexPatternMatcher : PatternMatcherBase
{
	private Regex regEx;

	private string pattern;

	public string Pattern
	{
		get
		{
			return pattern;
		}
		set
		{
			if (!(pattern == value))
			{
				pattern = value;
				OnPatternChanged(EventArgs.Empty);
			}
		}
	}

	public event EventHandler PatternChanged;

	public RegexPatternMatcher()
	{
		PatternChanged = (EventHandler)Delegate.Combine(PatternChanged, new EventHandler(RegexPatternMatcher_PatternChanged));
	}

	public RegexPatternMatcher(string pattern)
		: this()
	{
		Pattern = pattern;
	}

	protected virtual void OnPatternChanged(EventArgs e)
	{
		if (PatternChanged != null)
		{
			PatternChanged(this, e);
		}
	}

	public override int Match(string textToMatch, int matchAtIndex)
	{
		if (regEx == null)
		{
			return 0;
		}
		Match match = regEx.Match(textToMatch, matchAtIndex);
		if (match.Success)
		{
			return match.Length;
		}
		return 0;
	}

	private void RegexPatternMatcher_PatternChanged(object sender, EventArgs e)
	{
		regEx = new Regex($"\\G({pattern})", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);
	}
}
