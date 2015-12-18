using System;

namespace Redbox.HAL.Configuration;

internal class ValidValueListProviderAttribute : Attribute
{
	public string MethodName { get; private set; }

	public ValidValueListProviderAttribute(string methodName)
	{
		MethodName = methodName;
	}
}
