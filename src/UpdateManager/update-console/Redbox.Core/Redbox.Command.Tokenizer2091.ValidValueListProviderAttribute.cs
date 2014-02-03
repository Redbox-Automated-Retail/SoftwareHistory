using System;

namespace Redbox.Core;

internal class Redbox_002ECommand_002ETokenizer2091_002EValidValueListProviderAttribute : Attribute
{
	public string MethodName { get; private set; }

	public Redbox_002ECommand_002ETokenizer2091_002EValidValueListProviderAttribute(string methodName)
	{
		MethodName = methodName;
	}
}
