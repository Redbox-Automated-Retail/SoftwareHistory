using System;

namespace Alsing.SourceCode;

public class RowEventArgs : EventArgs
{
	public Row Row;

	public RowEventArgs(Row row)
	{
		Row = row;
	}
}
