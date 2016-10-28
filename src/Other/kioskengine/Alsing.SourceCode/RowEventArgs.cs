using System;
using System.Runtime.InteropServices;

namespace Alsing.SourceCode;

[ComVisible(true)]
public class RowEventArgs : EventArgs
{
	public Row Row;

	public RowEventArgs(Row row)
	{
		Row = row;
	}
}
