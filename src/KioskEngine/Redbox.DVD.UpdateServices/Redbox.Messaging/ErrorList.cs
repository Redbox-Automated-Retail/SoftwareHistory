using System;
using System.Collections.Generic;
using System.Text;

namespace Redbox.Messaging;

internal class ErrorList : List<Redbox.Messaging.Error>, ICloneable
{
	public int ErrorCount => FindAll((Redbox.Messaging.Error each) => !each.IsWarning).Count;

	public int WarningCount => FindAll((Redbox.Messaging.Error each) => each.IsWarning).Count;

	public object Clone()
	{
		Redbox.Messaging.ErrorList errorList = new Redbox.Messaging.ErrorList();
		errorList.AddRange(this);
		return errorList;
	}

	public override string ToString()
	{
		StringBuilder sb = new StringBuilder();
		ForEach(delegate(Redbox.Messaging.Error e)
		{
			sb.AppendLine(e.ToString());
		});
		return sb.ToString();
	}
}
