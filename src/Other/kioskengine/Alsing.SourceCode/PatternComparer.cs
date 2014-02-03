using System.Collections.Generic;

namespace Alsing.SourceCode;

public class PatternComparer : IComparer<Pattern>
{
	public int Compare(Pattern x, Pattern y)
	{
		return y.StringPattern.Length.CompareTo(x.StringPattern.Length);
	}
}
