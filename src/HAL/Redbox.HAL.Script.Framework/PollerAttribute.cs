using System;

namespace Redbox.HAL.Script.Framework;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class PollerAttribute : Attribute
{
	public string Name { get; set; }
}
