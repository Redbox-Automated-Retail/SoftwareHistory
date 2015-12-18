using System;

namespace Redbox.Core;

internal class Redbox_002EMacros1955_002EValidValueListProviderAttribute : Attribute
{
	public string MethodName { get; private set; }

	public Redbox_002EMacros1955_002EValidValueListProviderAttribute(string methodName)
	{
		MethodName = methodName;
	}
}
