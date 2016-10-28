using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Alsing.SourceCode;

[ComVisible(true)]
public sealed class UndoBuffer : List<UndoBlockCollection>
{
	public int MaxSize { get; set; }

	public UndoBuffer()
	{
		MaxSize = 1000;
	}

	public void ClearFrom(int index)
	{
		while (index <= base.Count - 1)
		{
			RemoveAt(index);
		}
	}
}
