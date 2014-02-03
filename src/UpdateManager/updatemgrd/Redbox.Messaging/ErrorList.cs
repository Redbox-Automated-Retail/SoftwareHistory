using System;
using System.Collections.Generic;
using System.Text;

namespace Redbox.Messaging;

internal class ErrorList : List<Error>, ICloneable
{
	public int ErrorCount => FindAll((Error each) => !each.IsWarning).Count;

	public int WarningCount => FindAll((Error each) => each.IsWarning).Count;

	public object Clone()
	{
		ErrorList errorList = new ErrorList();
		errorList.AddRange(this);
		return errorList;
	}

	public override string ToString()
	{
		StringBuilder sb = new StringBuilder();
		ForEach(delegate(Error e)
		{
			sb.AppendLine(e.ToString());
		});
		return sb.ToString();
	}
}
