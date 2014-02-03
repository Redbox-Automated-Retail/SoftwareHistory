using System;
using System.Collections.Generic;

namespace Redbox.WUA;

internal class ErrorList : List<Error>, ICloneable
{
	public bool ContainsCode(string code)
	{
		return Find((Error each) => each.Code == code) != null;
	}

	public int RemoveCode(string code)
	{
		return RemoveAll((Error each) => each.Code == code);
	}

	public object Clone()
	{
		ErrorList errorList = new ErrorList();
		errorList.AddRange(this);
		return errorList;
	}
}
