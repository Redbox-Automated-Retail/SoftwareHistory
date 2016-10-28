using System;

namespace Redbox.HAL.Core;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
internal class StateHandlerAttribute : Attribute
{
	public object State { get; set; }
}
