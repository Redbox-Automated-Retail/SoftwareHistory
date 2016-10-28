using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Alsing.SourceCode;

[ComVisible(true)]
public class PatternComparer : IComparer<Pattern>
{
	public int Compare(Pattern x, Pattern y)
	{
		return y.StringPattern.Length.CompareTo(x.StringPattern.Length);
	}
}
