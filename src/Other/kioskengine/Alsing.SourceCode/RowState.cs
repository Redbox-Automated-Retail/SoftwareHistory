using System.Runtime.InteropServices;

namespace Alsing.SourceCode;

[ComVisible(true)]
public enum RowState
{
	NotParsed,
	SpanParsed,
	AllParsed
}
