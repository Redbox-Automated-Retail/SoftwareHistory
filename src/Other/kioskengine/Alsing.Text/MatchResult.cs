namespace Alsing.Text;

public struct MatchResult
{
	public bool Found;

	public int Index;

	public int Length;

	public object[] Tags;

	public string Text;

	public static MatchResult NoMatch => new MatchResult
	{
		Found = false
	};

	public override string ToString()
	{
		if (!Found)
		{
			return "no match";
		}
		if (Tags != null)
		{
			return string.Concat(Tags, "  ", Index, "  ", Length);
		}
		return "MatchResult";
	}

	public string GetText()
	{
		if (Text != null)
		{
			return Text.Substring(Index, Length);
		}
		return "";
	}
}
