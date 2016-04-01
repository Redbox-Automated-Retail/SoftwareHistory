using System;

namespace Redbox.HAL.Component.Model;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
internal class PollerAttribute : Attribute
{
	public string Name { get; set; }
}
